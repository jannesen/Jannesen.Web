using System;
using System.Net;
using System.Security.Principal;
using Microsoft.Data.SqlClient;
using Jannesen.Web.Core;
using Jannesen.Web.Core.Impl;

#pragma warning disable CA1416 // Validate platform compatibility

namespace Jannesen.Web.MSSql
{
    [WebCoreAttribureResource("mssql")]
    public class ResourceMSSqlDatabase: WebCoreResource
    {
        private readonly        string              _server;
        private readonly        string              _instance;
        private readonly        string              _database;
        private readonly        bool                _iisUserIdentity;
        private readonly        string              _username;
        private readonly        string              _passwd;
        private readonly        string              _connectString;

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
        public                  bool                IISUserIdentityswd
        {
            get {
                return _iisUserIdentity;
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
            _server          = configReader.GetValueString("server");
            _instance        = configReader.GetValueString("instance", null);
            _database        = configReader.GetValueString("database");
            _iisUserIdentity = configReader.GetValueBool("iis-user-identity", false);
            _username        = (!_iisUserIdentity) ? configReader.GetValueString("username", null) : null;
            _passwd          = (_username != null) ? configReader.GetValueString("passwd") : null;

            _connectString = "Server="                        + (!string.IsNullOrEmpty(_instance) ? _server+"\\"+_instance : _server) +
                             ";Database="                     + _database +
                             ";Current Language=us_english"   +
                             ";Connect Timeout=15"            +
                             ";Application Name=Jannesen.Web" +
                             ";TrustServerCertificate=True";

            if (!string.IsNullOrEmpty(_username)) {
                _connectString += ";User ID=" + _username +
                                  ";Pwd="     + _passwd;
            }
            else
                _connectString += ";Integrated Security=true";
        }

        public                  string              GetConnectString()
        {
            return _connectString;
        }

        public                  SqlConnection       GetConnection(WebCoreCall httpCall)
        {
            if (_iisUserIdentity) {
                var windowsIdentity = httpCall.Context.User.Identity as WindowsIdentity
                                        ?? throw new WebHttpException(HttpStatusCode.Unauthorized, "No windows Identity available.");

                return WindowsIdentity.RunImpersonated<SqlConnection>(windowsIdentity.AccessToken, ()=> GetConnection());
            }
            else {
                return GetConnection();
            }
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
