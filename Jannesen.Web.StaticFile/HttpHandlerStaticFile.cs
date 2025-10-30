using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Jannesen.Web.Core;
using Jannesen.Web.Core.Impl;

#pragma warning disable CA3003 // Potential file path injection vulnerability was found where (_mapPhysicalPath has a path validator);

namespace Jannesen.Web.StaticFile
{
    [WebCoreHttpHandlerAttribute("staticfile")]
    public class HttpHandlerStaticFile: WebCoreHttpHandler
    {
        private readonly        string              _directory;
        private readonly        string              _mimetype;
        private readonly        bool                _compress;
        private readonly        int                 _cacheMaxAge;
        private readonly        int                 _versionCacheMaxAge;

        public  override        string              Mimetype            => _mimetype;

        public                                      HttpHandlerStaticFile(WebCoreConfigReader configReader): base(configReader)
        {
            ArgumentNullException.ThrowIfNull(configReader);

            _directory           = System.IO.Path.GetDirectoryName(configReader.Filename) ?? throw new InvalidOperationException("Can't determin directory.");
            _mimetype            = configReader.GetValueString("mimetype");
            _compress            = configReader.GetValueBool("compress", false);
            _cacheMaxAge         = configReader.GetValueInt("cache-max-age",         -1, 0, 30*24*60*60);
            _versionCacheMaxAge  = configReader.GetValueInt("version-cache-max-age", -1, 0, 30*24*60*60);

            configReader.NoChildElements();
        }

        public  override        WebCoreResponse     Process(WebCoreCall httpCall)
        {
            ArgumentNullException.ThrowIfNull(httpCall);

            var response     = (Internal.ResponseStatic?)null;
            var physicalPath = _mapPhysicalPath(httpCall);
            var fileinfo     = _getFileInfo(physicalPath);

            if (_compress && fileinfo.Length < 10000000) { // Only public and <10 M files are compressed
                var compressEncoding = WebCoreResponse.GetResponseCompressionEncoding(httpCall);
                var cacheKey         = (compressEncoding != null ? "TextFile/" + compressEncoding + "/" : "StaticFileUTF8//" ) + physicalPath;
                var webFileCache     = (Internal.FileCache?)httpCall.Cache.Get(cacheKey);

                if (webFileCache != null && webFileCache.HasData) {
                    if (fileinfo.Length           != webFileCache.FileLength ||
                        fileinfo.LastWriteTimeUtc != webFileCache.LastWriteTimeUtc) {
                        webFileCache = null;
                    }
                }

                if (webFileCache == null) {
                    webFileCache = new Internal.FileCache(physicalPath, compressEncoding, fileinfo);

                    if (this.Public) {
                        httpCall.Cache.Set(cacheKey, webFileCache,
                                           new MemoryCacheEntryOptions() {
                                               AbsoluteExpiration = DateTime.UtcNow.AddSeconds(15*60),
                                               Size               = webFileCache.FileLength
                                           });
                    }
                }

                if (webFileCache.HasData) {
                    response = webFileCache.GetCompressedResponse(this.Mimetype, this.Public);
                }
            }

            if (response == null)
                response = new Internal.ResponseStaticFile(this.Mimetype, this.Public, physicalPath, fileinfo);

            if (_versionCacheMaxAge >= 0 && !string.IsNullOrEmpty(httpCall.Request.Query["v"])) {
                response.CacheMaxAge = _versionCacheMaxAge;
            }
            else if (_cacheMaxAge >= 0)
            {
                response.CacheMaxAge = _cacheMaxAge;
            }

            return response;
        }

        private                 string              _mapPhysicalPath(WebCoreCall httpCall)
        {
            var path = httpCall.Request.Path.Value ?? throw new InvalidOperationException("path is null.");

            for (var i = 0; i < path.Length ; ++i) {
                var c = path[i];

                if ((c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9') ||
                    (c == '-' || c == '_' || c == '~')) {
                    continue;
                }

                if (c == '/' && (i == 0 || path[i-1] != '/')) {
                    continue;
                }

                if (c == '.' && i > 0 && path[i-1] != '/') {
                    continue;
                }

                throw new WebHttpException(HttpStatusCode.BadRequest, "Invalid character in url.");
            }

            return _directory + path.Replace("/", "\\", StringComparison.Ordinal);
        }
        private     static      FileInfo            _getFileInfo(string physicalPath)
        {
            FileInfo    fileinfo;

            try {
                fileinfo = new FileInfo(physicalPath);
            }
            catch(IOException) {
                throw new WebHttpException(HttpStatusCode.NotFound, "Resource not found or available.");
            }
            catch (System.Security.SecurityException) {
                throw new WebHttpException(HttpStatusCode.Unauthorized, "Unauthorized");
            }

            if ((!fileinfo.Exists) ||(fileinfo.Attributes & FileAttributes.Hidden) != (FileAttributes)0) {
                throw new WebHttpException(HttpStatusCode.NotFound, "Resource not found or available.");
            }

            if ((fileinfo.Attributes & FileAttributes.Directory) != (FileAttributes)0)
                throw new WebHttpException(HttpStatusCode.Forbidden, "Forbidden");

            return fileinfo;
        }
    }
}
