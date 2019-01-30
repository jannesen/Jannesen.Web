using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Text;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreResponseRedirect: WebCoreResponse
    {
        public  readonly        string              Target;

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
