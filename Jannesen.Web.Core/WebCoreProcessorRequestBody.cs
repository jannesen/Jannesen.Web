using System;
using System.Collections.Generic;
using System.Net;

namespace Jannesen.Web.Core;

public sealed class WebCoreProcessorRequestBody: IWebCoreProcessor<WebCoreProcessResultRequestBody>
{
    public static readonly  IWebCoreProcessor<WebCoreProcessResultRequestBody>  Processor = new WebCoreProcessorRequestBody();

    public          IEnumerable<IWebCoreProcessor>      GetRequiredProcessors()
    {
        return [];
    }
    public          object                              Process(WebCoreCall httpCall)
    {
        ArgumentNullException.ThrowIfNull(httpCall);

        var sContentLength = httpCall.Request.GetHeader("Content-Length");

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

            return new WebCoreProcessResultRequestBody(httpCall.Request.ContentType, buf);
        }

        if (httpCall.Request.Headers["Transfer-Encoding"].Count > 0) {
            throw new WebHttpException(HttpStatusCode.LengthRequired, "Content-Length required.");
        }

        throw new WebHttpException(HttpStatusCode.BadRequest, "Missing body.");
    }
}
