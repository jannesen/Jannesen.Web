using System;
using System.IO;
using System.Text;
using System.Web;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.StaticFile.Internal
{
    class FileCache
    {
        private                 string                      _physicalPath;
        private                 string                      _contentEncoding;
        private                 byte[]                      _data;
        private                 DateTime                    _lastWriteTimeUtc;
        private                 string                      _eTag;
        private                 bool                        _decodeCharSet;

        public                  string                      PhysicalPath
        {
            get {
                return _physicalPath;
            }
        }
        public                  string                      ContentEncoding
        {
            get {
                return _contentEncoding;
            }
        }
        public                  bool                        DecodeCharSet
        {
            get {
                return _decodeCharSet;
            }
        }
        public                  bool                        HasData
        {
            get {
                return _data != null;
            }
        }
        public                  byte[]                      Data
        {
            get {
                return _data;
            }
        }
        public                  int                         FileLength
        {
            get {
                return _data.Length;
            }
        }
        public                  DateTime                    LastWriteTimeUtc
        {
            get {
                return _lastWriteTimeUtc;
            }
        }
        public                  string                      ETag
        {
            get {
                return _eTag;
            }
        }

        public                                              FileCache(string physicalPath, string contentEncoding, FileInfo fileinfo, bool decodeCharSet)
        {
            _physicalPath    = physicalPath;
            _contentEncoding = contentEncoding;
            _decodeCharSet   = decodeCharSet;

            using (MemoryStream outBuffer = new MemoryStream((int)fileinfo.Length))
            {
                using (Stream inStream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (System.Security.Cryptography.SHA1 sha = System.Security.Cryptography.SHA1.Create())
                    {
                        _eTag = Convert.ToBase64String(sha.ComputeHash(inStream)).Substring(0, 26).Replace('/','-');

                        inStream.Seek(0, SeekOrigin.Begin);
                    }

                    Stream compressStream = WebCoreResponse.GetCompressor(contentEncoding, outBuffer);

                    if (decodeCharSet)
                        _decodeAndCopy(inStream, compressStream);
                    else
                        inStream.CopyTo(compressStream);

                    if (compressStream != outBuffer)
                        compressStream.Close();
                }

                if (outBuffer.Length < fileinfo.Length || decodeCharSet)
                    _data = outBuffer.ToArray();
            }

            _lastWriteTimeUtc = fileinfo.LastWriteTimeUtc;
//          _eTag             = "W/\"" + fileinfo.LastWriteTimeUtc.ToFileTimeUtc().ToString("x8", System.Globalization.CultureInfo.InvariantCulture) + "\"";
        }

        public                  ResponseStaticCache         GetCompressedResponse(string contentType, bool publicCache)
        {
            if (_data == null)
                throw new InternalErrorException("Compressed data not available.");

            if (_decodeCharSet)
                contentType += "; charset=utf-8";

            return new ResponseStaticCache(contentType, publicCache, this);
        }

        private                 void                        _decodeAndCopy(Stream inStream, Stream outStream)
        {
            byte[] buf = new byte[81920];

            int rs = inStream.Read(buf, 0, buf.Length);

            if (buf[0] == 0xEF && buf[1] == 0xBB && buf[2] == 0xBF) {
                outStream.Write(buf, 3, rs - 3);
                inStream.CopyTo(outStream);
            }
            else {
                int off;
                int cs;
                int bs;
                char[] cbuf = new char[buf.Length];
                Decoder decoder;
                Encoder encoder = Encoding.UTF8.GetEncoder();

                if (buf[0] == 0xFF && buf[0] == 0xFE)
                    throw new NotSupportedException("UTF-16 LE not supported.");

                if (buf[0] == 0xFE && buf[0] == 0xFF) {
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
