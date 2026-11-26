using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace Jannesen.Web.Core;

public sealed class WebCoreProcessorTextXml: IWebCoreProcessor<WebCoreProcessResultTextXml>
{
    public static readonly  IWebCoreProcessor<WebCoreProcessResultTextXml>  Processor = new WebCoreProcessorTextXml();

    public          IEnumerable<IWebCoreProcessor>      GetRequiredProcessors()
    {
        return [ WebCoreProcessorRequestBody.Processor ];
    }
    public          ValueTask<object>                   ProcessAsync(WebCoreCall httpCall)
    {
        ArgumentNullException.ThrowIfNull(httpCall);

        try {
            using(var reader = httpCall.RequestBody.GetStreamReader("text/xml")) {
                if (reader != null) {
                    var document = new XmlDocument() { XmlResolver=null } ;
                    using (var r = new XmlTextReader(reader) { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }) {
                        document.Load(r);
                    }
                    return ValueTask.FromResult((object)new WebCoreProcessResultTextXml(document));
                }
            }

            return ValueTask.FromResult((object)new WebCoreProcessResultTextXml(null));
        }
        catch(TaskCanceledException) {
            throw;
        }
        catch(Exception err) {
            throw new WebRequestException("Error in XML HTTP-BODY.", err);
        }
    }
}
