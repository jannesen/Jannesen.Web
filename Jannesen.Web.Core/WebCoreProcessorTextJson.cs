using System;
using System.Collections.Generic;
using Jannesen.FileFormat.Json;

namespace Jannesen.Web.Core;

public sealed class WebCoreProcessorTextJson: IWebCoreProcessor<WebCoreProcessResultTextJson>
{
    public static readonly  IWebCoreProcessor<WebCoreProcessResultTextJson> Processor = new WebCoreProcessorTextJson();

    public          IEnumerable<IWebCoreProcessor>      GetRequiredProcessors()
    {
        return [ WebCoreProcessorRequestBody.Processor ];
    }
    public          object                              Process(WebCoreCall httpCall)
    {
        ArgumentNullException.ThrowIfNull(httpCall);

        try {
            var stream = httpCall.RequestBody.GetStreamReader("application/json");
            if (stream != null) {
                using(var reader = new JsonReader(stream)) {
                    return new WebCoreProcessResultTextJson(reader.ParseDocument());
                }
            }
            else {
                return new WebCoreProcessResultTextJson(null);
            }
        }
        catch(Exception err) {
            throw new WebRequestException("Error in JSON HTTP-BODY.", err);
        }
    }
}
