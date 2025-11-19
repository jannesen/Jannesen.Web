using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;

#pragma warning disable CA1815 // Override equals and operator equals on value types

namespace Jannesen.Web.Core.Impl
{
    public readonly struct WebCoreResponseCompressor
    {
        public                  ReadOnlyMemory<byte>    Data            { get; private init; }
        public                  string?                 Encoding        { get; private init; }

        private static readonly string[]                _compressors = [ "br", "gzip", "deflate" ];

        public                                          WebCoreResponseCompressor(HttpRequest request, ReadOnlyMemory<byte> data)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (data.Length >= 1024) {
                var encoding = GetResponseCompressionEncoding(request.Headers.AcceptEncoding);

                if (encoding != null) {
                    var bufferWriter = new ArrayBufferWriter<byte>(_initCapacity(data.Length));

                    using (var compressstream = _createCompressor(encoding, new WebCoreArraryBufferStream(bufferWriter))) {
                        compressstream.Write(data.Span);
                    }

                    var writen = bufferWriter.WrittenMemory;

                    if (writen.Length < data.Length) {
                        Encoding = encoding;
                        Data     = writen;
                    }
                }
            }
            else {
                Encoding = null;
                Data     = data;
            }
        }

        public      static      string?                 GetResponseCompressionEncoding(string? acceptEncoding)
        {
            var f = _compressors.Length;

            if (!string.IsNullOrEmpty(acceptEncoding)) {
                var encoding_pos = 0;

                // Walk to multiple encodings;
                while (encoding_pos < acceptEncoding.Length && f > 0) {
                    var encoding_end = acceptEncoding.IndexOf(',', encoding_pos);
                    if (encoding_end < 0) encoding_end = acceptEncoding.Length;

                    var encoding = acceptEncoding.AsSpan(encoding_pos, encoding_end-encoding_pos);
                    var j = encoding.IndexOf(";");
                    if (j > 0) {
                        encoding = encoding.Slice(0, j);
                    }

                    encoding = encoding.Trim();

                    for(var i = 0 ; i < f; ++i) {
                        if (MemoryExtensions.SequenceEqual(encoding, _compressors[i])) {
                            f = i;
                            break;
                        }
                    }

                    encoding_pos = encoding_end + 1;
                }

                if (f < _compressors.Length) {
                    return _compressors[f];
                }
            }

            return null;
        }
        public  static          byte[]                  Compress(string encoding, Stream input)
        {
            ArgumentNullException.ThrowIfNull(encoding);
            ArgumentNullException.ThrowIfNull(input);

            using (var outstream = new MemoryStream(0x1000)) {
                using (var compressstream = _createCompressor(encoding, outstream)) {
                    input.CopyTo(compressstream);
                }

                return outstream.ToArray();
            }
        }

        public                  void                    WriteTo(HttpResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);

            if (Encoding != null) {
                response.Headers.ContentEncoding = Encoding;
            }

            response.SendBuffer(Data.Span);
        }

        private     static      Stream                  _createCompressor(string encoding, Stream s)
        {
            return encoding switch {
                       "br"      => new BrotliStream (s, CompressionMode.Compress, false),
                       "gzip"    => new GZipStream   (s, CompressionMode.Compress, false),
                       "deflate" => new DeflateStream(s, CompressionMode.Compress, false),
                       _         => throw new InvalidOperationException("Unknown compressor '" + encoding + "'.")
                   };
        }
        private     static      int                     _initCapacity(int inlength)
        {
            return inlength switch {
                       <  4096     => inlength,
                       < 65536     => 4096 + ((inlength-4096)/2),
                       _           => 4096 + ((65536   -4096)/2) + ((inlength-65536)/4)
                   };
        }
    }
}
