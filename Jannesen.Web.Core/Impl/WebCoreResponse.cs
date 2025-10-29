using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Jannesen.Web.Core.Impl
{
    public abstract class WebCoreResponse
    {
        public      abstract    void            Send(WebCoreCall call, HttpResponse response);
        public      abstract    void            WriteLoggingData(StreamWriter writer);

        private static readonly string[]        _compressors = [ "gzip", "deflate" ];

        public      static      string          GetResponseCompressionEncoding(WebCoreCall httpCall)
        {
            ArgumentNullException.ThrowIfNull(httpCall);

            var s = httpCall.GetHeader("Accept-Encoding");

            if (!string.IsNullOrEmpty(s)) {
                for(var c = 0 ; c < _compressors.Length ; ++c) {
                    var compressor = _compressors[c];
                    var i = s.IndexOf(compressor, StringComparison.Ordinal);

                    if (i >= 0) {
                        i += compressor.Length;

                        if (i >= s.Length || s[i] == ',')
                            return compressor;
                    }
                }
            }

            return null;
        }
        public      static      Stream          GetCompressor(string compressor, Stream outstream)
        {
            switch(compressor) {
            case null:      return outstream;
            case "gzip":    return new System.IO.Compression.GZipStream   (outstream, System.IO.Compression.CompressionMode.Compress, true);
            case "deflate": return new System.IO.Compression.DeflateStream(outstream, System.IO.Compression.CompressionMode.Compress, true);
            default:        throw new NotImplementedException("Unknown compressor '" + compressor + "'.");
            }
        }
    }
}
