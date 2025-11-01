using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Jannesen.Web.StaticFile.Internal
{
    internal sealed class ResponseStaticFile: ResponseStatic
    {
        private readonly        string          _physicalPath;

        public                                  ResponseStaticFile(string contentType, bool cachepublic, string physicalPath, FileInfo fileinfo): base(contentType, cachepublic, fileinfo.LastWriteTimeUtc, "W/\"" + fileinfo.LastWriteTimeUtc.ToFileTimeUtc().ToString("x8", CultureInfo.InvariantCulture) + "\"")
        {
            _physicalPath = physicalPath;
        }

        protected   override    void            SendBodyData(HttpResponse response)
        {
            using(var stream = File.Open(_physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                response.ContentLength = stream.Length;
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
