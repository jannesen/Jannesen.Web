using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core
{
    public class HandlerFactory: IHttpHandlerFactory
    {
        public                          IHttpHandler            GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            WebCoreHttpHandler httpHandler = WebApplication.GetHttpHandler(context.Request.Path, requestType);

            if (httpHandler == null)
                throw new HttpException(404, "Resource not found or available.");

            return httpHandler.GetHttpHandler();
        }
        public                          void                    ReleaseHandler(IHttpHandler handler)
        {
        }
    }
}
