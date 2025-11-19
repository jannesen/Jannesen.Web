using System;
using System.Buffers;
using System.IO;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreArraryBufferStream: Stream
    {
        private                     ArrayBufferWriter<byte>?        _bufferWriter;

        public      override        bool                            CanSeek         => false;
        public      override        bool                            CanRead         => false;
        public      override        bool                            CanWrite        => _bufferWriter != null;
        public      override        long                            Length          { get; }
        public      override        long                            Position        { get { throw new NotSupportedException("get_Position not allowed."); }
                                                                                      set { throw new NotSupportedException("set_Position not allowed."); } }

        public                      ArrayBufferWriter<byte>         BufferWriter
        {
            get {
                ObjectDisposedException.ThrowIf(_bufferWriter == null, this);
                return _bufferWriter;
            }
        }
        internal                                                    WebCoreArraryBufferStream(ArrayBufferWriter<byte> bufferWriter)
        {
            _bufferWriter = bufferWriter;
        }

        public      override        void                            SetLength(long value)
        {
            throw new NotSupportedException("SetLength not allowed.");
        }
        public      override        long                            Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Seek not allowed.");
        }
        public      override        void                            Flush()
        {
        }
        public      override        int                             Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Read not allowed.");
        }
        public      override        void                            Write(byte[] buffer, int offset, int count)
        {
            Write(buffer.AsSpan(offset, count));
        }
        public      override        void                            Write(ReadOnlySpan<byte> buffer)
        {
            ObjectDisposedException.ThrowIf(_bufferWriter == null, this);

            buffer.CopyTo(_bufferWriter.GetSpan(buffer.Length));
            _bufferWriter.Advance(buffer.Length);
        }
        public      override        void                            WriteByte(byte value)
        {
            ObjectDisposedException.ThrowIf(_bufferWriter == null, this);

            _bufferWriter.GetSpan(1)[0] = value;
            _bufferWriter.Advance(1);

        }
        protected   override        void                            Dispose(bool disposing)
        {
            _bufferWriter = null;
            base.Dispose(disposing);
        }
    }
}
