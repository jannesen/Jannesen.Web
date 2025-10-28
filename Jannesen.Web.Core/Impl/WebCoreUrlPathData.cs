using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

#pragma warning disable CA1010 // Collections should implement generic interface. Fault in NameValueCollection.

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreUrlPathData: NameValueCollection, IWebCoreCallProcessor
    {
        public                      WebCoreUrlPathData()
        {
        }

        public          void        Proces(WebCoreCall httpCall)
        {
            ArgumentNullException.ThrowIfNull(httpCall);

            if (httpCall.Handler.WildcardPathProcessor == null)
                throw new WebHandlerConfigException("URL-PATH parameters not available, because not a wildcard handler.");

            var names = httpCall.Handler.WildcardPathProcessor.Names;

            if (names != null) {
                Match   match = httpCall.Handler.WildcardPathProcessor.RegexMatch(httpCall.Request.Path);

                foreach(string name in names) {
                    base.Add(name, match.Groups[name].Value);
                }
            }
        }
    }
}
