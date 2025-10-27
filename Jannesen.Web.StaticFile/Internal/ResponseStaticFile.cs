using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Jannesen.Web.StaticFile.Internal
{
    internal sealed class ResponseStaticFile: ResponseStatic
    {
        private readonly        string          _physicalPath;
        private readonly        long            _length;

        public                                  ResponseStaticFile(string contentType, bool cachepublic, string physicalPath, FileInfo fileinfo): base(contentType, cachepublic, fileinfo.LastWriteTimeUtc, "W/\"" + fileinfo.LastWriteTimeUtc.ToFileTimeUtc().ToString("x8", CultureInfo.InvariantCulture) + "\"")
        {
            _physicalPath = physicalPath;
            _length       = fileinfo.Length;
        }

        protected   override    void            SendBodyData(HttpResponse response)
        {
            using(var stream = File.Open(_physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                stream.CopyTo(response.Body);
            }
        }

        public      override    void            WriteLoggingData(StreamWriter writer)
        {
            writer.Write("[FILE: ");
            writer.Write(_physicalPath);
            writer.WriteLine("]");
        }
    }
}
