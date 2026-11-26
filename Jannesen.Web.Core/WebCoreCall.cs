using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Jannesen.Web.Core;

public class WebCoreCall
{
    readonly struct ProcessorResult
    {
        public IWebCoreProcessor    Processor       { get; init; }
        public object               Result          { get; init; }
    }

    private readonly        WebApplicationConfig                _applicationConfig;
    private readonly        DateTime                            _timestamp;
    private readonly        HttpContext                         _context;
    private readonly        WebCoreHttpHandler                  _handler;
    private readonly        List<ProcessorResult>               _processorResult;

    public                  WebApplicationConfig                ApplicationConfig   => _applicationConfig;
    public                  DateTime                            Timestamp           => _timestamp;
    public                  HttpContext                         Context             => _context;
    public                  HttpRequest                         Request             => _context.Request;
    public                  CancellationToken                   RequestAborted      => _context.RequestAborted;
    public                  IMemoryCache                        Cache               => _applicationConfig.Application.Cache;
    public                  WebCoreHttpHandler                  Handler             => _handler;
    public                  string                              HttpMethod          => _context.Request.Method;
    public                  WebCoreProcessResultRequestBody     RequestBody         => GetProcessorResult(WebCoreProcessorRequestBody.Processor);

    public                                                      WebCoreCall(WebApplicationConfig applicationConfig, HttpContext httpCall, WebCoreHttpHandler handler)
    {
        ArgumentNullException.ThrowIfNull(httpCall);

        _applicationConfig = applicationConfig;
        _timestamp         = DateTime.UtcNow;
        _context           = httpCall;
        _handler           = handler;
        _processorResult        = new List<ProcessorResult>();
    }

    public           async ValueTask                            LoadProcessorsAsync()
    {
        foreach(var processor in _handler.Processors) {
            _processorResult.Add(new ProcessorResult() {
                                     Processor = processor,
                                     Result    = await processor.ProcessAsync(this)
                                 });

        }
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
    public                  T                                   GetProcessorResult<T>(IWebCoreProcessor<T> processor) where T : class
    {
        return GetProcessorResultOptional(processor) ??
                    throw new InvalidOperationException($"processor '{processor}' instance not created.");
    }
    public                  T?                                  GetProcessorResultOptional<T>(IWebCoreProcessor<T> definition) where T:class
    {
        ArgumentNullException.ThrowIfNull(definition);

        foreach(var p in _processorResult) {
            if (object.ReferenceEquals(p.Processor, definition)) {
                return (T)p.Result;
            }
        }

        return default;
    }
}
