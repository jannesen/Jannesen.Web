using System;
using System.Data.SqlClient;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.MSSql
{
    [WebCoreAttribureResource("mssql")]
    public class ResourceMSSqlDatabase: WebCoreResource
    {
        private                 string              _server;
        private                 string              _instance;
        private                 string              _database;
        private                 string              _username;
        private                 string              _passwd;
        private                 string              _connectString;

        public      override    string              Type
        {
            get {
                return "mssql";
            }
        }

        public                  string              Server
        {
            get {
                return _server;
            }
        }
        public                  string              Instance
        {
            get {
                return _instance;
            }
        }
        public                  string              Database
        {
            get {
                return _database;
            }
        }
        public                  string              Username
        {
            get {
                return _username;
            }
        }
        public                  string              Passwd
        {
            get {
                return _passwd;
            }
        }

        public                                      ResourceMSSqlDatabase(WebCoreConfigReader configReader): base(configReader)
        {
            _server   = configReader.GetValueString("server");
            _instance = configReader.GetValueString("instance", null);
            _database = configReader.GetValueString("database");
            _username = configReader.GetValueString("username", null);
            _passwd   = (_username != null) ? configReader.GetValueString("passwd") : null;

            _connectString = "Server="                        + (!string.IsNullOrEmpty(_instance) ? _server+"\\"+_instance : _server) +
                             ";Database="                     + _database +
                             ";Current Language=us_english"   +
                             ";Connection Reset=false"        +
                             ";Connect Timeout=15"            +
                             ";Application Name=Jannesen.Web";

            if (!string.IsNullOrEmpty(_username)) {
                _connectString += ";User ID=" + _username +
                                  ";Pwd="     + _passwd;
            }
            else
                _connectString += ";Trusted_Connection=true";
        }

        public                  string              GetConnectString()
        {
            return _connectString;
        }
        public                  SqlConnection       GetConnection()
        {
            SqlConnection   sqlConnection = new SqlConnection(GetConnectString());

            try {
                sqlConnection.Open();
                return sqlConnection;
            }
            catch(Exception) {
                sqlConnection.Dispose();
                throw;
            }
        }
    }
}
