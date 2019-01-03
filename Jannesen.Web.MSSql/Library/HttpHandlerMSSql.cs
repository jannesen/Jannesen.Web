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
        private                 string                      _procedure;
        private                 string                      _database;
        private                 int                         _timeout;
        private                 ParameterList               _parameters;
        private                 bool                        _mapTo200;

        public      override    bool                        MapTo200
        {
            get {
                return _mapTo200;
            }
        }

        public                                              HttpHandlerMSSql(WebCoreConfigReader configReader): base(configReader)
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public       override   WebCoreResponse             Process(WebCoreCall httpCall)
        {
            int retry_count = 0;

retry:      using (SqlConnection sqlConnection = GetConnection())
            {
                using (SqlCommand sqlCommand = new SqlCommand(_procedure, sqlConnection))
                {
                    sqlCommand.CommandType    = CommandType.StoredProcedure;
                    sqlCommand.CommandTimeout = _timeout;
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
        public      override    int                         ProcessErrorCode(Exception err, ref string code)
        {
            if (err is SqlException) {
                string      msg = err.Message;
                int         i;

                if (msg.Length > 2 && msg[0] == '[' && (i = msg.IndexOf("] ")) > 0) {
                    code = msg.Substring(1, i - 1);
                }
                else {
                    switch(((SqlException)err).Number) {
                    case -2:    code = "DATABASE-TIMEOUT";      break;
                    default:    code = "DATABASE-ERROR";        break;
                    }
                }

                switch(code) {
                case "INVALID-BASIC-AUTHENTICATION":        return 401;
                default:                                    return 500;
                }
            }

            if (err is NoDataException) {
                code = "NO-DATA";
                return 500;
            }

            return 0;
        }

        protected   virtual     WebCoreResponse             Process(WebCoreCall httpCall, SqlCommand sqlCommand)
        {
            using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
            {
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
        protected               HttpStatusCode              HandleResponseOptions(WebCoreResponseBuffer webResponseBuffer, SqlDataReader dataReader)
        {
            try {
                if (dataReader.FieldCount>0 && dataReader.GetName(0).StartsWith("opt.")) {
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
