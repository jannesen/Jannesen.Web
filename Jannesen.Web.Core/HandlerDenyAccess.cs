using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core
{
    public class HandlerNotFound: IHttpHandlerFactory
    {
        public                          IHttpHandler            GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            throw new HttpException(404, "Resource not found or available.");
        }
        public                          void                    ReleaseHandler(IHttpHandler handler)
        {
        }
    }
}
