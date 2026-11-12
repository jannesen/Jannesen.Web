using System;
using System.Buffers;
using System.IO;

namespace Jannesen.Web.Core;

internal sealed class WebCoreReadonlyMemoryStream: Stream
{
    private                     int                             _pos;
    private readonly            ReadOnlyMemory<byte>            _data;

    public      override        bool                            CanSeek         => true;
    public      override        bool                            CanRead         => true;
    public      override        bool                            CanWrite        => false;
    public      override        long                            Length          => _data.Length;
    public      override        long                            Position
    {
        get {
            return _pos;
        }
        set {
            if (value < 0 || value > int.MaxValue) {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _pos = (int)value;
        }
    }

    public                                                      WebCoreReadonlyMemoryStream(ReadOnlyMemory<byte> data)
    {
        _data = data;
    }

    public      override        void                            SetLength(long value)
    {
        throw new NotSupportedException("SetLength not allowed.");
    }
    public      override        long                            Seek(long offset, SeekOrigin origin)
    {
        switch(origin) {
        case SeekOrigin.Begin:      Position =  offset;         break;
        case SeekOrigin.Current:    Position += offset;         break;
        case SeekOrigin.End:        Position = Length - offset; break;
        default:                    throw new ArgumentOutOfRangeException(nameof(origin));
        }

        return Position;
    }
    public      override        void                            Flush()
    {
    }
    public      override        int                             Read(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);
        return Read(buffer.AsSpan(offset, count));
    }
    public      override        int                             Read(Span<byte> buffer)
    {
        var remaining = _data.Length - _pos;
        if (remaining <= 0) {
            return 0;
        }

        var rs = Math.Min(buffer.Length, remaining);
        _data.Span.Slice(_pos, rs).CopyTo(buffer);
        _pos += rs;
        return rs;
    }

    public      override        int                             ReadByte()
    {
        if (_pos >= Length) {
            return -1;
        }

        return _data.Span[_pos++];
    }
    public      override        void                            Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("Write not allowed.");
    }
    public      override        void                            Write(ReadOnlySpan<byte> buffer)
    {
        throw new NotSupportedException("Write not allowed.");
    }
    public      override        void                            WriteByte(byte value)
    {
        throw new NotSupportedException("Write not allowed.");
    }
    protected   override        void                            Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}
