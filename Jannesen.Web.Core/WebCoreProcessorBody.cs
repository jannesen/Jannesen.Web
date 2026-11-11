using System;
using System.IO;
using System.Net;
using Jannesen.FileFormat.Json;

namespace Jannesen.Web.Core;

#pragma warning disable CA1819 // Properties should not return arrays

public class WebCoreProcessorBody
{
    public static readonly  IWebCoreProcessorDefinition    Definition = new DefinitionClass();

    internal sealed class DefinitionClass: IWebCoreProcessorDefinition
    {
        public          string          Name    => "BodyData";

        public          object          Create(WebCoreCall httpCall)
        {
            ArgumentNullException.ThrowIfNull(httpCall);

            var sContentLength = httpCall.GetHeader("Content-Length");

            if (!string.IsNullOrEmpty(sContentLength) && int.TryParse(sContentLength, out var contentLength)) {
                var buf = new byte[contentLength];

                using (var inputStream = httpCall.Request.Body) {
                    var size = 0;
                    int rs;

                    while (size < contentLength && (rs = inputStream.Read(buf, size, contentLength - size)) > 0) {
                        size += rs;
                    }

                    if (size != contentLength) {
                        throw new WebRequestException("BODY is incomplete.");
                    }
                }

                return new WebCoreProcessorBody(httpCall.Request.ContentType, buf);
            }

            if (httpCall.Request.Headers["Transfer-Encoding"].Count > 0) {
                throw new WebHttpException(HttpStatusCode.LengthRequired, "Content-Length required.");
            }

            throw new WebHttpException(HttpStatusCode.BadRequest, "Missing body.");
        }
    }

    private readonly        string?                 _contentType;
    private readonly        byte[]                  _body;

    public                  string?                 ContentType     => _contentType;
    public                  byte[]                  Data            => _body; // Voor nu byte[] omdat Microsoft.Data.SqlClient ReadonlyMemory niet ondersteund.

    internal                                        WebCoreProcessorBody(string? contenType, byte[] body)
    {
        _contentType = contenType;
        _body        = body;
    }

    public                  Stream                  GetStream()
    {
        return new ReadonlyMemoryStream(_body);
    }
    public                  StreamReader?           GetStreamReader(string contenttype)
    {
        if (string.IsNullOrEmpty(_contentType)) {
            if (_body.Length <= 0) {
                return null;
            }

            throw new WebRequestException("Missing Content-Type.");
        }

        var encoding         = (System.Text.Encoding?)null;
        var contentTypeParts = _contentType.Split(';');

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
            throw new WebRequestException("Missing charset in Content-Type " + _contentType + ".");

        return new StreamReader(GetStream(), encoding, false, 4096, false);
    }
    public                  string?                 GetString(string contenttype)
    {
        using(var reader = GetStreamReader(contenttype)) {
            return reader?.ReadToEnd();
        }
    }
}
