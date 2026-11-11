using System;
using System.Collections.Specialized;

#pragma warning disable CA1010 // Collections should implement generic interface. Fault in NameValueCollection.

namespace Jannesen.Web.Core;

public class WebCoreProcessorUrlPathData: NameValueCollection
{
    public static readonly  IWebCoreProcessorDefinition    Definition = new DefinitionClass();

    internal sealed class DefinitionClass: IWebCoreProcessorDefinition
    {
        public          string          Name    => "TextXml";

        public          object          Create(WebCoreCall httpCall)
        {
            ArgumentNullException.ThrowIfNull(httpCall);
            return new WebCoreProcessorUrlPathData(httpCall);
        }
    }

    internal                WebCoreProcessorUrlPathData(WebCoreCall httpCall)
    {
        if (httpCall.Handler.WildcardPathProcessor == null) {
            throw new WebHandlerConfigException("URL-PATH parameters not available, because not a wildcard handler.");
        }

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
