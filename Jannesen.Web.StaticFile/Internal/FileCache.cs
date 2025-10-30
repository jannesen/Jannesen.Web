using System;
using System.IO;
using System.Text;
using Jannesen.Web.Core.Impl;

#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms (SHA1 used for etag)

namespace Jannesen.Web.StaticFile.Internal
{
    internal sealed class FileCache
    {
        private readonly        string                      _physicalPath;
        private readonly        string?                     _contentEncoding;
        private readonly        byte[]?                     _data;
        private readonly        DateTime                    _lastWriteTimeUtc;
        private readonly        string                      _eTag;

        public                  string                      PhysicalPath            => _physicalPath;
        public                  string?                     ContentEncoding         => _contentEncoding;
        public                  bool                        HasData                 => _data != null;
        public                  byte[]                      Data                    => _data ?? throw new InvalidOperationException("FileCache has with data.");
        public                  int                         FileLength              => Data.Length;
        public                  DateTime                    LastWriteTimeUtc        => _lastWriteTimeUtc;
        public                  string                      ETag                    => _eTag;

        public                                              FileCache(string physicalPath, string? contentEncoding, FileInfo fileinfo)
        {
            _physicalPath    = physicalPath;
            _contentEncoding = contentEncoding;

            using (var outBuffer = new MemoryStream((int)fileinfo.Length)) {
                using (var inStream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (var sha = System.Security.Cryptography.SHA1.Create()) {
                        _eTag = Convert.ToBase64String(sha.ComputeHash(inStream)).Substring(0, 26).Replace('/','-');

                        inStream.Seek(0, SeekOrigin.Begin);
                    }

                    var compressStream = WebCoreResponse.GetCompressor(contentEncoding, outBuffer);

                    inStream.CopyTo(compressStream);

                    if (compressStream != outBuffer)
                        compressStream.Close();
                }

                if (outBuffer.Length < fileinfo.Length) {
                    _data = outBuffer.ToArray();
                }
            }

            _lastWriteTimeUtc = fileinfo.LastWriteTimeUtc;
        }

        public                  ResponseStaticCache         GetCompressedResponse(string contentType, bool publicCache)
        {
            if (_data == null)
                throw new InternalErrorException("Compressed data not available.");

            return new ResponseStaticCache(contentType, publicCache, this);
        }
    }
}
