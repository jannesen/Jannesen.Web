using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Jannesen.Web.Core;

public class WebCoreResponseRedirect: IWebCoreResponse
{
    public                  string              Target          { get; init; }

    public                                      WebCoreResponseRedirect(string target)
    {
        Target = target;
    }

    public                  Task                SendAsync(WebCoreCall call, HttpResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        response.Redirect(Target);
        return Task.CompletedTask;
    }
    public            async Task                WriteLoggingDataAsync(StreamWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        await writer.WriteLineAsync("REDIRECT TO: " + Target);
        await writer.WriteLineAsync();
    }
}
