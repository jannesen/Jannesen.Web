using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Jannesen.Web.Core;

public interface IWebCoreResponse
{
    Task            SendAsync(WebCoreCall call, HttpResponse response);
    Task            WriteLoggingDataAsync(StreamWriter writer);
}
