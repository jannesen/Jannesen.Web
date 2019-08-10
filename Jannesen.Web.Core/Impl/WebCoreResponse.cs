using System;
using System.IO;
using System.Net;
using System.Web;

namespace Jannesen.Web.Core.Impl
{
    public abstract class WebCoreResponse
    {
        public      abstract    void            Send(WebCoreCall call, HttpResponse response);
        public      abstract    void            WriteLoggingData(StreamWriter writer);

        private static readonly string[]        _compressors = new string[] { "gzip", "deflate" };
        public      static      string          GetResponseCompressionEncoding(WebCoreCall httpCall)
        {
            string s = httpCall.Request.Headers["Accept-Encoding"];

            if (!string.IsNullOrEmpty(s)) {
                for(int c = 0 ; c < _compressors.Length ; ++c) {
                    string compressor = _compressors[c];

                    int i = s.IndexOf(compressor, StringComparison.Ordinal);

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
