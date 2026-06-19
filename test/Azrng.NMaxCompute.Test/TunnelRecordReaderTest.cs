using System.Collections;
using System.Text;
using Azrng.NMaxCompute.Rest;
using Azrng.NMaxCompute.Tunnel;
using Azrng.NMaxCompute.Tunnel.Types;
using Azrng.NMaxCompute.Tunnel.Wire;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class TunnelRecordReaderTest
{
    /// <summary>
    /// Wire 编码助手：仅测试用，写出 varint、tag、字段。
    /// </summary>
    private sealed class WireEncoder
    {
        private readonly MemoryStream _stream = new();

        public WireEncoder WriteVarUInt32(uint value)
        {
            while (value > 0x7F)
            {
                _stream.WriteByte((byte)(value | 0x80));
                value >>= 7;
            }
            _stream.WriteByte((byte)value);
            return this;
        }

        public WireEncoder WriteVarUInt64(ulong value)
        {
            while (value > 0x7F)
            {
                _stream.WriteByte((byte)(value | 0x80));
                value >>= 7;
            }
            _stream.WriteByte((byte)value);
            return this;
        }

        public WireEncoder WriteTag(int fieldNumber, int wireType)
        {
            // tag = (field_number << 3) | wire_type
            // field_number 与 wireType 拼接后再 varint 编码
            // 注意 field_number 可能超过 28 位（魔数）
            var tag = ((ulong)fieldNumber << 3) | (uint)wireType;
            return WriteVarUInt64(tag);
        }

        public WireEncoder WriteSInt64(long value)
        {
            // zigzag encode
            var zz = (ulong)((value << 1) ^ (value >> 63));
            return WriteVarUInt64(zz);
        }

        public WireEncoder WriteFixed64(double value)
        {
            var raw = BitConverter.DoubleToUInt64Bits(value);
            for (var i = 0; i < 8; i++)
                _stream.WriteByte((byte)(raw >> (i * 8)));
            return this;
        }

        public WireEncoder WriteBool(bool value)
        {
            _stream.WriteByte(value ? (byte)1 : (byte)0);
            return this;
        }

        public WireEncoder WriteBytes(byte[] bytes)
        {
            WriteVarUInt32((uint)bytes.Length);
            _stream.Write(bytes, 0, bytes.Length);
            return this;
        }

        public WireEncoder WriteString(string value)
            => WriteBytes(Encoding.UTF8.GetBytes(value));

        public WireEncoder WriteRawByte(byte b)
        {
            _stream.WriteByte(b);
            return this;
        }

        /// <summary>
        /// 计算给定 wire bytes 序列的 CRC32C。
        /// </summary>
        public static uint ComputeCrc(Action<WireEncoder> write)
        {
            var enc = new WireEncoder();
            write(enc);
            var crc = new Crc32C();
            crc.Update(enc._stream.ToArray());
            return crc.GetValue();
        }

        public byte[] ToArray() => _stream.ToArray();

        public Stream ToStream() => new MemoryStream(_stream.ToArray());
    }

    /// <summary>
    /// 模拟一条 wire 流记录：field1=int, field2=string, field3=bool + EndRecord + CRC32C。
    /// </summary>
    private static byte[] BuildRecord(long intValue, string strValue, bool boolValue)
    {
        var enc = new WireEncoder();

        // field1 (int): write tag + zigzag value, 同时 CRC 累计 fieldIndex + value
        enc.WriteTag(1, 0).WriteSInt64(intValue);
        enc.WriteTag(2, 2).WriteString(strValue);
        enc.WriteTag(3, 0).WriteBool(boolValue);

        // 构造 TUNNEL_END_RECORD tag (fieldNumber=33553408, wireType=0)
        // 然后是 CRC32C uint32

        // 计算这条记录的 CRC32C：
        // update_int(1) + update_long(intValue) + update_int(2) + update(bytes) + update_int(3) + update_bool(boolValue)
        var crc = new Checksum();
        crc.UpdateInt(1);
        crc.UpdateLong(intValue);
        crc.UpdateInt(2);
        crc.Update(Encoding.UTF8.GetBytes(strValue));
        crc.UpdateInt(3);
        crc.UpdateBool(boolValue);
        var expected = crc.GetValue();

        enc.WriteTag(TunnelWireConstants.TunnelEndRecord, 0).WriteVarUInt32(expected);

        return enc.ToArray();
    }

    [Fact]
    public void Read_SingleRecord_MixedTypes()
    {
        var bytes = BuildRecord(42L, "hello", true);
        var decoders = new ITypeDecoder[]
        {
            IntegerDecoder.Instance,
            StringDecoder.Instance,
            BooleanDecoder.Instance
        };

        var reader = new TunnelRecordReader(new MemoryStream(bytes), decoders);
        var row = reader.Read();

        Assert.NotNull(row);
        Assert.Equal(42L, row![0]);
        Assert.Equal("hello", row[1]);
        Assert.True((bool)row[2]!);
    }

    [Fact]
    public void Read_MultipleRecords()
    {
        var stream = new MemoryStream();
        foreach (var (i, s, b) in new[] { (1L, "a", true), (2L, "bb", false), (3L, "ccc", true) })
        {
            var bytes = BuildRecord(i, s, b);
            stream.Write(bytes, 0, bytes.Length);
        }
        stream.Position = 0;

        var decoders = new ITypeDecoder[]
        {
            IntegerDecoder.Instance,
            StringDecoder.Instance,
            BooleanDecoder.Instance
        };

        var reader = new TunnelRecordReader(stream, decoders);

        var r1 = reader.Read();
        Assert.Equal(1L, r1![0]);
        Assert.Equal("a", r1[1]);

        var r2 = reader.Read();
        Assert.Equal(2L, r2![0]);
        Assert.Equal("bb", r2[1]);

        var r3 = reader.Read();
        Assert.Equal(3L, r3![0]);
        Assert.Equal("ccc", r3[1]);
    }

    [Fact]
    public void Read_CorruptedChecksum_Throws()
    {
        var bytes = BuildRecord(42L, "hello", true);
        // 把中间的 string 内容字节翻转：CRC 一定对不上
        // BuildRecord 输出顺序：tag(int) + varint(42) + tag(str) + len(5) + "hello" + tag(bool) + 1 + end + crc
        // 翻转 'e' (位置在 len 后第一字节)
        // 用一个粗略定位：找第一个 'h' 之后翻转 'e'
        for (var i = 0; i < bytes.Length - 4; i++)
        {
            if (bytes[i] == (byte)'h' && i + 1 < bytes.Length && bytes[i + 1] == (byte)'e')
            {
                bytes[i + 1] ^= 0x20; // 'e' -> 'E'
                break;
            }
        }

        var decoders = new ITypeDecoder[]
        {
            IntegerDecoder.Instance,
            StringDecoder.Instance,
            BooleanDecoder.Instance
        };

        var reader = new TunnelRecordReader(new MemoryStream(bytes), decoders);
        // CRC 校验失败应抛 OdpsException；如果翻转影响了 varint 长度，会先抛 InvalidDataException，也算检测到错误
        Assert.ThrowsAny<Exception>(() => reader.Read());
    }

    [Fact]
    public void Read_DoubleColumn_Works()
    {
        // 构造一条只有 double 列的记录
        var enc = new WireEncoder();
        enc.WriteTag(1, 1).WriteFixed64(3.14);

        var crc = new Checksum();
        crc.UpdateInt(1);
        crc.UpdateDouble(3.14);
        var expected = crc.GetValue();

        enc.WriteTag(TunnelWireConstants.TunnelEndRecord, 0).WriteVarUInt32(expected);

        var reader = new TunnelRecordReader(
            new MemoryStream(enc.ToArray()),
            new ITypeDecoder[] { DoubleDecoder.Instance });

        var row = reader.Read();
        Assert.NotNull(row);
        Assert.Equal(3.14, row![0]);
    }

    [Fact]
    public void Read_PartialFields_NullMissing()
    {
        // 只写 field2，field1 和 field3 留空
        var enc = new WireEncoder();
        enc.WriteTag(2, 2).WriteString("middle");

        var crc = new Checksum();
        crc.UpdateInt(2);
        crc.Update(Encoding.UTF8.GetBytes("middle"));
        var expected = crc.GetValue();

        enc.WriteTag(TunnelWireConstants.TunnelEndRecord, 0).WriteVarUInt32(expected);

        var decoders = new ITypeDecoder[]
        {
            IntegerDecoder.Instance,
            StringDecoder.Instance,
            BooleanDecoder.Instance
        };

        var reader = new TunnelRecordReader(new MemoryStream(enc.ToArray()), decoders);
        var row = reader.Read();

        Assert.NotNull(row);
        Assert.Null(row![0]);
        Assert.Equal("middle", row[1]);
        Assert.Null(row[2]);
    }
}
