using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Jannesen.Web.Core;

public interface IWebCoreResponse
{
    void            Send(WebCoreCall call, HttpResponse response);
    void            WriteLoggingData(StreamWriter writer);
}
