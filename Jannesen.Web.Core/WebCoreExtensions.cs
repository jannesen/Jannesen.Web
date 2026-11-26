using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Jannesen.Web.Core;

public static class WebCoreExtensions
{
    extension(HttpRequest request) {
        public  string?         GetHeader(string name)
        {
            ArgumentNullException.ThrowIfNull(request);
            var h = request.Headers[name];
            switch(h.Count) {
            case 0: return null;
            case 1: return h[0];
            default: throw new WebHttpException(HttpStatusCode.BadRequest, "Multiple '" + name + "' headers.");
            }

        }
        public  DateTime?       IfModifiedSince
        {
            get {
                var s = request.GetHeader("If-Modified-Since");

                if (!string.IsNullOrEmpty(s)) {
                    if (DateTime.TryParseExact(s, "R", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.AdjustToUniversal, out var rtn))
                        return rtn;
                }

                return null;
            }
        }
        public  string?         IfNoneMatch
        {
            get {
                var s = request.GetHeader("If-None-Match");

                return !string.IsNullOrEmpty(s) ? s : null;
            }
        }
        public  string?         Referer
        {
            get {
                var s = request.GetHeader("Referer");

                return !string.IsNullOrEmpty(s) ? s : null;
            }
        }
        public  string?         UserAgent
        {
            get {
                var s = request.GetHeader("User-Agent");

                return !string.IsNullOrEmpty(s) ? s : null;
            }
        }
    }

    extension(HttpResponse response) {
        public  ValueTask       SendBufferAsync(ReadOnlyMemory<byte> data)
        {
            ArgumentNullException.ThrowIfNull(response);
            response.HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
            response.ContentLength = data.Length;
            return response.Body.WriteAsync(data);
        }
    }

    extension(WebCoreCall call) {
        public  string?         RemoteAddr
        {
            get {
                ArgumentNullException.ThrowIfNull(call);
                var rtn = call.Request.GetHeader("X-Forwarded-For");

                if (!string.IsNullOrEmpty(rtn)) {
                    var p = rtn.IndexOf(',', StringComparison.Ordinal);
                    if (p > 0) {
                        rtn = rtn.Substring(0, p);
                    }
                }
                else {
                    rtn = call.Context.Connection.RemoteIpAddress?.ToString();
                }

                return !string.IsNullOrEmpty(rtn) ? rtn : null;
            }
        }
    }
}
