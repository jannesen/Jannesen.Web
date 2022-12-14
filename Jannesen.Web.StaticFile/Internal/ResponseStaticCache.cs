using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;

namespace Jannesen.Web.StaticFile.Internal
{
    internal sealed class ResponseStaticCache: ResponseStatic
    {
        private readonly        FileCache       _fileCache;

        public                                  ResponseStaticCache(string contentType, bool publicCache, FileCache fileCache) : base(contentType, publicCache, null, fileCache.ETag)
        {
            _fileCache = fileCache;
        }

        protected   override    void            SendBodyData(HttpResponse response)
        {
            response.AppendHeader("Content-Encoding", _fileCache.ContentEncoding);
            response.AppendHeader("Content-Length",   _fileCache.Data.Length.ToString(CultureInfo.InvariantCulture));
            response.OutputStream.Write(_fileCache.Data, 0, _fileCache.Data.Length);
        }

        public      override    void            WriteLoggingData(StreamWriter writer)
        {
            writer.Write("[FILE-CACHE: ");
            writer.Write(_fileCache.PhysicalPath);
            writer.WriteLine("]");
        }
    }
}
