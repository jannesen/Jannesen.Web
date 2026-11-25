using System;
using System.Collections.Generic;
using System.Xml;

namespace Jannesen.Web.Core;

public sealed class WebCoreProcessorTextXml: IWebCoreProcessor<WebCoreProcessResultTextXml>
{
    public static readonly  IWebCoreProcessor<WebCoreProcessResultTextXml>  Processor = new WebCoreProcessorTextXml();

    public          IEnumerable<IWebCoreProcessor>      GetRequiredProcessors()
    {
        return [ WebCoreProcessorRequestBody.Processor ];
    }
    public          object                              Process(WebCoreCall httpCall)
    {
        ArgumentNullException.ThrowIfNull(httpCall);

        try {
            using(var reader = httpCall.RequestBody.GetStreamReader("text/xml")) {
                if (reader != null) {
                    var document = new XmlDocument() { XmlResolver=null } ;
                    using (var r = new XmlTextReader(reader) { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }) {
                        document.Load(r);
                    }
                    return new WebCoreProcessResultTextXml(document);
                }
            }

            return new WebCoreProcessResultTextXml(null);
        }
        catch(Exception err) {
            throw new WebRequestException("Error in XML HTTP-BODY.", err);
        }
    }
}
