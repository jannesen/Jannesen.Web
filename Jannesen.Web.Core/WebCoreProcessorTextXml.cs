using System;
using System.Xml;

namespace Jannesen.Web.Core;

public class WebCoreProcessorTextXml
{
    public static readonly  IWebCoreProcessorDefinition    Definition = new DefinitionClass();

    internal sealed class DefinitionClass: IWebCoreProcessorDefinition
    {
        public          string          Name    => "TextXml";

        public          object          Create(WebCoreCall httpCall)
        {
            ArgumentNullException.ThrowIfNull(httpCall);
            return new WebCoreProcessorTextXml(httpCall);
        }
    }

    private readonly        XmlDocument?        _document;

    public                  XmlDocument?        Document => _document;

    internal                                    WebCoreProcessorTextXml(WebCoreCall httpCall)
    {
        try {

            using(var reader = httpCall.RequestBody.GetStreamReader("text/xml")) {
                if (reader != null) {
                    _document = new XmlDocument() { XmlResolver=null } ;
#pragma warning disable CA2000 // CA2000: Dispose objects before losing scope
                    _document.Load(new XmlTextReader(reader) { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
#pragma warning restore CA2000
                }
            }
        }
        catch(Exception err) {
            throw new WebRequestException("Error in XML HTTP-BODY.", err);
        }
    }

    public                  string?             GetStringValue(string xpath)
    {
        if (_document == null)
            throw new WebRequestException("Empty body.");

        return _document.SelectSingleNode(xpath)?.Value;
    }
}
