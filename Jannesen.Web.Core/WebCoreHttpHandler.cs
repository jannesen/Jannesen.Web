using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Jannesen.Web.Core;

public abstract class WebCoreHttpHandler
{
    private readonly        string                              _path;
    private readonly        string                              _verb;
    private readonly        List<IWebCoreProcessor>             _processors;
    private readonly        WebCoreWildcardPathProcessor?       _wildcardPathProcessor;
    private readonly        WebCoreErrorHandler                 _errorHandler;
    private readonly        ResourceLogging?                    _logging;

    public                  string                              Path                    => _path;
    public                  string                              Verb                    => _verb;
    public                  IReadOnlyList<IWebCoreProcessor>    Processors              => _processors;
    internal                WebCoreWildcardPathProcessor?       WildcardPathProcessor   => _wildcardPathProcessor;
    public                  ResourceLogging?                    Logging                 => _logging;
    public      virtual     string?                             Mimetype                => null;

    protected                                                   WebCoreHttpHandler(WebCoreConfigReader configReader)
    {
        ArgumentNullException.ThrowIfNull(configReader);

        _path         = configReader.GetValuePathName("path");
        _verb         = string.Intern(configReader.GetValueString("verb", "GET")!.ToUpperInvariant());
        _processors   = new List<IWebCoreProcessor>();
        _errorHandler = WebLoader.Instance.GetErrorHandler(configReader.GetValueString("error-handler", null));

        var logging = configReader.GetValueString("logging", null);

        if (logging != null)
            _logging = configReader.ApplicationConfig.GetResource<ResourceLogging>(logging);

        _wildcardPathProcessor = WebCoreWildcardPathProcessor.GetProcessor(_path);
    }

    public      virtual     Task                                ProcessRequestAsync(WebApplicationConfig applicationConfig, HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(applicationConfig);
        ArgumentNullException.ThrowIfNull(context);

        return _processRequest(applicationConfig, context);
    }

    public      abstract    Task<IWebCoreResponse>              ProcessAsync(WebCoreCall httpCall);
    public      virtual     WebCoreErrorData?                   ProcessError(Exception err)
    {
        return null;
    }

    public                  IWebCoreDataSource                  GetDataSource(string source)
    {
        var ds = WebLoader.Instance.GetDataSource(source);

        foreach(var p in ds.GetRequiredProcessors()) {
            _addProcessor(p);
        }

        return ds;
    }

    private                 void                                _addProcessor(IWebCoreProcessor processor)
    {
        foreach(var p in _processors) {
            if (object.ReferenceEquals(p, processor)) {
                return ;
            }
        }

        foreach(var p in processor.GetRequiredProcessors()) {
            _addProcessor(p);
        }

        _processors.Add(processor);
    }
    private           async Task                                _processRequest(WebApplicationConfig applicationConfig, HttpContext context)
    {
        var                 httpCall    = new WebCoreCall(applicationConfig, context, this);
        IWebCoreResponse    webResponse;

        try {
            await httpCall.LoadProcessorsAsync();
            webResponse = await ProcessAsync(httpCall);
        }
        catch(WebHttpException err) {
            switch(err.StatusCode) {
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.BadRequest:
            case HttpStatusCode.RequestTimeout:
            case HttpStatusCode.RequestEntityTooLarge:
            case HttpStatusCode.InternalServerError:
            case HttpStatusCode.NotImplemented:
            case HttpStatusCode.BadGateway:
            case HttpStatusCode.ServiceUnavailable:
            case HttpStatusCode.GatewayTimeout:
                webResponse = _errorHandler(this, err);
                break;

            default:
                if (_logging != null) {
                    await _logging.LoggingAsync(httpCall, err);
                }
                throw;
            }
        }
        catch(TaskCanceledException) {
            throw;
        }
        catch(Exception err) {
            if (!(err is WebException werr && werr.logError == false)) {
                applicationConfig.Application.LogError("Error in handler: " + Path + " Url: " + httpCall.Request.GetDisplayUrl(), err);
            }

            webResponse = _errorHandler(this, err);
        }

        if (webResponse != null) {
            await webResponse.SendAsync(httpCall, context.Response);
            if (_logging != null) {
                await _logging.LoggingAsync(httpCall, webResponse, context.Response);
            }
        }
    }
}
