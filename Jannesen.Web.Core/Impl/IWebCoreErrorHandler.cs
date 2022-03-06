using System;
using System.Reflection;

namespace Jannesen.Web.Core.Impl
{
    public interface IWebCoreErrorHandler
    {
        WebCoreResponse     GetErrorResponse(WebCoreHttpHandler handler, Exception err);
    }
}
