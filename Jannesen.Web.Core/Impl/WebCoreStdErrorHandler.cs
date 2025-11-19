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
    public static class WebCoreStdErrorHandler
    {
        public  static          IWebCoreResponse            Create(WebCoreHttpHandler? handler, Exception exception)
        {
            var errorData = WebCoreErrorData.Create(handler, exception);
            var response  = new WebCoreResponseBuffer(null, true) {
                                StatusCode = errorData.StatusCode
                            };

            using (var streamWriter = response.GetStreamWriter()) {
                switch (handler?.Mimetype) {
                case "text/xml":
                    response.ContentType = "text/xml; charset=utf-8";
                    _writeXml(streamWriter, errorData, exception);
                    break;

                case "application/json":
                    response.ContentType = "application/json; charset=utf-8";
                    _writeJson(streamWriter, errorData, exception);
                    break;

                default:
                    response.ContentType = "text/plain; charset=utf-8";
                    _writeText(streamWriter, errorData, exception);
                    break;
                }
            }

            return response;
        }

        private static          void                    _writeText(StreamWriter streamWriter, WebCoreErrorData errorData, Exception exception)
        {
            streamWriter.WriteLine("ERROR PROCESSING REQUEST");
            streamWriter.WriteLine("ERROR-CODE: " + errorData.Code);
            if (_withDetails(errorData)) {
                streamWriter.WriteLine();
                streamWriter.WriteLine("============================================================");
                streamWriter.WriteLine("DETAILS:");
                for (var ex = exception ; ex != null ; ex = ex.InnerException) { 
                    streamWriter.WriteLine(ex.Message);
                }
                streamWriter.WriteLine("============================================================");
            }
        }
        private static          void                    _writeXml(StreamWriter streamWriter, WebCoreErrorData errorData, Exception exception)
        {
            using (var xmlWriter = new XmlTextWriter(streamWriter)) {
                xmlWriter.WriteStartElement("error");
                xmlWriter.WriteAttributeString("code", errorData.Code);

                if (_withDetails(errorData)) {
                    for (var ex = exception ; ex != null ; ex = ex.InnerException) { 
                        xmlWriter.WriteStartElement("error-detail");
                        xmlWriter.WriteAttributeString("class",   ex.GetType().FullName);
                        xmlWriter.WriteAttributeString("message", ex.Message);
                        xmlWriter.WriteEndElement();
                    }
                }

                xmlWriter.WriteEndElement();
            }
        }
        private static          void                    _writeJson(StreamWriter streamWriter, WebCoreErrorData errorData, Exception exception)
        {
            using (var jsonWriter = new JsonWriter(streamWriter, false)) {
                jsonWriter.WriteStartObject();

                    jsonWriter.WriteNameValue("code", errorData.Code);

                    if (_withDetails(errorData)) {
                        jsonWriter.WriteStartArray("detail");

                        for (var ex = exception ; ex != null ; ex = ex.InnerException) { 
                            jsonWriter.WriteStartObject();
                            jsonWriter.WriteNameValue("class",   ex.GetType().FullName);
                            jsonWriter.WriteNameValue("message", ex.Message);
                            jsonWriter.WriteEndObject();
                        }

                        jsonWriter.WriteEndArray();
                    }

                jsonWriter.WriteEndObject();
            }
        }
        private static          bool                    _withDetails(WebCoreErrorData errorData)
        {
            switch(errorData.StatusCode) {
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
