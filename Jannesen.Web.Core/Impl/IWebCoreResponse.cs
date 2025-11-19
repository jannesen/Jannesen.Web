using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Jannesen.Web.Core.Impl
{
    public interface IWebCoreResponse
    {
        void            Send(WebCoreCall call, HttpResponse response);
        void            WriteLoggingData(StreamWriter writer);
    }
}
