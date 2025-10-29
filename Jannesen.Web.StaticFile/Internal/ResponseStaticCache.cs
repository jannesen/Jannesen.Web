using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Http;

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
            response.Headers.ContentEncoding = _fileCache.ContentEncoding;
            response.Headers.ContentLength   = _fileCache.Data.Length;
            response.Body.Write(_fileCache.Data, 0, _fileCache.Data.Length);
        }

        public      override    void            WriteLoggingData(StreamWriter writer)
        {
            writer.Write("[FILE-CACHE: ");
            writer.Write(_fileCache.PhysicalPath);
            writer.WriteLine("]");
        }
    }
}
