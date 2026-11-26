using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Jannesen.Web.StaticFile.Internal;

internal sealed class ResponseStaticFile: ResponseStatic
{
    private readonly            string          _physicalPath;
    private readonly            long            _length;

    public                                      ResponseStaticFile(string contentType, string physicalPath, FileInfo fileinfo): base(contentType, fileinfo.LastWriteTimeUtc, "W/\"" + fileinfo.LastWriteTimeUtc.ToFileTimeUtc().ToString("x8", CultureInfo.InvariantCulture) + "\"")
    {
        _physicalPath = physicalPath;
        _length       = fileinfo.Length;
    }

    protected override          Task            SendBodyDataAsync(HttpResponse response)
    {
        response.ContentLength = _length;
        return response.SendFileAsync(_physicalPath);
    }
    public    override    async Task            WriteLoggingDataAsync(StreamWriter writer)
    {
        await writer.WriteAsync("[FILE: ");
        await writer.WriteAsync(_physicalPath);
        await writer.WriteLineAsync("]");
    }
}
