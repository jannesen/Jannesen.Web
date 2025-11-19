using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreResponseRedirect: IWebCoreResponse
    {
        public                  string              Target          { get; init; }

        public                                      WebCoreResponseRedirect(string target)
        {
            Target = target;
        }

        public                  void                Send(WebCoreCall call, HttpResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);

            response.Redirect(Target);
        }

        public                  void                WriteLoggingData(StreamWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);

            writer.WriteLine("REDIRECT TO: " + Target);
            writer.WriteLine();
        }
    }
}
