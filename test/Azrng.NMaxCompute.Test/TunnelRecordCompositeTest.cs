using System.Text;
using Azrng.NMaxCompute.Rest;
using Azrng.NMaxCompute.Tunnel;
using Azrng.NMaxCompute.Tunnel.Types;
using Azrng.NMaxCompute.Tunnel.Wire;
using Xunit;

namespace Azrng.NMaxCompute.Test;

/// <summary>
/// 端到端：复合类型列走 TunnelRecordReader，验证 record 级 CRC 在「size/null marker 不计入」语义下能匹配。
/// 这是 S2 最关键的验证点——若 CRC 语义与 PyODPS 不一致，整条记录校验会失败。
/// </summary>
public class TunnelRecordCompositeTest
{
    private sealed class Enc
    {
        private readonly List<byte> _b = new();
        public Enc VarUInt(uint v) { while (v > 0x7F) { _b.Add((byte)(v | 0x80)); v >>= 7; } _b.Add((byte)v); return this; }
        public Enc SInt64(long v) { var zz = (ulong)((v << 1) ^ (v >> 63)); while (zz > 0x7F) { _b.Add((byte)(zz | 0x80)); zz >>= 7; } _b.Add((byte)zz); return this; }
        public Enc Byte(byte v) { _b.Add(v); return this; }
        public Enc String(string s) { var d = Encoding.UTF8.GetBytes(s); VarUInt((uint)d.Length); _b.AddRange(d); return this; }
        public Enc Tag(int field, int wire) { var tag = ((ulong)field << 3) | (uint)wire; while (tag > 0x7F) { _b.Add((byte)(tag | 0x80)); tag >>= 7; } _b.Add((byte)tag); return this; }
        public byte[] ToArray() => _b.ToArray();
        public MemoryStream ToStream() => new(_b.ToArray());
    }

    [Fact]
    public void Record_WithArrayColumn_CrcMatchesLeafValuesOnly()
    {
        // 一列 array<bigint>，值 = [1, 2]
        // record wire：tag(field=1, wire=2) + <array bytes> + EndRecord + crc
        var enc = new Enc();
        enc.Tag(1, 2);          // field 1, length-delimited（array 用 LD wire）
        // array bytes: size=2, [false,1],[false,2]
        enc.VarUInt(2);
        enc.Byte(0); enc.SInt64(1);
        enc.Byte(0); enc.SInt64(2);

        // record crc：只有 fieldIndex(1) 和叶子值 1,2 计入
        var crc = new Checksum();
        crc.UpdateInt(1);
        crc.UpdateLong(1);
        crc.UpdateLong(2);
        var expected = crc.GetValue();

        enc.Tag(TunnelWireConstants.TunnelEndRecord, 0).VarUInt(expected);

        var decoders = new ITypeDecoder[] { TypeDecoderFactory.GetDecoder("array<bigint>") };
        var reader = new TunnelRecordReader(enc.ToStream(), decoders);

        var row = reader.Read();

        Assert.NotNull(row);
        var arr = Assert.IsType<List<object?>>(row![0]);
        Assert.Equal(new object?[] { 1L, 2L }, arr);
    }

    [Fact]
    public void Record_WithArrayColumn_CorruptLeaf_ThrowsCrc()
    {
        // 故意让叶子值与写入的 crc 不符
        var enc = new Enc();
        enc.Tag(1, 2);
        enc.VarUInt(2);
        enc.Byte(0); enc.SInt64(1);
        enc.Byte(0); enc.SInt64(99);  // 实际值 99
        // 但 crc 按叶子 [1, 2] 写入 → 不匹配
        var crc = new Checksum();
        crc.UpdateInt(1);
        crc.UpdateLong(1);
        crc.UpdateLong(2);
        enc.Tag(TunnelWireConstants.TunnelEndRecord, 0).VarUInt(crc.GetValue());

        var decoders = new ITypeDecoder[] { TypeDecoderFactory.GetDecoder("array<bigint>") };
        var reader = new TunnelRecordReader(enc.ToStream(), decoders);

        Assert.ThrowsAny<Exception>(() => reader.Read());
    }

    [Fact]
    public void Record_MixedBasicAndComposite()
    {
        // 两列：bigint id, array<string> tags
        var enc = new Enc();
        // field 1: bigint
        enc.Tag(1, 0).SInt64(42);
        // field 2: array<string> = ["a","bb"]
        enc.Tag(2, 2);
        enc.VarUInt(2);
        enc.Byte(0); enc.String("a");
        enc.Byte(0); enc.String("bb");

        var crc = new Checksum();
        crc.UpdateInt(1); crc.UpdateLong(42);
        crc.UpdateInt(2);
        crc.Update(Encoding.UTF8.GetBytes("a"));
        crc.Update(Encoding.UTF8.GetBytes("bb"));
        enc.Tag(TunnelWireConstants.TunnelEndRecord, 0).VarUInt(crc.GetValue());

        var decoders = new ITypeDecoder[]
        {
            TypeDecoderFactory.GetDecoder("bigint"),
            TypeDecoderFactory.GetDecoder("array<string>")
        };
        var reader = new TunnelRecordReader(enc.ToStream(), decoders);

        var row = reader.Read();
        Assert.NotNull(row);
        Assert.Equal(42L, row![0]);
        var tags = Assert.IsType<List<object?>>(row[1]);
        Assert.Equal("a", tags[0]);
        Assert.Equal("bb", tags[1]);
    }

    /// <summary>
    /// 回归：count 必须按 zigzag sint 编码。模拟旧 bug（普通 varint count=1，字节 0x01），
    /// reader 按 zigzag 解出 -1，与本批 1 条记录不符 → 抛错。
    /// 防止 writer 误把 count 改回普通 varint（集群才会暴露，此单测兜底）。
    /// </summary>
    [Fact]
    public void BatchCount_PlainVarint_RejectedByValidation()
    {
        var enc = new Enc();
        enc.Tag(1, 0).SInt64(42);                       // 1 条 bigint 记录
        var crc = new Checksum();
        crc.UpdateInt(1); crc.UpdateLong(42);
        enc.Tag(TunnelWireConstants.TunnelEndRecord, 0).VarUInt(crc.GetValue());

        enc.Tag(TunnelWireConstants.TunnelMetaCount, 0).VarUInt(1);   // 故意用普通 varint（应为 SInt64）
        enc.Tag(TunnelWireConstants.TunnelMetaChecksum, 0).VarUInt(0); // crccrc（count 校验先抛，不会到达）

        var decoders = new ITypeDecoder[] { TypeDecoderFactory.GetDecoder("bigint") };
        var reader = new TunnelRecordReader(enc.ToStream(), decoders);

        Assert.NotNull(reader.Read());                  // 第 1 条记录正常
        Assert.ThrowsAny<Exception>(() => reader.Read()); // 第 2 次 Read 命中 META_COUNT → count 校验抛错
    }
}