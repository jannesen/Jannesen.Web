using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Jannesen.Web.Core.Impl
{
    public static class WebCoreExtensions
    {
        public      static      ReadOnlyMemory<byte>        GetReadOnlyData(this MemoryStream memoryStream)
        {
            ArgumentNullException.ThrowIfNull(memoryStream);
            return new ReadOnlyMemory<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }

        public      static      void                        SendBuffer(this HttpResponse response, ReadOnlySpan<byte> data)
        {
            ArgumentNullException.ThrowIfNull(response);
            response.HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
            response.ContentLength = data.Length;
            response.Body.Write(data);
        }
    }
}
