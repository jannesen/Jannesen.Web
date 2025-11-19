using System;
using System.Globalization;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreResponseBuffer: IWebCoreResponse
    {
        private                 string?             _contentType;
        private readonly        bool                _compression;
        private                 DateTime            _lastModified;
        private                 string?             _eTag;
        private                 int                 _cacheMaxAge;
        private                 string?             _disposition;
        private                 HttpStatusCode      _statusCode;
        private                 byte[]?             _data;
        private                 int                 _length;

        public                  string?             ContentType
        {
            get {
                return _contentType;
            }
            set {
                _contentType = value;
            }
        }
        public                  DateTime            LastModified
        {
            get {
                return _lastModified;
            }
            set {
                if (value < DateTime.MaxValue) {
                    var ticks = value.ToUniversalTime().Ticks;

                    _lastModified = new DateTime(ticks - ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc);;
                }
                else
                    _lastModified = DateTime.MaxValue;
            }
        }
        public                  string?             ETag
        {
            get {
                return _eTag;
            }
            set {
                _eTag = value;
            }
        }
        public                  string?             Disposition
        {
            get {
                return _disposition;
            }
            set {
                _disposition = value;
            }
        }
        public                  int                 CacheMaxAge
        {
            get {
                return _cacheMaxAge;
            }
            set {
                _cacheMaxAge = value;
            }
        }
        public                  HttpStatusCode      StatusCode
        {
            get {
                return _statusCode;
            }
            set {
                _statusCode = value;
            }
        }

        public                                      WebCoreResponseBuffer(string? contentType, bool compression)
        {
            _contentType     = contentType;
            _compression     = compression;
            _lastModified    = DateTime.MaxValue;
            _eTag            = null;
            _cacheMaxAge     = -1;
            _statusCode      = HttpStatusCode.OK;
        }

        public                  void                SetData(MemoryStream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            _data   = stream.GetBuffer();
            _length = (int)stream.Length;
        }
        public                  void                SetData(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);

            _data   = data;
            _length = data.Length;
        }
        public                  void                SetData(byte[] data, int length)
        {
            ArgumentNullException.ThrowIfNull(data);

            _data   = data;
            _length = length;
        }

        public                  void                Send(WebCoreCall call, HttpResponse response)
        {
            ArgumentNullException.ThrowIfNull(call);
            ArgumentNullException.ThrowIfNull(response);

            if (_statusCode == HttpStatusCode.OK && _data != null) {
                if (_disposition != null) {
                    response.Headers.ContentDisposition = _disposition;
                }

                if (_lastModified < DateTime.MaxValue &&  _lastModified > DateTime.UtcNow)
                    _lastModified = DateTime.MaxValue;

                if (_lastModified < DateTime.MaxValue || _eTag != null) {
                    var req_etag            = (string?)null;
                    var req_ifModifiedSince = (DateTime?)null;

                    if (_lastModified < DateTime.MaxValue) {
                        response.Headers.LastModified = LastModified.ToString("R", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        req_ifModifiedSince = call.RequestIfModifiedSince;
                    }

                    if (_eTag != null) {
                        response.Headers.ETag = _eTag;
                        req_etag = call.RequestIfNoneMatch;
                    }

                    response.Headers.CacheControl = _cacheMaxAge >= 0
                                                        ? ("private, max-age=" + _cacheMaxAge.ToString(CultureInfo.InvariantCulture) + ", must-revalidate")
                                                        : ("private"          );

                    if ((req_etag != null             && _eTag == req_etag                   ) ||
                        (req_ifModifiedSince.HasValue && _lastModified == req_ifModifiedSince))
                    {
                        response.StatusCode = (int)HttpStatusCode.NotModified;
                        return;
                    }
                }
                else
                if (_cacheMaxAge > 0)
                    response.Headers.CacheControl = "private, max-age=" + _cacheMaxAge.ToString(CultureInfo.InvariantCulture);
                else
                    response.Headers.CacheControl = "no-cache, no-store";
            }
            else
                response.StatusCode = (int)_statusCode;

            response.ContentType = null;

            if (_data != null) {
                response.Headers.ContentType = _contentType;

                if (call.HttpMethod != "HEAD") {
                    response.HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

                    if (_compression) {
                        (new WebCoreResponseCompressor(call.Request, new ReadOnlyMemory<byte>(_data, 0, _length))).WriteTo(response);
                    }
                    else {
                        response.Headers.ContentLength = _length;
                        response.Body.Write(_data, 0, _length);
                    }
                }
            }
            else {
                response.Headers.ContentLength = 0;
            }
        }
        public                  void                WriteLoggingData(StreamWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);

            if (_data != null) {
                if (_contentType != null && _contentType.Contains("charset=utf-8", StringComparison.OrdinalIgnoreCase)) {
                    writer.WriteLine();
                    writer.Flush();
                    writer.BaseStream.Write(_data, 0, _length);
                    writer.WriteLine();
                }
                else
                    writer.WriteLine("[BINARY-DATA]");
            }
        }
    }
}
