using System;
using Jannesen.FileFormat.Json;

namespace Jannesen.Web.Core;

public sealed class WebCoreProcessResultTextJson(object? document)
{
    public          object?         Document        => document;

    public          bool            GetValue(string name, out object? rtn)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (document == null)
            throw new WebRequestException("Empty body or null.");

        if (!(document is JsonObject docObject))
            throw new WebRequestException("JSON root must by a object");

        if (name.IndexOf('.', StringComparison.Ordinal) < 0)
            return docObject.TryGetValue(name, out rtn);

        var obj = document;

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
