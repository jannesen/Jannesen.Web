using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Jannesen.Web.Core;

public sealed class WebCoreProcessResultRequestBody(string? contentType, byte[] body)
{
    public          string?         ContentType     => contentType;
    public          int             ContentLength   => body.Length;
    [SuppressMessage("Performance", "CA1819", Justification = "Microsoft.Data.SqlClient don't support ReadonlyMemory<byte> https://github.com/dotnet/SqlClient/issues/4491")]
    public          byte[]          Data            => body;

    public          Stream          GetStream()
    {
        return new WebCoreReadonlyMemoryStream(body);
    }
    public          StreamReader?   GetStreamReader(string contenttype)
    {
        if (string.IsNullOrEmpty(contentType)) {
            if (body.Length <= 0) {
                return null;
            }

            throw new WebRequestException("Missing Content-Type.");
        }

        var encoding         = (System.Text.Encoding?)null;
        var contentTypeParts = contentType.Split(';');

        for (var i = 1 ; i <contentTypeParts.Length ; ++i)
            contentTypeParts[i] = contentTypeParts[i].TrimStart();

        if (contenttype != null && contentTypeParts[0] != contenttype)
            throw new WebRequestException("Expect " + contenttype + " body.");

        for (var i = 1 ; i < contentTypeParts.Length ; ++i) {
            if (contentTypeParts[i].StartsWith("charset=", StringComparison.Ordinal)) {
                var charset     = contentTypeParts[i].Substring(8);

                switch(charset.ToLowerInvariant()) {
                case "unicode": encoding = System.Text.Encoding.Unicode;    break;
                case "utf-8":   encoding = System.Text.Encoding.UTF8;       break;
                default:        throw new NotImplementedException("Invalid characterset '" + charset + "'.");
                }
            }
        }

        if (encoding == null)
            throw new WebRequestException("Missing charset in Content-Type " + contentType + ".");

        return new StreamReader(GetStream(), encoding, false, 4096, false);
    }
    public          string?         GetString(string contenttype)
    {
        using(var reader = GetStreamReader(contenttype)) {
            return reader?.ReadToEnd();
        }
    }
}
