using System;
using System.IO;
using System.Globalization;
using System.Net;
using System.Xml;
using System.Text;
using Jannesen.FileFormat.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreResponseError: WebCoreResponse
    {
        private readonly        WebCoreHttpHandler      _handler;
        private readonly        Exception               _err;
        private                 string?                 _code;

        private readonly         HttpStatusCode         _statusCode;
        private readonly         string                 _contentType;
        private readonly         ReadOnlyMemory<byte>   _data;

        public                                          WebCoreResponseError(WebCoreHttpHandler handler, Exception err, string? contentType)
        {
            _handler = handler;
            _err     = err;

            _statusCode  = _processErrorCode();
            _contentType = contentType ?? "text/plain";

            using (var buffer = new MemoryStream()) {
                using (var streamWriter = new StreamWriter(buffer, new UTF8Encoding(false, false), 0x1000, true)) {
                    switch (_contentType) {
                    case "text/xml":
                        _contentType = "text/xml; charset=utf-8";
                        _writeXml(streamWriter);
                        break;

                    case "application/json":
                        _contentType = "application/json; charset=utf-8";
                        _writeJson(streamWriter);
                        break;

                    default:
                        _contentType = "text/plain; charset=utf-8";
                        _writeText(streamWriter);
                        break;
                    }
                }

                _data = buffer.GetReadOnlyData();
            }
        }

        public      override    void                    Send(WebCoreCall call, HttpResponse response)
        {
            ArgumentNullException.ThrowIfNull(call);
            ArgumentNullException.ThrowIfNull(response);

            if (_statusCode == HttpStatusCode.Unauthorized) {
                response.Headers.Append("WWW-Authenticate", "Basic realm=\"" + call.ApplicationConfig.Application.Realm + "\"");
            }

            response.StatusCode    = (int)_statusCode;
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

        private                 HttpStatusCode          _processErrorCode()
        {
            for (var err = _err ; err != null ; err = err.InnerException) {
                if (err is WebHttpException httpException) {
                    var statusCode = httpException.StatusCode;

                    if (statusCode == HttpStatusCode.InternalServerError) { // HttpException are not used for server error
                        statusCode = HttpStatusCode.BadRequest;
                    }

                    _code = "HTTP-ERROR-CODE-" + ((int)statusCode).ToString(CultureInfo.InvariantCulture);
                    return (HttpStatusCode)statusCode;
                }

                if (err is WebResourceDownException) {
                    _code = "SERVICE-DOWN";
                    return HttpStatusCode.ServiceUnavailable;
                }

                if (err is InternalErrorException || err is WebResponseException) {
                    _code = "INTERNAL-ERROR";
                    return HttpStatusCode.InternalServerError;
                }

                if (err is WebConfigException || err is WebAppNotInitialized || err is WebInitializationException || err is WebResourceNotFoundException) {
                    _code = "CONFIG-ERROR";
                    return HttpStatusCode.InternalServerError;
                }

                if (err is WebRequestException) {
                    _code = "REQUEST-ERROR";
                    return HttpStatusCode.BadRequest;
                }

                if (err is WebBasicAutorizationException) {
                    _code = "BASIC-AUTORIZATION-NEEDED";
                    return HttpStatusCode.Unauthorized;
                }

                {
                    var httpCode = _handler.ProcessErrorCode(err, out _code, out var _);
                    if (httpCode != 0) {
                        return (HttpStatusCode)httpCode;
                    }
                }
            }

            _code       = "GENERAL-ERROR";
            return HttpStatusCode.InternalServerError;
        }
        private                 void                    _writeText(StreamWriter streamWriter)
        {
            streamWriter.WriteLine("ERROR PROCESSING REQUEST");
            streamWriter.WriteLine("ERROR-CODE: " + _code);
            if (_withDetails()) {
                streamWriter.WriteLine();
                streamWriter.WriteLine("============================================================");
                streamWriter.WriteLine("DETAILS:");
                for (var err = _err ; err != null ; err = err.InnerException) { 
                    streamWriter.WriteLine(err.Message);
                }
                streamWriter.WriteLine("============================================================");
            }
        }
        private                 void                    _writeXml(StreamWriter streamWriter)
        {
            using (var xmlWriter = new XmlTextWriter(streamWriter)) {
                xmlWriter.WriteStartElement("error");
                xmlWriter.WriteAttributeString("code", _code);

                if (_withDetails()) {
                    for (var err = _err ; err != null ; err = err.InnerException) {
                        xmlWriter.WriteStartElement("error-detail");
                        xmlWriter.WriteAttributeString("class",   err.GetType().FullName);
                        xmlWriter.WriteAttributeString("message", err.Message);
                        xmlWriter.WriteEndElement();
                    }
                }

                xmlWriter.WriteEndElement();
            }
        }
        private                 void                    _writeJson(StreamWriter streamWriter)
        {
            using (var jsonWriter = new JsonWriter(streamWriter, false)) {
                jsonWriter.WriteStartObject();

                    jsonWriter.WriteNameValue("code", _code);

                    if (_withDetails()) {
                        jsonWriter.WriteStartArray("detail");

                        for (var err = _err ; err != null ; err = err.InnerException) {
                            jsonWriter.WriteStartObject();
                            jsonWriter.WriteNameValue("class",   err.GetType().FullName);
                            jsonWriter.WriteNameValue("message", err.Message);
                            jsonWriter.WriteEndObject();
                        }

                        jsonWriter.WriteEndArray();
                    }

                jsonWriter.WriteEndObject();
            }
        }

        private                 bool                    _withDetails()
        {
            switch(_statusCode) {
            case HttpStatusCode.OK:
            case HttpStatusCode.Created:
            case HttpStatusCode.InternalServerError:
            case HttpStatusCode.BadRequest:
                return true;

            default:
                return false;
            }
        }
    }
}
