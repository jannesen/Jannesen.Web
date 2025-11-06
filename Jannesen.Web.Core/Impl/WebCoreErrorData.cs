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
    public delegate WebCoreResponse    WebCoreErrorHandler(WebCoreHttpHandler? httpHandler, Exception err);

    public readonly struct WebCoreErrorData
    {
        public                  HttpStatusCode          Status      { get; init; }
        public                  string                  Code        { get; init; }
        public                  string                  Message     { get; init; }

        public  static          WebCoreErrorData        Create(WebCoreHttpHandler? handler, Exception exception)
        {
            for (var err = exception ; err != null ; err = err.InnerException) {
                if (err is WebHttpException httpException) {
                    return new WebCoreErrorData() { 
                               Status    = httpException.StatusCode,
                               Code      = "HTTP-ERROR-CODE-" + ((int)httpException.StatusCode).ToString(CultureInfo.InvariantCulture),
                               Message   = err.Message
                           };
                }

                if (err is WebResourceDownException) {
                    return new WebCoreErrorData() { 
                               Status    = HttpStatusCode.ServiceUnavailable,
                               Code      = "SERVICE-DOWN",
                               Message   = "Service down"
                           };
                }

                if (err is InternalErrorException || err is WebResponseException) {
                    return new WebCoreErrorData() { 
                               Status    = HttpStatusCode.InternalServerError,
                               Code      = "INTERNAL-ERROR",
                               Message   = err.Message
                           };
                }

                if (err is WebConfigException || err is WebAppNotInitialized || err is WebInitializationException || err is WebResourceNotFoundException) {
                    return new WebCoreErrorData() { 
                               Status    = HttpStatusCode.InternalServerError,
                               Code      = "CONFIG-ERROR",
                               Message   = err.Message
                           };
                }

                if (err is WebRequestException) {
                    return new WebCoreErrorData() { 
                               Status    = HttpStatusCode.BadRequest,
                               Code      = "REQUEST-ERROR",
                               Message   = err.Message
                           };
                }

                if (err is WebBasicAutorizationException) {
                    return new WebCoreErrorData() {
                               Status    = HttpStatusCode.Unauthorized,
                               Code      = "BASIC-AUTORIZATION-NEEDED",
                               Message   = err.Message
                           };
                }

                if (handler != null)
                {
                    var e = handler.ProcessError(err);
                    if (e.HasValue) {
                        return e.Value;
                    }
                }
            }

            return new WebCoreErrorData() {
                       Status    = HttpStatusCode.InternalServerError,
                       Code      = "GENERAL-ERROR",
                       Message   = exception.Message
                   };
        }
    }
}
