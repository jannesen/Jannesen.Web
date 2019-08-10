using System;
using System.Web;
using System.Net;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreProcessorBasicAutorization : IWebCoreCallProcessor
    {
        private                 string              _username;
        private                 string              _passwd;

        public                  string              UserName
        {
            get {
                return _username;
            }
        }
        public                  string              Passwd
        {
            get {
                return _passwd;
            }
        }

        public                  void                Proces(WebCoreCall httpCall)
        {
            string          authorization   = httpCall.Request.Headers["Authorization"];

            if (authorization != null && authorization.StartsWith("Basic ", StringComparison.Ordinal)) {
                string  AuthStr     = System.Text.Encoding.ASCII.GetString(System.Convert.FromBase64String(authorization.Substring(6)));
                int     AuthStrPos  = AuthStr.IndexOf(':');

                if (AuthStrPos>=0) {
                    _username       = AuthStr.Substring(0, AuthStrPos);
                    _passwd     = AuthStr.Substring(AuthStrPos+1);
                    return ;
                }
            }

            throw new WebBasicAutorizationException("Basic authorization missing");
        }
    }
}
