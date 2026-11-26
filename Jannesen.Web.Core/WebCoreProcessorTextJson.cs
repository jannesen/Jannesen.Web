using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jannesen.FileFormat.Json;

namespace Jannesen.Web.Core;

public sealed class WebCoreProcessorTextJson: IWebCoreProcessor<WebCoreProcessResultTextJson>
{
    public static readonly  IWebCoreProcessor<WebCoreProcessResultTextJson> Processor = new WebCoreProcessorTextJson();

    public          IEnumerable<IWebCoreProcessor>      GetRequiredProcessors()
    {
        return [ WebCoreProcessorRequestBody.Processor ];
    }
    public          ValueTask<object>                   ProcessAsync(WebCoreCall httpCall)
    {
        ArgumentNullException.ThrowIfNull(httpCall);

        try {
            var stream = httpCall.RequestBody.GetStreamReader("application/json");
            if (stream != null) {
                using(var reader = new JsonReader(stream)) {
                    return ValueTask.FromResult((object)new WebCoreProcessResultTextJson(reader.ParseDocument()));
                }
            }
            else {
                return ValueTask.FromResult((object)new WebCoreProcessResultTextJson(null));
            }
        }
        catch(TaskCanceledException) {
            throw;
        }
        catch(Exception err) {
            throw new WebRequestException("Error in JSON HTTP-BODY.", err);
        }
    }
}
