using System;
using System.IO;
using System.Threading;

namespace Xtream.Jelly.Service;

/// <summary>
/// Stream which reads from a self-overwriting internal buffer.
/// </summary>
public class WrappedBufferReadStream : Stream
{
    private readonly WrappedBufferStream _sourceBuffer;
    private readonly long _initialReadHead;

    /// <summary>
    /// Initializes a new instance of the <see cref="WrappedBufferReadStream"/> class.
    /// </summary>
    /// <param name="sourceBuffer">The source buffer to read from.</param>
    public WrappedBufferReadStream(WrappedBufferStream sourceBuffer)
    {
        _sourceBuffer = sourceBuffer;
        _initialReadHead = Math.Max(0, sourceBuffer.TotalBytesWritten - (sourceBuffer.BufferSize / 2));
        ReadHead = _initialReadHead;
    }

    /// <summary>
    /// Gets the virtual position in the source buffer.
    /// </summary>
    public long ReadHead { get; private set; }

    /// <inheritdoc />
    public override long Position
    {
        get => ReadHead % _sourceBuffer.BufferSize; set { }
    }

    /// <inheritdoc />
    public override bool CanRead => true;

    /// <inheritdoc />
    public override bool CanWrite => false;

    /// <inheritdoc />
    public override bool CanSeek => false;

#pragma warning disable CA1065
    /// <inheritdoc />
    public override long Length { get => throw new NotImplementedException(); }
#pragma warning restore CA1065

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        long gap = _sourceBuffer.TotalBytesWritten - ReadHead;

        while (gap == 0)
        {
            Thread.Sleep(1);
            gap = _sourceBuffer.TotalBytesWritten - ReadHead;
        }

        if (gap > _sourceBuffer.BufferSize)
        {
            throw new IOException("Reader cannot keep up");
        }

        long canCopy = Math.Min(count, gap);
        long read = 0;

        while (read < canCopy)
        {
            long readable = Math.Min(canCopy - read, _sourceBuffer.BufferSize - Position);
            Array.Copy(_sourceBuffer.Buffer, Position, buffer, offset + read, readable);
            read += readable;
            ReadHead += readable;
        }

        return (int)read;
    }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

    /// <inheritdoc />
    public override void SetLength(long value) => throw new NotImplementedException();

    /// <inheritdoc />
    public override void Flush()
    {
    }
}
