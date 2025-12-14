using System;
using System.IO;

namespace VRC.SDKBase.Editor.VTP
{
    // A mock for testing VTP code that expects a network stream.
    public class MockNetworkStream : Stream
    {
        // Since with a network stream you can write to the stream and you won't read back what you wrote, we have to use two memory streams to implement that.
        public MemoryStream incomingStream;
        public MemoryStream outgoingStream;

        public MockNetworkStream(MemoryStream incomingStream, MemoryStream outgoingStream)
        {
            this.incomingStream = incomingStream;
            this.outgoingStream = outgoingStream;
        }
        
        public override bool CanRead => true;
        
        public override bool CanSeek => false;
        
        public override bool CanWrite => true;
        public override long Length => incomingStream.Length;
        public override long Position { get; set; }

        public override void Flush()
        {
            outgoingStream.Flush();
        }
        
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Position < 0 || Position > incomingStream.Length)
                throw new ArgumentOutOfRangeException(nameof(Position), "Position is out of bounds");
            incomingStream.Position = Position;
            int read = incomingStream.Read(buffer, offset, count);
            Position += read;
            return read;
        }
        
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException($"Seek operation is not supported for {nameof(MockNetworkStream)}");
        }
        
        public override void SetLength(long value)
        {
            incomingStream.SetLength(value);
        }
        
        public override void Write(byte[] buffer, int offset, int count)
        {
            outgoingStream.Write(buffer, offset, count);
        }
    }
}