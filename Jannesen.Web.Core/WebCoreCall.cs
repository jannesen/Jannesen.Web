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
        public IWebCoreProcessorDefinition     Definition   { get; init; }
        public object                          Instance     { get; init; }
    }

    private readonly        WebApplicationConfig                _applicationConfig;
    private readonly        DateTime                            _timestamp;
    private readonly        HttpContext                         _context;
    private readonly        HttpRequest                         _request;
    private readonly        WebCoreHttpHandler                  _handler;
    private readonly        List<ProcessorInstance>             _processors;

    public                  WebApplicationConfig                ApplicationConfig   => _applicationConfig;
    public                  DateTime                            Timestamp           => _timestamp;
    public                  HttpContext                         Context             => _context;
    public                  HttpRequest                         Request             => _request;
    public                  IMemoryCache                        Cache               => _applicationConfig.Application.Cache;
    public                  WebCoreHttpHandler                  Handler             => _handler;
    public                  string                              HttpMethod          => _request.Method;
    public                  string?                             RequestContentType  => _request.ContentType;
    public                  int?                                RequestContentLength
    {
        get {
            var s = GetHeader("Content-Length");

            if (!string.IsNullOrEmpty(s)) {
                if (int.TryParse(s, out var rtn)) {
                    return rtn;
                }
            }

            return null;
        }
    }
    public                  DateTime?                           RequestIfModifiedSince
    {
        get {
            var s = GetHeader("If-Modified-Since");

            if (!string.IsNullOrEmpty(s)) {
                if (DateTime.TryParseExact(s, "R", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.AdjustToUniversal, out var rtn))
                    return rtn;
            }

            return null;
        }
    }
    public                  string?                             RequestIfNoneMatch
    {
        get {
            var s = GetHeader("If-None-Match");

            return !string.IsNullOrEmpty(s) ? s : null;
        }
    }
    public                  string?                             RequestReferer
    {
        get {
            var s = GetHeader("Referer");

            return !string.IsNullOrEmpty(s) ? s : null;
        }
    }
    public                  string?                             RequestUserAgent
    {
        get {
            var s = GetHeader("User-Agent");

            return !string.IsNullOrEmpty(s) ? s : null;
        }
    }
    public                  string?                             RequestRemoteAddr
    {
        get {
            var rtn = GetHeader("X-Forwarded-For");

            if (!string.IsNullOrEmpty(rtn)) {
                var p = rtn.IndexOf(',', StringComparison.Ordinal);
                if (p > 0) {
                    rtn = rtn.Substring(0, p);
                }
            }
            else {
                rtn = _context.Connection.RemoteIpAddress?.ToString();
            }

            return !string.IsNullOrEmpty(rtn) ? rtn : null;
        }
    }
    public                  WebCoreProcessorBody                RequestBody                 => GetProcessor<WebCoreProcessorBody>(WebCoreProcessorBody.Definition);
    public                  WebCoreProcessorBasicAutorization   RequestBasicAutorization    => GetProcessor<WebCoreProcessorBasicAutorization>(WebCoreProcessorBasicAutorization.Definition);
    public                  WebCoreProcessorUrlPathData         RequestUrlPathData          => GetProcessor<WebCoreProcessorUrlPathData>(WebCoreProcessorUrlPathData.Definition);
    public                  WebCoreProcessorTextXml             RequestTextXml              => GetProcessor<WebCoreProcessorTextXml>(WebCoreProcessorTextXml.Definition);
    public                  WebCoreProcessorTextJson            RequestTextJson             => GetProcessor<WebCoreProcessorTextJson>(WebCoreProcessorTextJson.Definition);

    public                                                      WebCoreCall(WebApplicationConfig applicationConfig, HttpContext httpCall, WebCoreHttpHandler handler)
    {
        ArgumentNullException.ThrowIfNull(httpCall);

        _applicationConfig = applicationConfig;
        _timestamp         = DateTime.UtcNow;
        _context           = httpCall;
        _request           = httpCall.Request;
        _handler           = handler;
        _processors        = new List<ProcessorInstance>();
    }

    public                  string?                             GetHeader(string name)
    {
        var h = _request.Headers[name];
        switch(h.Count) {
        case 0: return null;
        case 1: return h[0];
        default: throw new WebHttpException(HttpStatusCode.BadRequest, "Multiple '" + name + "' headers.");
        }
    }
    public                  T                                   GetProcessor<T>(IWebCoreProcessorDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        foreach(var p in _processors) {
            if (p.Definition == definition) {
                return (T)p.Instance;
            }
        }

        var newInstance = definition.Create(this);
        _processors.Add(new ProcessorInstance() {
                            Definition = definition,
                            Instance   = newInstance
                        });

        return (T)newInstance;
    }
    public                  T?                                  GetProcessorOptional<T>(IWebCoreProcessorDefinition definition)
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
