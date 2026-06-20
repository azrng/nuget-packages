using System.Buffers.Binary;
using System.Text;
using Azrng.NMaxCompute.Tunnel;
using Azrng.NMaxCompute.Tunnel.Types;
using Azrng.NMaxCompute.Tunnel.Wire;
using Xunit;

namespace Azrng.NMaxCompute.Test;

/// <summary>
/// interval_day_time / interval_year_month / vector 的 wire 解码 + record CRC 单元测试。
/// 对照 PyODPS reader.py::_read_field / _read_vector 的 wire 与 CRC 语义。
/// </summary>
public class IntervalVectorDecoderTest
{
    private sealed class Enc
    {
        private readonly List<byte> _b = new();
        public Enc VarUInt(uint v) { while (v > 0x7F) { _b.Add((byte)(v | 0x80)); v >>= 7; } _b.Add((byte)v); return this; }
        public Enc SInt64(long v) { var zz = (ulong)((v << 1) ^ (v >> 63)); while (zz > 0x7F) { _b.Add((byte)(zz | 0x80)); zz >>= 7; } _b.Add((byte)zz); return this; }
        public Enc SInt32(int v) { uint zz = (uint)((v << 1) ^ (v >> 31)); while (zz > 0x7F) { _b.Add((byte)(zz | 0x80)); zz >>= 7; } _b.Add((byte)zz); return this; }
        public Enc Double(double v) { Span<byte> buf = stackalloc byte[8]; BinaryPrimitives.WriteDoubleLittleEndian(buf, v); _b.AddRange(buf.ToArray()); return this; }
        public Enc Tag(int field, int wire) { var tag = ((ulong)field << 3) | (uint)wire; while (tag > 0x7F) { _b.Add((byte)(tag | 0x80)); tag >>= 7; } _b.Add((byte)tag); return this; }
        public Enc EndRecordWithCrc(Checksum crc) => Tag(TunnelWireConstants.TunnelEndRecord, 0).VarUInt(crc.GetValue());
        public MemoryStream ToStream() => new(_b.ToArray());
    }

    [Fact]
    public void IntervalDayTime_DecodesToTimeSpan()
    {
        // seconds=2, nanos=500_000_000 → 2.5s
        var enc = new Enc();
        enc.Tag(1, 0).SInt64(2).SInt32(500_000_000);

        var crc = new Checksum();
        crc.UpdateInt(1);
        crc.UpdateLong(2);
        crc.UpdateInt(500_000_000);
        enc.EndRecordWithCrc(crc);

        var reader = new TunnelRecordReader(enc.ToStream(),
            new[] { TypeDecoderFactory.GetDecoder("interval_day_time") });

        var row = reader.Read();
        Assert.NotNull(row);
        Assert.Equal(TimeSpan.FromSeconds(2.5), row![0]);
    }

    [Fact]
    public void IntervalDayTime_Negative_Decodes()
    {
        // seconds=-3, nanos=0 → -3s
        var enc = new Enc();
        enc.Tag(1, 0).SInt64(-3).SInt32(0);

        var crc = new Checksum();
        crc.UpdateInt(1);
        crc.UpdateLong(-3);
        crc.UpdateInt(0);
        enc.EndRecordWithCrc(crc);

        var reader = new TunnelRecordReader(enc.ToStream(),
            new[] { TypeDecoderFactory.GetDecoder("interval_day_time") });

        var row = reader.Read();
        Assert.Equal(TimeSpan.FromSeconds(-3), row![0]);
    }

    [Fact]
    public void IntervalYearMonth_DecodesToMonths()
    {
        // months=18
        var enc = new Enc();
        enc.Tag(1, 0).SInt64(18);

        var crc = new Checksum();
        crc.UpdateInt(1);
        crc.UpdateLong(18);
        enc.EndRecordWithCrc(crc);

        var reader = new TunnelRecordReader(enc.ToStream(),
            new[] { TypeDecoderFactory.GetDecoder("interval_year_month") });

        var row = reader.Read();
        Assert.Equal(18L, row![0]);
    }

    [Fact]
    public void Vector_Double_Decodes()
    {
        // vector<double,2> = [1.5, 2.5]；dim 不计入 crc，仅元素计入
        var enc = new Enc();
        enc.Tag(1, 2);              // field 1（wire type 被忽略）
        enc.VarUInt(2);             // dim
        enc.Double(1.5);
        enc.Double(2.5);

        var crc = new Checksum();
        crc.UpdateInt(1);
        crc.UpdateDouble(1.5);
        crc.UpdateDouble(2.5);
        enc.EndRecordWithCrc(crc);

        var reader = new TunnelRecordReader(enc.ToStream(),
            new[] { TypeDecoderFactory.GetDecoder("vector<double,32>") });

        var row = reader.Read();
        Assert.NotNull(row);
        var vec = Assert.IsType<double[]>(row![0]);
        Assert.Equal(new[] { 1.5, 2.5 }, vec);
    }

    [Theory]
    [InlineData("vector<float,32>")]
    [InlineData("vector(double,64)")]
    [InlineData("vector<float,128>")]
    public void Vector_TypeString_Parses(string typeString)
    {
        var d = TypeDecoderFactory.GetDecoder(typeString);
        Assert.IsType<VectorDecoder>(d);
    }

    [Theory]
    [InlineData("interval_day_time", typeof(IntervalDayTimeDecoder))]
    [InlineData("interval_year_month", typeof(IntervalYearMonthDecoder))]
    public void Interval_Types_Registered(string typeString, Type expected)
    {
        Assert.IsType(expected, TypeDecoderFactory.GetDecoder(typeString));
    }
}
