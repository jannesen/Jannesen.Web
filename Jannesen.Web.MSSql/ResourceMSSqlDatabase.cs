using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Jannesen.Web.Core;

namespace Jannesen.Web.MSSql;

[WebCoreResource("mssql")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
public class ResourceMSSqlDatabase: WebCoreResource
{
    private readonly        string                  _server;
    private readonly        string?                 _instance;
    private readonly        string                  _database;
    private readonly        bool                    _iisUserIdentity;
    private readonly        string?                 _username;
    private readonly        string?                 _passwd;
    private readonly        string                  _connectString;

    public      override    string                  Type                => "mssql";
    public                  string                  Server              => _server;
    public                  string?                 Instance            => _instance;
    public                  string                  Database            => _database;
    public                  bool                    IISUserIdentityswd  => _iisUserIdentity;
    public                  string?                 Username            => _username;
    public                  string?                 Passwd              => _passwd;


    public                                          ResourceMSSqlDatabase(WebCoreConfigReader configReader): base(configReader)
    {
        ArgumentNullException.ThrowIfNull(configReader);

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

    public                  Task<SqlConnection>     GetConnectionAsync(WebCoreCall httpCall, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(httpCall);

        return _getConnectionAsync(_iisUserIdentity
                                        ? httpCall.Context.User.Identity as WindowsIdentity
                                              ?? throw new WebHttpException(HttpStatusCode.Unauthorized, "No windows Identity available.")
                                        : null,
                                   ct);
    }
    public                  Task<SqlConnection>     GetConnectionAsync(CancellationToken ct)
    {
        return _getConnectionAsync(null, ct);
    }
    private           async Task<SqlConnection>     _getConnectionAsync(WindowsIdentity? identity, CancellationToken ct)
    {
        var sqlConnection = new SqlConnection(_connectString);

        try {
            if (identity != null) {
                await WindowsIdentity.RunImpersonatedAsync(identity.AccessToken, ()=> sqlConnection.OpenAsync(ct));
            }
            else {
                await sqlConnection.OpenAsync(ct);
            }

            return sqlConnection;
        }
        catch(Exception) {
            sqlConnection.Dispose();
            throw;
        }
    }
}
