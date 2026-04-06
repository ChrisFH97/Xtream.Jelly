using System;
using System.IO;
using System.Threading;

namespace Xtream.Jelly.Service;

/// <summary>
/// Stream which writes to a self-overwriting internal buffer.
/// </summary>
/// <param name="bufferSize">Size in bytes of the internal buffer.</param>
public class WrappedBufferStream(int bufferSize) : Stream
{
    /// <summary>
    /// Gets the maximal size in bytes of read/write chunks.
    /// </summary>
    public int BufferSize { get => Buffer.Length; }

#pragma warning disable CA1819
    /// <summary>
    /// Gets the internal buffer.
    /// </summary>
    public byte[] Buffer { get; } = new byte[bufferSize];
#pragma warning restore CA1819

    /// <summary>
    /// Gets the number of bytes that have been written to this stream.
    /// </summary>
    public long TotalBytesWritten { get; private set; }

    /// <inheritdoc />
    public override long Position
    {
        get => TotalBytesWritten % BufferSize; set { }
    }

    /// <inheritdoc />
    public override bool CanRead => false;

    /// <inheritdoc />
    public override bool CanWrite => true;

    /// <inheritdoc />
    public override bool CanSeek => false;

    /// <inheritdoc />
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        long written = 0;
        while (written < count)
        {
            long writable = Math.Min(count - written, BufferSize - Position);
            Array.Copy(buffer, offset + written, Buffer, Position, writable);
            written += writable;
            TotalBytesWritten += writable;
        }
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Flush()
    {
    }
}
