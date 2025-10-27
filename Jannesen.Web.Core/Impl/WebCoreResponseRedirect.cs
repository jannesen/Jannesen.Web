using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreResponseRedirect: WebCoreResponse
    {
        public                  string              Target          { get; init; }

        public                                      WebCoreResponseRedirect(string target)
        {
            Target = target;
        }

        public      override    void                Send(WebCoreCall call, HttpResponse response)
        {
            response.Redirect(Target);
        }

        public      override    void                WriteLoggingData(StreamWriter writer)
        {
            writer.WriteLine("REDIRECT TO: " + Target);
            writer.WriteLine();
        }
    }
}
