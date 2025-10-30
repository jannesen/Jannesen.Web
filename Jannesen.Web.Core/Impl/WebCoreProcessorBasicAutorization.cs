using System;
using System.Net;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreProcessorBasicAutorization : IWebCoreCallProcessor
    {
        private                 string?             _username;
        private                 string?             _passwd;

        public                  string?             UserName        => _username;
        public                  string?             Passwd          => _passwd;

        public                  void                Proces(WebCoreCall httpCall)
        {
            ArgumentNullException.ThrowIfNull(httpCall);

            var authorization   = httpCall.GetHeader("Authorization");

            if (authorization != null && authorization.StartsWith("Basic ", StringComparison.Ordinal)) {
                var AuthStr     = System.Text.Encoding.ASCII.GetString(System.Convert.FromBase64String(authorization.Substring(6)));
                var AuthStrPos  = AuthStr.IndexOf(':', StringComparison.Ordinal);

                if (AuthStrPos>=0) {
                    _username = AuthStr.Substring(0, AuthStrPos);
                    _passwd   = AuthStr.Substring(AuthStrPos+1);
                    return ;
                }
            }

            throw new WebBasicAutorizationException("Basic authorization missing");
        }
    }
}
