using System;
using System.Xml;
using System.Web;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreProcessorTextXml : IWebCoreCallProcessor
    {
        private                 XmlDocument         _document;

        public                  void                Proces(WebCoreCall httpCall)
        {
            try {

                using(var reader = httpCall.GetBodyText("text/xml")) {
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

        public                  string              GetStringValue(string xpath)
        {
            if (_document == null)
                throw new WebRequestException("Empty body.");

            return _document.SelectSingleNode(xpath)?.Value;
        }
    }
}
