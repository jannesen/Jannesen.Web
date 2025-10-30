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
        private readonly        bool                        _decodeCharSet;

        public                  string                      PhysicalPath            => _physicalPath;
        public                  string?                     ContentEncoding         => _contentEncoding;
        public                  bool                        DecodeCharSet           => _decodeCharSet;
        public                  bool                        HasData                 => _data != null;
        public                  byte[]                      Data                    => _data ?? throw new InvalidOperationException("FileCache has with data.");
        public                  int                         FileLength              => Data.Length;
        public                  DateTime                    LastWriteTimeUtc        => _lastWriteTimeUtc;
        public                  string                      ETag                    => _eTag;

        public                                              FileCache(string physicalPath, string? contentEncoding, FileInfo fileinfo, bool decodeCharSet)
        {
            _physicalPath    = physicalPath;
            _contentEncoding = contentEncoding;
            _decodeCharSet   = decodeCharSet;

            using (var outBuffer = new MemoryStream((int)fileinfo.Length)) {
                using (var inStream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (var sha = System.Security.Cryptography.SHA1.Create()) {
                        _eTag = Convert.ToBase64String(sha.ComputeHash(inStream)).Substring(0, 26).Replace('/','-');

                        inStream.Seek(0, SeekOrigin.Begin);
                    }

                    var compressStream = WebCoreResponse.GetCompressor(contentEncoding, outBuffer);

                    if (decodeCharSet)
                        _decodeAndCopy(inStream, compressStream);
                    else
                        inStream.CopyTo(compressStream);

                    if (compressStream != outBuffer)
                        compressStream.Close();
                }

                if (outBuffer.Length < fileinfo.Length || decodeCharSet) {
                    _data = outBuffer.ToArray();
                }
            }

            _lastWriteTimeUtc = fileinfo.LastWriteTimeUtc;
//          _eTag             = "W/\"" + fileinfo.LastWriteTimeUtc.ToFileTimeUtc().ToString("x8", CultureInfo.InvariantCulture) + "\"";
        }

        public                  ResponseStaticCache         GetCompressedResponse(string contentType, bool publicCache)
        {
            if (_data == null)
                throw new InternalErrorException("Compressed data not available.");

            if (_decodeCharSet)
                contentType += "; charset=utf-8";

            return new ResponseStaticCache(contentType, publicCache, this);
        }

        private static          void                        _decodeAndCopy(FileStream inStream, Stream outStream)
        {
            var buf = new byte[81920];

            var rs = inStream.Read(buf, 0, buf.Length);

            if (buf[0] == 0xEF && buf[1] == 0xBB && buf[2] == 0xBF) {
                outStream.Write(buf, 3, rs - 3);
                inStream.CopyTo(outStream);
            }
            else {
                int off;
                int cs;
                int bs;
                var cbuf = new char[buf.Length];
                Decoder decoder;
                var encoder = Encoding.UTF8.GetEncoder();

                if (buf[0] == 0xFF && buf[1] == 0xFE)
                    throw new NotSupportedException("UTF-16 LE not supported.");

                if (buf[0] == 0xFE && buf[1] == 0xFF) {
                    off = 2;
                    decoder = Encoding.BigEndianUnicode.GetDecoder();
                }
                else {
                    off = 0;
                    decoder = Encoding.GetEncoding("Windows-1255").GetDecoder();
                }

                if (rs > off) {
                    do {
                        if ((cs = decoder.GetChars(buf, off, rs - off, cbuf, 0, false)) > 0) {
                            if ((bs = encoder.GetBytes(cbuf, 0, cs, buf, 0, false)) > 0)
                                outStream.Write(buf, 0, bs);
                        }

                        off = 0;
                    }
                    while ((rs = inStream.Read(buf, 0, buf.Length)) > 0);

                    if ((cs = decoder.GetChars(buf, off, rs - off, cbuf, 0, true)) > 0) {
                        if ((bs = encoder.GetBytes(cbuf, 0, cs, buf, 0, true)) > 0)
                            outStream.Write(buf, 0, bs);
                    }
                }
            }
        }
    }
}
