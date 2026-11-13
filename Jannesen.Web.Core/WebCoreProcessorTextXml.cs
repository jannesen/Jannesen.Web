using System;
using System.Xml;

namespace Jannesen.Web.Core;

public sealed class WebCoreProcessorTextXml: IWebCoreProcessor
{
    public static readonly  IWebCoreProcessor    Definition = new WebCoreProcessorTextXml();

    public          string          Name    => "TextXml";

    public          object          Process(WebCoreCall httpCall)
    {
        ArgumentNullException.ThrowIfNull(httpCall);

        try {
            using(var reader = httpCall.RequestBody.GetStreamReader("text/xml")) {
                if (reader != null) {
                    var document = new XmlDocument() { XmlResolver=null } ;
#pragma warning disable CA2000 // CA2000: Dispose objects before losing scope
                    document.Load(new XmlTextReader(reader) { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
#pragma warning restore CA2000
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
