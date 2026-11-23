using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Jannesen.Web.Core;

public class WebCoreCall
{
    readonly struct ProcessorInstance
    {
        public object   Definition   { get; init; }
        public object   Instance     { get; init; }
    }

    private readonly        WebApplicationConfig                _applicationConfig;
    private readonly        DateTime                            _timestamp;
    private readonly        HttpContext                         _context;
    private readonly        WebCoreHttpHandler                  _handler;
    private readonly        List<ProcessorInstance>             _processors;

    public                  WebApplicationConfig                ApplicationConfig   => _applicationConfig;
    public                  DateTime                            Timestamp           => _timestamp;
    public                  HttpContext                         Context             => _context;
    public                  HttpRequest                         Request             => _context.Request;
    public                  IMemoryCache                        Cache               => _applicationConfig.Application.Cache;
    public                  WebCoreHttpHandler                  Handler             => _handler;
    public                  string                              HttpMethod          => _context.Request.Method;
    public                  WebCoreProcessResultRequestBody     RequestBody         => GetProcessorResult(WebCoreProcessorRequestBody.Definition);

    public                                                      WebCoreCall(WebApplicationConfig applicationConfig, HttpContext httpCall, WebCoreHttpHandler handler)
    {
        ArgumentNullException.ThrowIfNull(httpCall);

        _applicationConfig = applicationConfig;
        _timestamp         = DateTime.UtcNow;
        _context           = httpCall;
        _handler           = handler;
        _processors        = new List<ProcessorInstance>();
    }

    public                  string?                             GetHeader(string name)
    {
        var h = _context.Request.Headers[name];
        switch(h.Count) {
        case 0: return null;
        case 1: return h[0];
        default: throw new WebHttpException(HttpStatusCode.BadRequest, "Multiple '" + name + "' headers.");
        }
    }
    public                  T                                   GetProcessorResult<T>(IWebCoreProcessor<T> definition) where T : class
    {
        ArgumentNullException.ThrowIfNull(definition);

        foreach(var p in _processors) {
            if (object.ReferenceEquals(p.Definition, definition)) {
                return (T)p.Instance;
            }
        }

        var newInstance = definition.Process(this);
        _processors.Add(new ProcessorInstance() {
                            Definition = definition,
                            Instance   = newInstance
                        });

        return newInstance;
    }
    public                  T?                                  GetProcessorResultOptional<T>(IWebCoreProcessor<T> definition) where T:class
    {
        ArgumentNullException.ThrowIfNull(definition);

        foreach(var p in _processors) {
            if (p.Definition == definition) {
                return (T)p.Instance;
            }
        }

        return default;
    }
}
