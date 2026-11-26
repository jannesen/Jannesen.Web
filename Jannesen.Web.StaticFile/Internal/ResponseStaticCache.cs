using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Jannesen.Web.StaticFile.Internal;

internal sealed class ResponseStaticCache: ResponseStatic
{
    private readonly            FileCache       _fileCache;

    public                                      ResponseStaticCache(string contentType, FileCache fileCache) : base(contentType, null, fileCache.ETag)
    {
        _fileCache = fileCache;
    }

    protected override          Task            SendBodyDataAsync(HttpResponse response)
    {
        response.Headers.ContentEncoding = _fileCache.ContentEncoding;
        var data = _fileCache.Data;
        response.Headers.ContentLength   = data.Length;
        return response.Body.WriteAsync(_fileCache.Data, 0, data.Length);
    }
    public    override    async Task            WriteLoggingDataAsync(StreamWriter writer)
    {
        await writer.WriteAsync("[FILE-CACHE: ");
        await writer.WriteAsync(_fileCache.PhysicalPath);
        await writer.WriteLineAsync("]");
    }
}
