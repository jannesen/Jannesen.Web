using System;
using System.Collections.Generic;

namespace Jannesen.Web.Core;

public sealed class WebCoreProcessorUrlPathData: IWebCoreProcessor<WebCoreProcessResultUrlPathData>
{
    public static readonly  IWebCoreProcessor<WebCoreProcessResultUrlPathData>    Definition = new WebCoreProcessorUrlPathData();

    public          IEnumerable<IWebCoreProcessor<object>>  GetRequiredProcessors()
    {
        return [];
    }
    public          WebCoreProcessResultUrlPathData         Process(WebCoreCall httpCall)
    {
        ArgumentNullException.ThrowIfNull(httpCall);

        if (httpCall.Handler.WildcardPathProcessor == null) {
            throw new WebHandlerConfigException("URL-PATH parameters not available, because not a wildcard handler.");
        }

        var names = httpCall.Handler.WildcardPathProcessor.Names;

        if (names != null) {
            var match = httpCall.Handler.WildcardPathProcessor.RegexMatch(httpCall.Request.Path);

            if (match != null) {
                return new WebCoreProcessResultUrlPathData(names, match);
            }
        }

        return new WebCoreProcessResultUrlPathData();
    }
}
