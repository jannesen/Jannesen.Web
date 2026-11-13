using System;
using Jannesen.FileFormat.Json;

namespace Jannesen.Web.Core;

public sealed class WebCoreProcessorTextJson: IWebCoreProcessor
{
    public static readonly  IWebCoreProcessor    Definition = new WebCoreProcessorTextJson();

    public          string          Name    => "TextJson";

    public          object          Process(WebCoreCall httpCall)
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
