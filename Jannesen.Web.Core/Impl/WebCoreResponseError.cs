using System;
using System.IO;
using System.Data.SqlClient;
using System.Globalization;
using System.Net;
using System.Web;
using System.Xml;
using System.Text;
using Jannesen.FileFormat.Json;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreResponseError: WebCoreResponseBuffer
    {
        private readonly        WebCoreHttpHandler      _handler;
        private readonly        Exception               _err;
        private                 string                  _code;

        public                                          WebCoreResponseError(WebCoreHttpHandler handler, Exception err, string contentType): base(contentType, false, true)
        {
            _handler = handler;
            _err     = err;

            StatusCode = _processErrorCode();

            if ((!(err is HttpException)) && _handler.MapTo200 && (StatusCode != HttpStatusCode.Unauthorized))
                StatusCode = HttpStatusCode.OK;

            using (MemoryStream buffer = new MemoryStream()) {
                using (StreamWriter streamWriter = new StreamWriter(buffer, new UTF8Encoding(false, false), 0x1000, true)) {
                    switch (ContentType) {
                    case "text/xml":            _writeXml(streamWriter);    break;
                    case "application/json":    _writeJson(streamWriter);   break;
                    default:                    _writeText(streamWriter);   break;
                    }
                }

                SetData(buffer);
            }
        }

        public      override    void                    Send(WebCoreCall call, HttpResponse response)
        {
            if (StatusCode == HttpStatusCode.Unauthorized)
                response.AppendHeader("WWW-Authenticate", "Basic realm=\"" + WebApplication.Name + "\"");

            base.Send(call, response);
        }

        private                 HttpStatusCode          _processErrorCode()
        {
            for (Exception err = _err ; err != null ; err = err.InnerException) {
                if (err is HttpException) {
                    int statusCode = ((HttpException)err).GetHttpCode();

                    switch(((HttpException)err).WebEventCode) {
                    case System.Web.Management.WebEventCodes.RuntimeErrorRequestAbort:  statusCode = 408;   break;
                    case System.Web.Management.WebEventCodes.RuntimeErrorPostTooLarge:  statusCode = 413;   break;
                    default:
                        switch(err.Message) {
                        case "Request timed out.":  statusCode = 408;   break;
                        }
                        break;
                    }

                    if (statusCode == 500) { // HttpException are not used for server error
                        statusCode = 400;
                    }
                    _code = "HTTP-ERROR-CODE-" + statusCode.ToString(CultureInfo.InvariantCulture);
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
                    int httpCode = _handler.ProcessErrorCode(err, out _code, out var _);
                    if (httpCode != 0)
                        return (HttpStatusCode)httpCode;
                }
            }

            _code       = "GENERAL-ERROR";
            return HttpStatusCode.InternalServerError;
        }
        private                 void                    _writeText(StreamWriter streamWriter)
        {
            ContentType = "text/plain; charset=utf-8";

            streamWriter.WriteLine("ERROR PROCESSING REQUEST");
            streamWriter.WriteLine("ERROR-CODE: " + _code);
            if (_withDetails()) { 
                streamWriter.WriteLine();
                streamWriter.WriteLine("============================================================");
                streamWriter.WriteLine("DETAILS:");
                for (Exception err = _err ; err != null ; err = err.InnerException)
                    streamWriter.WriteLine(err.Message);
                streamWriter.WriteLine("============================================================");
            }
        }
        private                 void                    _writeXml(StreamWriter streamWriter)
        {
            ContentType = "text/xml; charset=utf-8";

            using (XmlTextWriter xmlWriter = new XmlTextWriter(streamWriter)) {
                xmlWriter.WriteStartElement("error");
                xmlWriter.WriteAttributeString("code", _code);

                if (_withDetails()) { 
                    for (Exception err = _err ; err != null ; err = err.InnerException) {
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
            ContentType = "application/json; charset=utf-8";

            using (JsonWriter jsonWriter = new JsonWriter(streamWriter, false)) {
                jsonWriter.WriteStartObject();

                if (_handler.MapTo200) {
                    jsonWriter.WriteStartObject("error");
                }

                jsonWriter.WriteNameValue("code", _code);

                if (_withDetails()) { 
                    jsonWriter.WriteStartArray("detail");

                    for (Exception err = _err ; err != null ; err = err.InnerException) {
                        jsonWriter.WriteStartObject();
                        jsonWriter.WriteNameValue("class",   err.GetType().FullName);
                        jsonWriter.WriteNameValue("message", err.Message);
                        jsonWriter.WriteEndObject();
                    }
                }
            }
        }

        private                 bool                    _withDetails()
        {
            switch(StatusCode) {
            case HttpStatusCode.OK:
            case HttpStatusCode.Created:
            case HttpStatusCode.InternalServerError:
                return true;

            default:
                return false;
            }
        }
    }
}
