using System;
using System.Net;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Jannesen.Web.Core;

namespace Jannesen.Web.MSSql.Library;

public abstract class HttpHandlerMSSql: WebCoreHttpHandler
{
    private readonly        string                          _procedure;
    private readonly        string                          _database;
    private readonly        int                             _timeout;
    private readonly        ParameterList                   _parameters;

    protected                                               HttpHandlerMSSql(WebCoreConfigReader configReader): base(configReader)
    {
        ArgumentNullException.ThrowIfNull(configReader);

        _procedure       = configReader.GetValueString("procedure");
        _timeout         = configReader.GetValueInt("timeout", 30, 5, 300);
        _database        = configReader.GetValueString("database");
        _parameters      = new ParameterList();
    }

    protected               void                            ParseParameter(WebCoreConfigReader configReader)
    {
        ArgumentNullException.ThrowIfNull(configReader);

        _parameters.Add(new ParameterValue(this, configReader));
    }

    public     override   async Task<IWebCoreResponse>      ProcessAsync(WebCoreCall httpCall)
    {
        ArgumentNullException.ThrowIfNull(httpCall);
        var retry_count = 0;

        var ct = httpCall.RequestAborted;

retry:  using (var sqlConnection = await GetConnectionAsync(httpCall, ct)) {
            using (var sqlCommand = new SqlCommand(_procedure, sqlConnection) {CommandType = CommandType.StoredProcedure, CommandTimeout = _timeout } ) {
                _parameters.AddParametersToCommand(sqlCommand, httpCall);

                try {
                    return await ProcessAsync(httpCall, sqlCommand, ct);
                }
                catch(Exception err) {
                    if (retry_count <= 3 && _deadlockError(err)) {
                        httpCall.ApplicationConfig.Application.LogWarning("Deadlock on "+ sqlCommand.CommandText);
                        ++retry_count;
                        await Task.Delay(150, ct);
                        goto retry;
                    }

                    throw;
                }
            }
        }
    }
    public     override         WebCoreErrorData?           ProcessError(Exception err)
    {
        if (err is SqlException sqlErr) {
            var msg = err.Message;
            int i;

            if (msg.Length > 2 && msg[0] == '[' && msg[^1] == ']') {
                return _createErrorData(msg.Substring(1, msg.Length - 2), msg.Substring(1, msg.Length - 2));
            }

            if (msg.Length > 2 && msg[0] == '[' && (i = msg.IndexOf("] ", StringComparison.Ordinal)) > 0) {
                return _createErrorData(msg.Substring(1, i - 1), msg.Substring(i+2));
            }

            if (sqlErr.Number == -2) {
                return new WebCoreErrorData() {
                            StatusCode = HttpStatusCode.InternalServerError,
                            Code       = "DATABASE-TIMEOUT",
                            Message    = "Database timeout"
                        };
            }

            return new WebCoreErrorData() {
                        StatusCode = HttpStatusCode.InternalServerError,
                        Code       = "DATABASE-ERROR",
                        Message    = msg
                    };
        }

        if (err is NoDataException) {
            return new WebCoreErrorData() {
                        StatusCode = HttpStatusCode.InternalServerError,
                        Code       = "NO-DATA",
                        Message    = "No data retrieved"
                    };
        }

        return null;
    }

    protected  virtual    async Task<IWebCoreResponse>      ProcessAsync(WebCoreCall httpCall, SqlCommand sqlCommand, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(httpCall);
        ArgumentNullException.ThrowIfNull(sqlCommand);

        using (var dataReader = await sqlCommand.ExecuteReaderAsync(ct)) {
            return await ProcessAsync(httpCall, dataReader, ct);
        }
    }
    protected  virtual    async Task<IWebCoreResponse>      ProcessAsync(WebCoreCall httpCall, SqlDataReader dataReader, CancellationToken ct)
    {
        throw new NotImplementedException("Not implemented HttpHandlerMSSql.Process");
    }

    protected                   Task<SqlConnection>         GetConnectionAsync(WebCoreCall httpCall, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(httpCall);

        return httpCall.ApplicationConfig.GetResource<ResourceMSSqlDatabase>(_database).GetConnectionAsync(httpCall, ct);
    }
    protected  static     async Task<HttpStatusCode>        HandleResponseOptionsAsync(WebCoreResponseBuffer webResponseBuffer, SqlDataReader dataReader, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(webResponseBuffer);
        ArgumentNullException.ThrowIfNull(dataReader);

        try {
            if (dataReader.FieldCount>0 && dataReader.GetName(0).StartsWith("opt.", StringComparison.Ordinal)) {
                if (await dataReader.ReadAsync(ct)) {
                    for (var i = 0 ; i < dataReader.FieldCount ; ++i) {
                        var fieldname = dataReader.GetName(i);

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

                await dataReader.NextResultAsync(ct);
            }
        }
        catch(Exception err) {
            throw new WebResponseException("Setting HTTP-OPTIONS failed.", err);
        }

        return webResponseBuffer.StatusCode;
    }

    private    static           bool                        _deadlockError(Exception err)
    {
        while (err.InnerException != null)
            err = err.InnerException;

        return err is SqlException sqlErr &&  sqlErr.Number == 1205;
    }
    private    static           WebCoreErrorData            _createErrorData(string code, string message)
    {
        return new WebCoreErrorData() {
                    StatusCode = code switch {
                                        "REQUEST-ERROR"                   => HttpStatusCode.BadRequest,
                                        "INVALID-AUTHENTICATION"          => HttpStatusCode.Unauthorized,
                                        "INVALID-BASIC-AUTHENTICATION"    => HttpStatusCode.Unauthorized,
                                        "NOT-FOUND"                       => HttpStatusCode.NotFound,
                                        "HTTP-401"                        => HttpStatusCode.Unauthorized,
                                        "HTTP-404"                        => HttpStatusCode.NotFound,
                                        _                                 => HttpStatusCode.InternalServerError
                                },
                    Code       = code,
                    Message    = message
                };
    }
}
