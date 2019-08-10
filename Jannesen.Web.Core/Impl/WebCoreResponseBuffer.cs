using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreResponseBuffer: WebCoreResponse
    {
        private                 string              _contentType;
        private readonly        bool                _cachepublic;
        private readonly        bool                _compression;
        private                 DateTime            _lastModified;
        private                 string              _eTag;
        private                 int                 _cacheMaxAge;
        private                 HttpStatusCode      _statusCode;
        private                 byte[]              _data;
        private                 int                 _length;

        public                  string              ContentType
        {
            get {
                return _contentType;
            }
            set {
                _contentType = value;
            }
        }
        public                  bool                CachePublic
        {
            get {
                return _cachepublic;
            }
        }
        public                  DateTime            LastModified
        {
            get {
                return _lastModified;
            }
            set {
                if (value < DateTime.MaxValue) {
                    long ticks = value.ToUniversalTime().Ticks;

                    _lastModified = new DateTime(ticks - ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc);;
                }
                else
                    _lastModified = DateTime.MaxValue;
            }
        }
        public                  string              ETag
        {
            get {
                return _eTag;
            }
            set {
                _eTag = value;
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

        public                                      WebCoreResponseBuffer(string contentType, bool pub, bool compression)
        {
            _contentType     = contentType;
            _cachepublic     = pub;
            _compression     = compression;
            _lastModified    = DateTime.MaxValue;
            _eTag            = null;
            _cacheMaxAge     = -1;
            _statusCode      = HttpStatusCode.OK;
        }

        public                  void                SetData(MemoryStream stream)
        {
            _data   = stream.GetBuffer();
            _length = (int)stream.Length;
        }
        public                  void                SetData(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            _data   = data;
            _length = data.Length;
        }
        public                  void                SetData(byte[] data, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _data   = data;
            _length = length;
        }

        public      override    void                Send(WebCoreCall call, HttpResponse response)
        {
            response.BufferOutput = false;

            if (_statusCode == HttpStatusCode.OK) {
                if (_lastModified < DateTime.MaxValue &&  _lastModified > DateTime.UtcNow)
                    _lastModified = DateTime.MaxValue;

                if (_lastModified < DateTime.MaxValue || _eTag != null) {
                    string      req_etag            = null;
                    DateTime?   req_ifModifiedSince = null;

                    if (_lastModified < DateTime.MaxValue) {
                        response.AppendHeader("Last-Modified", LastModified.ToString("R", System.Globalization.DateTimeFormatInfo.InvariantInfo));
                        req_ifModifiedSince = call.RequestIfModifiedSince;
                    }

                    if (_eTag != null) {
                        response.AppendHeader("ETag",          _eTag);
                        req_etag = call.RequestIfNoneMatch;
                    }

                    response.AddHeader("Cache-Control", _cacheMaxAge >= 0
                                                        ? ((_cachepublic ? "public, max-age=" : "private, max-age=") + _cacheMaxAge.ToString(CultureInfo.InvariantCulture) + ", must-revalidate")
                                                        : ((_cachepublic ? "public"           : "private"          )                                                ));

                    if ((req_etag != null             && _eTag == req_etag                   ) ||
                        (req_ifModifiedSince.HasValue && _lastModified == req_ifModifiedSince))
                    {
                        response.StatusCode = (int)HttpStatusCode.NotModified;
                        return;
                    }
                }
                else
                if (_cacheMaxAge > 0)
                    response.AddHeader("Cache-Control", (_cachepublic ? "public, max-age=" : "private, max-age=") + _cacheMaxAge.ToString(CultureInfo.InvariantCulture));
                else
                    response.AddHeader("Cache-Control", "no-cache, no-store");
            }
            else
                response.StatusCode = (int)_statusCode;

            response.ContentType = null;

            if (_data != null) {
                response.AppendHeader("Content-Type", _contentType);

                if (call.HttpMethod != "HEAD") {
                    if (_compression && _length > 512) {
                        string contentEncoding = GetResponseCompressionEncoding(call);

                        if (contentEncoding != null) {
                            response.AddHeader("Content-Encoding", contentEncoding);

                            using (MemoryStream buffer = new MemoryStream(_length > 0x4000 ? _length / 4 : 0x1000)) {
                                using (Stream stream = GetCompressor(contentEncoding, buffer))
                                    stream.Write(_data, 0, _length);

                                response.AppendHeader("Content-Length", buffer.Length.ToString(CultureInfo.InvariantCulture));
                                response.OutputStream.Write(buffer.GetBuffer(), 0, (int)buffer.Length);
                            }

                            return;
                        }
                    }

                    response.AppendHeader("Content-Length", _length.ToString(CultureInfo.InvariantCulture));
                    response.OutputStream.Write(_data, 0, _length);
                }
            }
        }

        public      override    void                WriteLoggingData(StreamWriter writer)
        {
            if (_contentType.IndexOf("charset=utf-8", StringComparison.Ordinal) > 0) {
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
