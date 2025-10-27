using System;
using System.Net;

namespace Jannesen.Web.Core
{
    public class WebHttpException: Exception
    {
        public      HttpStatusCode          StatusCode          { get; init; }

        public                              WebHttpException(): base()
        {
        }
        public                              WebHttpException(HttpStatusCode statusCode, string messages): base(messages)
        {
            this.StatusCode = statusCode;
        }
        public                              WebHttpException(HttpStatusCode statusCode, string messages, Exception innerException): base(messages, innerException)
        {
            this.StatusCode = statusCode;
        }
    }
}
