using System;
using System.Globalization;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Jannesen.Web.Core;
using System.Threading.Tasks;

namespace Jannesen.Web.StaticFile.Internal;

abstract class ResponseStatic: IWebCoreResponse
{
    private readonly        string              _contentType;
    private readonly        DateTime?           _lastModified;
    private readonly        string              _eTag;
    private                 int                 _cacheMaxAge;

    public                  int                 CacheMaxAge
    {
        get {
            return _cacheMaxAge;
        }
        set {
            _cacheMaxAge = value;
        }
    }

    public                                      ResponseStatic(string contentType, DateTime? lastModified, string eTag)
    {
        _contentType  = contentType;
        _lastModified = lastModified;
        _eTag         = eTag;
        _cacheMaxAge  = -1;
    }

    public                  Task                SendAsync(WebCoreCall call, HttpResponse response)
    {
        var etag            = (string?)null;
        var ifModifiedSince = (DateTime?)null;

        response.HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

        if (_lastModified.HasValue) {
            response.Headers.LastModified = _lastModified.Value.ToString("R", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            ifModifiedSince = call.Request.IfModifiedSince;
        }

        if (_eTag != null) {
            response.Headers.ETag = _eTag;
            etag = call.Request.IfNoneMatch;
        }

        response.Headers.ContentType  = _contentType;

        response.Headers.CacheControl = _cacheMaxAge switch {
                                                            0   => "private, max-age=0, must-revalidate",
                                                            > 0 => "private, max-age=" + _cacheMaxAge.ToString(CultureInfo.InvariantCulture),
                                                            < 0 => "private"
                                                        };

        if ((etag != null             && _eTag == etag                   ) ||
            (ifModifiedSince.HasValue && _lastModified == ifModifiedSince)) {
            response.StatusCode = (int)HttpStatusCode.NotModified;
            return Task.CompletedTask;
        }

        response.StatusCode = (int)HttpStatusCode.OK;
        if (call.Request.Method == "HEAD") {
            return Task.CompletedTask;
        }

        return SendBodyDataAsync(response);
    }
    public      abstract    Task                WriteLoggingDataAsync(StreamWriter writer);

    protected   abstract    Task                SendBodyDataAsync(HttpResponse response);
}
