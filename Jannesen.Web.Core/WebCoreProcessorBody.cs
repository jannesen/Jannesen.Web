using System;
using System.IO;
using System.Net;
using Jannesen.FileFormat.Json;

namespace Jannesen.Web.Core;

#pragma warning disable CA1819 // Properties should not return arrays

public sealed class WebCoreProcessorBody: IWebCoreProcessor
{
    public static readonly  IWebCoreProcessor    Definition = new WebCoreProcessorBody();

    public          string          Name    => "BodyData";

    public          object          Process(WebCoreCall httpCall)
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

            return new WebCoreProcessResultBody(httpCall.Request.ContentType, buf);
        }

        if (httpCall.Request.Headers["Transfer-Encoding"].Count > 0) {
            throw new WebHttpException(HttpStatusCode.LengthRequired, "Content-Length required.");
        }

        throw new WebHttpException(HttpStatusCode.BadRequest, "Missing body.");
    }
}
