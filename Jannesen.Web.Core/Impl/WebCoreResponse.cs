using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Jannesen.Web.Core.Impl
{
    public abstract class WebCoreResponse
    {
        public      abstract    void            Send(WebCoreCall call, HttpResponse response);
        public      abstract    void            WriteLoggingData(StreamWriter writer);
    }
}
