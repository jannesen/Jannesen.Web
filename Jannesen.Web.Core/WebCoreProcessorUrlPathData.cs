using System;
using System.Collections.Specialized;

#pragma warning disable CA1010 // Collections should implement generic interface. Fault in NameValueCollection.

namespace Jannesen.Web.Core;

public class WebCoreProcessorUrlPathData: NameValueCollection, IWebCoreProcessor
{
    public                      WebCoreProcessorUrlPathData()
    {
    }

    public          void        Proces(WebCoreCall httpCall)
    {
        ArgumentNullException.ThrowIfNull(httpCall);

        if (httpCall.Handler.WildcardPathProcessor == null)
            throw new WebHandlerConfigException("URL-PATH parameters not available, because not a wildcard handler.");

        var names = httpCall.Handler.WildcardPathProcessor.Names;

        if (names != null) {
            var match = httpCall.Handler.WildcardPathProcessor.RegexMatch(httpCall.Request.Path);

            if (match != null) {
                foreach(var name in names) {
                    base.Add(name, match.Groups[name].Value);
                }
            }
        }
    }
}
