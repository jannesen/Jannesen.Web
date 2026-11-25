using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using Jannesen.Web.Core;

namespace Jannesen.Web.StaticFile.Internal;

internal sealed class FileCache
{
    private readonly        string                      _physicalPath;
    private readonly        DateTime                    _lastWriteTimeUtc;
    private readonly        string                      _eTag;
    private readonly        string?                     _encoding;
    private readonly        byte[]?                     _data;

    public                  string                      PhysicalPath            => _physicalPath;
    public                  DateTime                    LastWriteTimeUtc        => _lastWriteTimeUtc;
    public                  string                      ETag                    => _eTag;
    public                  bool                        HasData                 => _data != null;
    public                  string?                     ContentEncoding         => _encoding;
    public                  byte[]                      Data                    => _data ?? throw new InvalidOperationException("FileCache has with data.");
    public                  int                         FileLength              => Data.Length;

    [SuppressMessage("Performance", "CA5350", Justification = "Using SHA1 To calculate a ETAG")]
    public                                              FileCache(string physicalPath, FileInfo fileinfo, string encoding)
    {
        _physicalPath     = physicalPath;
        _lastWriteTimeUtc = fileinfo.LastWriteTimeUtc;

        using (var inStream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
            using (var sha = SHA1.Create()) {
                _eTag = Convert.ToBase64String(sha.ComputeHash(inStream)).Substring(0, 26).Replace('/','-');
                inStream.Seek(0, SeekOrigin.Begin);
            }

            var compressData = WebCoreResponseCompressor.Compress(encoding, inStream);

            if (compressData.Length < inStream.Length) {
                _encoding = encoding;
                _data     = compressData;
            }
        }
    }

    public                  ResponseStaticCache         GetCompressedResponse(string contentType)
    {
        if (_data == null) {
            throw new InternalErrorException("Compressed data not available.");
        }

        return new ResponseStaticCache(contentType, this);
    }
}
