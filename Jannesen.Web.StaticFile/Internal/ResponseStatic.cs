using System;
using System.IO;
using System.Net;
using System.Web;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.StaticFile.Internal
{
    abstract class ResponseStatic: WebCoreResponse
    {
        private                 string              _contentType;
        private                 bool                _cachepublic;
        private                 DateTime?           _lastModified;
        private                 string              _eTag;
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

        public                                      ResponseStatic(string contentType, bool cachepublic, DateTime? lastModified, string eTag)
        {
            _contentType  = contentType;
            _cachepublic  = cachepublic;
            _lastModified = lastModified;
            _eTag         = eTag;
            _cacheMaxAge  = -1;
        }

        public      override    void                Send(WebCoreCall call, HttpResponse response)
        {
            string      etag            = null; ;
            DateTime?   ifModifiedSince = null ;

            response.BufferOutput = false;
            response.ContentType = null;

            if (_lastModified.HasValue) {
                response.AppendHeader("Last-Modified", _lastModified.Value.ToString("R", System.Globalization.DateTimeFormatInfo.InvariantInfo));
                ifModifiedSince = call.RequestIfModifiedSince;
            }

            if (_eTag != null) {
                response.AppendHeader("ETag", _eTag);
                etag = call.RequestIfNoneMatch;
            }

            response.AddHeader("Cache-Control",     (_cachepublic ? "public" : "private" ) +
                                                    (_cacheMaxAge > 0 ? ", max-age=" + _cacheMaxAge.ToString() : (_cacheMaxAge == 0 ? ", max-age=0, must-revalidate" : "")));
            response.AppendHeader("Content-Type", _contentType);

            if ((etag != null             && _eTag == etag                   ) ||
                (ifModifiedSince.HasValue && _lastModified == ifModifiedSince))
            {
                response.StatusCode = (int)HttpStatusCode.NotModified;
                return;
            }

            response.StatusCode = (int)HttpStatusCode.OK;
            if (call.HttpMethod != "HEAD")
                SendBodyData(response);
        }

        protected   abstract    void                SendBodyData(HttpResponse response);
    }
}
