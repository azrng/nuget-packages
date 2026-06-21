namespace Azrng.NMaxCompute.Arrow;

/// <summary>
/// 先吐出一段前缀字节，再透传底层流的只读流。用于在 Arrow 分帧流前前置 schema IPC 消息。
/// </summary>
internal sealed class PrefixStream : Stream
{
    private readonly byte[] _prefix;
    private readonly Stream _inner;
    private int _prefixOffset;
    private bool _prefixDone;

    public PrefixStream(byte[] prefix, Stream inner)
    {
        _prefix = prefix ?? Array.Empty<byte>();
        _inner = inner;
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (count <= 0) return 0;

        // 前缀未耗尽：优先服务前缀
        if (!_prefixDone)
        {
            var n = Math.Min(count, _prefix.Length - _prefixOffset);
            Buffer.BlockCopy(_prefix, _prefixOffset, buffer, offset, n);
            _prefixOffset += n;
            if (_prefixOffset >= _prefix.Length) _prefixDone = true;
            return n;
        }

        // 前缀耗尽：透传底层流
        return _inner.Read(buffer, offset, count);
    }

    protected override void Dispose(bool disposing)
    {
        // 不 dispose _inner（由 MaxComputeArrowReader 统一管理）
        base.Dispose(disposing);
    }
}
