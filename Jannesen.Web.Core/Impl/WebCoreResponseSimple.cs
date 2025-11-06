using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreResponseSimple: WebCoreResponse
    {
        private readonly        HttpStatusCode          _status;
        private readonly        string                  _contentType;
        private readonly        ReadOnlyMemory<byte>    _data;

        public                                          WebCoreResponseSimple(HttpStatusCode status, string contentType, ReadOnlyMemory<byte> data)
        {
            _status      = status;
            _contentType = contentType;
            _data        = data;
        }

        public      override    void                    Send(WebCoreCall call, HttpResponse response)
        {
            ArgumentNullException.ThrowIfNull(call);
            ArgumentNullException.ThrowIfNull(response);

            switch(_status) {

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

            response.StatusCode    = (int)_status;
            response.ContentType   = _contentType;
            response.SendBuffer(_data.Span);
        }
        public      override    void                    WriteLoggingData(StreamWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);

            writer.WriteLine();
            writer.Flush();
            writer.BaseStream.Write(_data.Span);
            writer.WriteLine();
        }

    }
}
