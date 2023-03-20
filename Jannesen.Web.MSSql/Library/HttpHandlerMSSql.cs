using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Jannesen.Web.Core;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.MSSql.Library
{
    public abstract class HttpHandlerMSSql: WebCoreHttpHandler
    {
        private readonly        string                      _procedure;
        private readonly        string                      _database;
        private readonly        int                         _timeout;
        private readonly        ParameterList               _parameters;
        private readonly        bool                        _mapTo200;

        public      override    bool                        MapTo200
        {
            get {
                return _mapTo200;
            }
        }

        protected                                           HttpHandlerMSSql(WebCoreConfigReader configReader): base(configReader)
        {
            _procedure       = configReader.GetValueString("procedure");
            _timeout         = configReader.GetValueInt("timeout", 30, 5, 300);
            _database        = configReader.GetValueString("database");
            _mapTo200        = configReader.GetValueBool("mapto200", false);
            _parameters      = new ParameterList();
        }

        protected               void                        ParseParameter(WebCoreConfigReader configReader)
        {
            string  type     = configReader.GetValueString  ("type");

            Parameter parameter;

            parameter = new ParameterValue(type, configReader);

            _parameters.Add(parameter);
        }

        public       override   WebCoreResponse             Process(WebCoreCall httpCall)
        {
            int retry_count = 0;

retry:      using (SqlConnection sqlConnection = GetConnection())
            {
                using (SqlCommand sqlCommand = new SqlCommand(_procedure, sqlConnection) {CommandType = CommandType.StoredProcedure, CommandTimeout = _timeout } ) {
                    _parameters.AddParametersToCommand(sqlCommand, httpCall);

                    try {
                        return Process(httpCall, sqlCommand);
                    }
                    catch(Exception err) {
                        if (retry_count <= 3 && _deadlockError(err)) {
                            Jannesen.Web.Core.WebApplication.LogEvent(WebApplication.EventID.DeadLockWarning, "Deadlock on "+ sqlCommand.CommandText);
                            ++retry_count;
                            System.Threading.Thread.Sleep(150);
                            goto retry;
                        }

                        throw;
                    }
                }
            }
        }
        public      override    int                         ProcessErrorCode(Exception err, out string code, out string message)
        {
            if (err is SqlException) {
                string      msg = err.Message;
                int         i;

                if (msg.Length > 2 && msg[0] == '[' && msg[msg.Length - 1] == ']') {
                    code     = msg.Substring(1, msg.Length - 2);
                    message  = code;
                }
                else if (msg.Length > 2 && msg[0] == '[' && (i = msg.IndexOf("] ", StringComparison.Ordinal)) > 0) {
                    code     = msg.Substring(1, i - 1);
                    message  = msg.Substring(i+2);
                }
                else {
                    switch(((SqlException)err).Number) {
                    case -2:
                        code    = "DATABASE-TIMEOUT";
                        message = "Database timeout";
                        break;
                    default:
                        code    = "DATABASE-ERROR";
                        message = msg;
                        break;
                    }
                }

                switch(code) {
                case "INVALID-AUTHENTICATION":              return 401;
                case "INVALID-BASIC-AUTHENTICATION":        return 401;
                case "NOT-FOUND":                           return 404;
                case "HTTP-401":                            return 401;
                case "HTTP-404":                            return 404;
                default:                                    return 500;
                }
            }

            if (err is NoDataException) {
                code    = "NO-DATA";
                message = "No data retrieved";
                return 500;
            }

            code    = null;
            message = null;
            return 0;
        }

        protected   virtual     WebCoreResponse             Process(WebCoreCall httpCall, SqlCommand sqlCommand)
        {
            using (SqlDataReader dataReader = sqlCommand.ExecuteReader()) {
                return Process(httpCall, dataReader);
            }
        }
        protected   virtual     WebCoreResponse             Process(WebCoreCall httpCall, SqlDataReader dataReader)
        {
            throw new NotImplementedException("Not implemented HttpHandlerMSSql.Process");
        }

        protected               SqlConnection               GetConnection()
        {
            return WebApplication.GetResource<ResourceMSSqlDatabase>(_database).GetConnection();
        }
        protected   static      HttpStatusCode              HandleResponseOptions(WebCoreResponseBuffer webResponseBuffer, SqlDataReader dataReader)
        {
            try {
                if (dataReader.FieldCount>0 && dataReader.GetName(0).StartsWith("opt.", StringComparison.Ordinal)) {
                    if (dataReader.Read()) {
                        for (int i = 0 ; i < dataReader.FieldCount ; ++i) {
                            string fieldname = dataReader.GetName(i);

                            switch(fieldname) {
                            case "opt.cache.lastmodified":
                                webResponseBuffer.LastModified = dataReader.GetDateTime(i);
                                break;

                            case "opt.cache.maxage":
                                webResponseBuffer.CacheMaxAge = dataReader.GetInt32(i);
                                break;

                            case "opt.statuscode":
                                webResponseBuffer.StatusCode = (HttpStatusCode)dataReader.GetInt32(i);
                                break;

                            case "opt.etag":
                                webResponseBuffer.ETag = dataReader.GetString(i);
                                break;

                            case "opt.disposition":
                                webResponseBuffer.Disposition = dataReader.GetString(i);
                                break;

                            default:
                                throw new InternalErrorException("Unknown option \'"+fieldname+"\' received from database.");
                            }
                        }
                    }

                    dataReader.NextResult();
                }
            }
            catch(Exception err) {
                throw new WebResponseException("Setting HTTP-OPTIONS failed.", err);
            }

            return webResponseBuffer.StatusCode;
        }

        private     static      bool                        _deadlockError(Exception err)
        {
            while (err.InnerException != null)
                err = err.InnerException;

            return err is SqlException &&  ((SqlException)err).Number == 1205;
        }
    }
}
