using System.Buffers.Binary;
using Azrng.NMaxCompute.Tunnel.Wire;

namespace Azrng.NMaxCompute.Arrow;

/// <summary>
/// MaxCompute Tunnel Arrow 分帧解码流。
/// <para>对应 PyODPS <c>odps/tunnel/io/reader.py::ArrowStreamReader</c>。原始响应流按
/// <c>[4B 大端 chunk_size][chunk_size 字节数据][4B 大端 crc32c]</c> 分块；逐块校验 crc，
/// 末块（不足 chunk_size+4）校验累计 crccrc。本流把内部数据拼接成一条普通字节流，
/// 供 <c>Apache.Arrow</c> IPC 流式读取器消费。</para>
/// </summary>
internal sealed class MaxComputeArrowFramedStream : Stream
{
    private readonly Stream _raw;
    private readonly Checksum _crc = new();     // 单块 crc
    private readonly Checksum _crccrc = new();  // 累计 crc
    private byte[]? _chunk;
    private int _chunkOffset;
    private int? _chunkSize;
    private bool _done;
    private bool _lastValidated;

    public MaxComputeArrowFramedStream(Stream raw) => _raw = raw;

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

        var copied = 0;
        while (copied < count)
        {
            if (_chunk is null || _chunkOffset >= _chunk.Length)
            {
                if (_done) break;
                if (!FillNextChunk()) break;
            }

            var n = Math.Min(count - copied, _chunk!.Length - _chunkOffset);
            Buffer.BlockCopy(_chunk, _chunkOffset, buffer, offset + copied, n);
            _chunkOffset += n;
            copied += n;
        }
        return copied;
    }

    /// <summary>读取并校验下一块，装入 _chunk。返回 false 表示已无更多数据。</summary>
    private bool FillNextChunk()
    {
        if (_chunkSize is null)
        {
            var sizeBytes = ReadExactly(_raw, 4);
            if (sizeBytes.Length == 0) { _done = true; return false; }   // 流末尾
            if (sizeBytes.Length < 4) throw new InvalidDataException("Truncated arrow chunk size.");
            _chunkSize = (int)BinaryPrimitives.ReadUInt32BigEndian(sizeBytes);
        }

        var readSize = _chunkSize.Value + 4;
        var block = ReadExactly(_raw, readSize);
        if (block.Length == 0) { _done = true; return false; }
        if (block.Length < 4) throw new InvalidDataException("Truncated arrow chunk.");

        var dataLen = block.Length - 4;
        var data = block.AsSpan(0, dataLen);
        var trailing = BinaryPrimitives.ReadUInt32BigEndian(block.AsSpan(dataLen, 4));

        _crc.Update(data);
        _crccrc.Update(data);

        if (block.Length < readSize)
        {
            // 末块：校验累计 crccrc
            if (trailing != _crccrc.GetValue())
                throw new InvalidDataException($"Arrow stream CRC mismatch (final): expected={trailing}, actual={_crccrc.GetValue()}.");
            _crccrc.Reset();
            _lastValidated = true;
            _done = true;
        }
        else
        {
            // 普通块：校验单块 crc
            if (trailing != _crc.GetValue())
                throw new InvalidDataException($"Arrow chunk CRC mismatch: expected={trailing}, actual={_crc.GetValue()}.");
            _crc.Reset();
        }

        _chunk = dataLen > 0 ? data.ToArray() : Array.Empty<byte>();
        _chunkOffset = 0;
        return true;
    }

    private static byte[] ReadExactly(Stream s, int count)
    {
        var buf = new byte[count];
        var off = 0;
        while (off < count)
        {
            var n = s.Read(buf, off, count - off);
            if (n <= 0) break;
            off += n;
        }
        return off == count ? buf : off == 0 ? Array.Empty<byte>() : buf[..off];
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _raw.Dispose();
        base.Dispose(disposing);
    }
}
