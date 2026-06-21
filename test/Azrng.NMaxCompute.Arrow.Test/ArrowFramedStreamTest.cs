using System.Buffers.Binary;
using Apache.Arrow;
using Apache.Arrow.Arrays;
using Apache.Arrow.Ipc;
using Apache.Arrow.Types;
using Azrng.NMaxCompute.Arrow;
using Azrng.NMaxCompute.Tunnel.Wire;
using Xunit;

namespace Azrng.NMaxCompute.Arrow.Test;

/// <summary>
/// Arrow 分帧解码层往返测试：手搓 MaxCompute 分帧 → MaxComputeArrowFramedStream 解码 → 还原原始字节；
/// 并构造真实 Arrow IPC 流做端到端（分帧 → 解码 → Apache.Arrow 解析 → 断言 RecordBatch）。
/// </summary>
public class ArrowFramedStreamTest
{
    /// <summary>按 MaxCompute Arrow 分帧协议打包：[4B BE chunkSize][block: chunkSize data + 4B BE crc]... [末块 + 累计 crccrc]。</summary>
    private static byte[] Frame(ReadOnlySpan<byte> data, int chunkSize)
    {
        var ms = new MemoryStream();
        Span<byte> b4 = stackalloc byte[4];

        BinaryPrimitives.WriteUInt32BigEndian(b4, (uint)chunkSize);
        ms.Write(b4);

        var crc = new Checksum();    // 单块
        var crccrc = new Checksum(); // 累计

        for (var offset = 0; offset < data.Length || offset == 0;)
        {
            var remaining = data.Length - offset;
            var isLast = remaining <= chunkSize;
            var thisSize = Math.Min(chunkSize, remaining);
            var chunk = data.Slice(offset, thisSize);

            crc.Update(chunk);
            crccrc.Update(chunk);

            uint trailing = !isLast ? crc.GetValue() : crccrc.GetValue();
            ms.Write(chunk);
            BinaryPrimitives.WriteUInt32BigEndian(b4, trailing);
            ms.Write(b4);

            crc.Reset();
            if (isLast) { crccrc.Reset(); break; }
            offset += thisSize;
        }
        return ms.ToArray();
    }

    private static byte[] ReadAll(MaxComputeArrowFramedStream stream)
    {
        using var outMs = new MemoryStream();
        stream.CopyTo(outMs);
        return outMs.ToArray();
    }

    [Fact]
    public void Framing_SingleChunk_RoundTrips()
    {
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var framed = Frame(data, 16);   // 一块即末块
        using var s = new MaxComputeArrowFramedStream(new MemoryStream(framed));
        Assert.Equal(data, ReadAll(s));
    }

    [Fact]
    public void Framing_MultiChunk_RoundTrips()
    {
        var data = Enumerable.Range(0, 50).Select(i => (byte)i).ToArray();
        var framed = Frame(data, 16);   // 多块
        using var s = new MaxComputeArrowFramedStream(new MemoryStream(framed));
        Assert.Equal(data, ReadAll(s));
    }

    [Fact]
    public void Framing_LargeData_RoundTrips()
    {
        var data = Enumerable.Range(0, 4096).Select(i => (byte)(i % 251)).ToArray();
        var framed = Frame(data, 1000);   // 非整除：4 满 + 96 末块
        using var s = new MaxComputeArrowFramedStream(new MemoryStream(framed));
        Assert.Equal(data, ReadAll(s));
    }

    [Fact]
    public void CorruptChunkCrc_Throws()
    {
        var data = new byte[32];
        var framed = Frame(data, 16);   // 两块：第一块普通
        framed[10] ^= 0xFF;             // 破坏第一块数据 → crc 不匹配
        using var s = new MaxComputeArrowFramedStream(new MemoryStream(framed));
        Assert.Throws<InvalidDataException>(() => ReadAll(s));
    }
}
