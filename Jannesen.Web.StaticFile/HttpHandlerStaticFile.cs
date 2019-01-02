using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Security;
using System.Text;
using Jannesen.Web.Core;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.StaticFile
{
    [WebCoreAttribureHttpHandler("staticfile")]
    public class HttpHandlerStaticFile: WebCoreHttpHandler
    {
        private                 string              _mimetype;
        private                 bool                _compress;
        private                 int                 _cacheMaxAge;
        private                 int                 _versionCacheMaxAge;
        private                 bool                _decodeCharSet;

        public  override        string              Mimetype
        {
            get {
                return _mimetype;
            }
        }

        public                                      HttpHandlerStaticFile(WebCoreConfigReader configReader): base(configReader)
        {
            _mimetype            = configReader.GetValueString("mimetype");
            _compress            = configReader.GetValueBool("compress", false);
            _cacheMaxAge         = configReader.GetValueInt("cache-max-age",         -1, 0, 30*24*60*60);
            _versionCacheMaxAge  = configReader.GetValueInt("version-cache-max-age", -1, 0, 30*24*60*60);
            _decodeCharSet       = configReader.GetValueBool("decode-charset", false);

            configReader.NoChildElements();
        }

        public  override        WebCoreResponse     Process(WebCoreCall httpCall)
        {
            Internal.ResponseStatic     response     = null;
            string                      physicalPath = httpCall.Request.PhysicalPath;
            FileInfo                    fileinfo     = GetFileInfo(physicalPath);

            if ((_compress || _decodeCharSet) && fileinfo.Length < 10000000) // Only public and <10 M files are compressed
            {
                string                  compressEncoding  = WebCoreResponse.GetResponseCompressionEncoding(httpCall);
                string                  cacheKey          = (compressEncoding != null ? "TextFile/" + compressEncoding + "/" : "StaticFileUTF8//" ) + physicalPath;
                Internal.FileCache      webFileCache      = (Internal.FileCache)httpCall.Cache.Get(cacheKey);

                if (webFileCache != null && webFileCache.HasData) {
                    if (fileinfo.Length           != webFileCache.FileLength ||
                        fileinfo.LastWriteTimeUtc != webFileCache.LastWriteTimeUtc)
                        webFileCache = null;
                }

                if (webFileCache == null) {
                    webFileCache = new Internal.FileCache(physicalPath, compressEncoding, fileinfo, _decodeCharSet);

                    if (this.Public)
                        httpCall.Cache.Insert(cacheKey, webFileCache, null, DateTime.UtcNow.AddSeconds(15*60), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
                }

                if (webFileCache.HasData)
                    response = webFileCache.GetCompressedResponse(this.Mimetype, this.Public);
            }

            if (response == null)
                response = new Internal.ResponseStaticFile(this.Mimetype, this.Public, physicalPath, fileinfo);

            if (_versionCacheMaxAge >= 0 && !string.IsNullOrEmpty(httpCall.Request.QueryString["v"])) {
                response.CacheMaxAge = _versionCacheMaxAge;
            }
            else if (_cacheMaxAge >= 0)
            {
                response.CacheMaxAge = _cacheMaxAge;
            }

            return response;
        }

        protected               FileInfo            GetFileInfo(string physicalPath)
        {
            FileInfo    fileinfo;

            try {
                fileinfo = new FileInfo(physicalPath);
            }
            catch(IOException) {
                throw new HttpException(404, "Resource not found or available.");
            }
            catch (System.Security.SecurityException) {
                throw new HttpException(401, "Unauthorized");
            }

            if ((!fileinfo.Exists) ||(fileinfo.Attributes & FileAttributes.Hidden) != (FileAttributes)0)
                throw new HttpException(404, "Resource not found or available.");

            if ((fileinfo.Attributes & FileAttributes.Directory) != (FileAttributes)0)
                throw new HttpException(403, "Forbidden");

            return fileinfo;
        }
    }
}
