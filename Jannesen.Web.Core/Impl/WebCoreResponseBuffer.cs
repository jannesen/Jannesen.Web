using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using System.Net;
using Jannesen.FileFormat.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreResponseBuffer: IWebCoreResponse
    {
        private sealed class BufferStreamWriter: WebCoreArraryBufferStream
        {
            private                     WebCoreResponseBuffer?          _response;

            public                                                      BufferStreamWriter(WebCoreResponseBuffer response, int initialCapacity): base(new ArrayBufferWriter<byte>(initialCapacity))
            {
                _response     = response;
            }

            protected   override        void                            Dispose(bool disposing)
            {
                if (_response != null) {
                    if (disposing) {
                        _response.Data = BufferWriter.WrittenMemory;
                    }

                    _response = null;
                }

                base.Dispose(disposing);
            }
        }

        public                  string?                 ContentType         { get; set; }
        public                  DateTime                LastModified        { get; set; }
        public                  string?                 ETag                { get; set; }
        public                  string?                 Disposition         { get; set; }
        public                  int                     CacheMaxAge         { get; set; }
        public                  HttpStatusCode          StatusCode          { get; set; }
        public                  bool                    Compression         { get; set; }
        public                  ReadOnlyMemory<byte>?   Data                { get; set; }

        public                                          WebCoreResponseBuffer(string? contentType, bool compression)
        {
            ContentType  = contentType;
            LastModified = DateTime.MaxValue;
            ETag         = null;
            CacheMaxAge  = -1;
            StatusCode   = HttpStatusCode.OK;
            Compression  = compression;
        }

        public      static      WebCoreResponseBuffer   CreateJson()
        {
            return new WebCoreResponseBuffer("application/json; charset=utf-8", true);
        }
        public                  Stream                  GetStream(int initialCapacity=0x1000)
        {
            return new BufferStreamWriter(this, initialCapacity);
        }
        public                  StreamWriter            GetStreamWriter(int initialCapacity=0x1000)
        {
            return new StreamWriter(GetStream(initialCapacity), new System.Text.UTF8Encoding(false), 256);
        }
        public                  JsonWriter              GetJsonWriter(int initialCapacity=0x1000)
        {
            return new JsonWriter(GetStreamWriter(initialCapacity), false);
        }
        public                  void                    Send(WebCoreCall call, HttpResponse response)
        {
            ArgumentNullException.ThrowIfNull(call);
            ArgumentNullException.ThrowIfNull(response);

            switch(StatusCode) {
            case HttpStatusCode.OK:
                if (Data.HasValue) {
                    if (Disposition != null) {
                        response.Headers.ContentDisposition = Disposition;
                    }

                    if (LastModified < DateTime.MaxValue &&  LastModified > DateTime.UtcNow) {
                        LastModified = DateTime.MaxValue;
                    }

                    if (LastModified < DateTime.MaxValue || ETag != null) {
                        var req_etag            = (string?)null;
                        var req_ifModifiedSince = (DateTime?)null;

                        if (LastModified < DateTime.MaxValue) {
                            response.Headers.LastModified = LastModified.ToString("R", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                            req_ifModifiedSince = call.RequestIfModifiedSince;
                        }

                        if (ETag != null) {
                            response.Headers.ETag = ETag;
                            req_etag              = call.RequestIfNoneMatch;
                        }

                        response.Headers.CacheControl = CacheMaxAge >= 0
                                                            ? ("private, max-age=" + CacheMaxAge.ToString(CultureInfo.InvariantCulture) + ", must-revalidate")
                                                            : ("private"          );

                        if ((req_etag != null             && ETag == req_etag                   ) ||
                            (req_ifModifiedSince.HasValue && LastModified == req_ifModifiedSince))
                        {
                            response.StatusCode = (int)HttpStatusCode.NotModified;
                            return;
                        }
                    }
                    else
                    if (CacheMaxAge > 0) {
                        response.Headers.CacheControl = "private, max-age=" + CacheMaxAge.ToString(CultureInfo.InvariantCulture);
                    }
                    else {
                        response.Headers.CacheControl = "no-cache, no-store";
                    }
                }
                break;

            case HttpStatusCode.Unauthorized:
                response.Headers.Append("WWW-Authenticate", "Basic realm=\"" + call.ApplicationConfig.Application.Realm + "\"");
                break;

            case HttpStatusCode.BadRequest:
            case HttpStatusCode.Forbidden:
            case HttpStatusCode.NotFound:
            case HttpStatusCode.MethodNotAllowed:
            case HttpStatusCode.NotAcceptable:
            case HttpStatusCode.Gone:
                response.Headers.Append("Cache-Control", "private, max-age=300");
                break;
            }

            response.StatusCode = (int)StatusCode;

            if (ContentType != null) {
                response.Headers.ContentType = ContentType;
            }

            if (call.HttpMethod == "HEAD" && StatusCode == HttpStatusCode.OK) {
                return;
            }

            if (Data.HasValue && Data.Value.Length > 0) {
                if (Compression) {
                    (new WebCoreResponseCompressor(call.Request, Data.Value)).WriteTo(response);
                }
                else {
                    response.SendBuffer(Data.Value.Span);
                }
            }
            else {
                response.Headers.ContentLength = 0;
            }
        }
        public                  void                    WriteLoggingData(StreamWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);

            if (Data.HasValue && Data.Value.Length > 0) {
                if (ContentType != null && ContentType.Contains("charset=utf-8", StringComparison.OrdinalIgnoreCase)) {
                    writer.WriteLine();
                    writer.Flush();
                    writer.BaseStream.Write(Data.Value.Span);
                    writer.WriteLine();
                }
                else {
                    writer.WriteLine("[BINARY-DATA]");
                }
            }
        }
    }
}
