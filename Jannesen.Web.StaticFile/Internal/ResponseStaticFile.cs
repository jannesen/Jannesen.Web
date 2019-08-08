using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;

namespace Jannesen.Web.StaticFile.Internal
{
    class ResponseStaticFile: ResponseStatic
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
            response.TransmitFile(_physicalPath, 0, _length);
        }

        public      override    void            WriteLoggingData(StreamWriter writer)
        {
            writer.Write("[FILE: ");
            writer.Write(_physicalPath);
            writer.WriteLine("]");
        }
    }
}
