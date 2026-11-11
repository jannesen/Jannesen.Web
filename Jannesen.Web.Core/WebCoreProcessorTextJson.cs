using System;
using Jannesen.FileFormat.Json;

namespace Jannesen.Web.Core;

public class WebCoreProcessorTextJson
{
    public static readonly  IWebCoreProcessorDefinition    Definition = new DefinitionClass();

    internal sealed class DefinitionClass: IWebCoreProcessorDefinition
    {
        public          string          Name    => "TextJson";

        public          object          Create(WebCoreCall httpCall)
        {
            ArgumentNullException.ThrowIfNull(httpCall);
            return new WebCoreProcessorTextJson(httpCall);
        }
    }

    private readonly        object?             _document;

    public                  object?             Document        => _document;

    internal                                    WebCoreProcessorTextJson(WebCoreCall httpCall)
    {
        try {
            var stream = httpCall.GetBodyText("application/json");
            if (stream != null) {
                using(var reader = new JsonReader(stream)) {
                    _document = reader.ParseDocument();
                }
            }
        }
        catch(Exception err) {
            throw new WebRequestException("Error in JSON HTTP-BODY.", err);
        }
    }

    public                  bool                GetValue(string name, out object? rtn)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_document == null)
            throw new WebRequestException("Empty body or null.");

        if (!(_document is JsonObject docObject))
            throw new WebRequestException("JSON root must by a object");

        if (name.IndexOf('.', StringComparison.Ordinal) < 0)
            return docObject.TryGetValue(name, out rtn);

        var obj = _document;

        foreach(var n in name.Split('.')) {
            if (!(obj is JsonObject jsonObject)) {
                throw new WebRequestException("Invalid Json schema.");
            }

            if (n == "$") {
                obj = JsonWriter.Stringify(jsonObject);
            }
            else {
                if (!jsonObject.TryGetValue(n, out obj)) {
                    rtn = null;
                    return false;
                }
            }
        }

        rtn = obj;
        return true;
    }
}
