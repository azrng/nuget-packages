using Azrng.NMaxCompute.Tunnel.Types;
using Azrng.NMaxCompute.Tunnel.Wire;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class TypeDecoderTest
{
    private static ProtobufWireReader ReaderFor(params byte[] bytes)
        => new(new MemoryStream(bytes));

    [Fact]
    public void IntegerDecoder_ReadsZigZag_AndUpdatesCrc()
    {
        // value = 300 → zigzag = 600 → varint 0xD8 0x04
        var reader = ReaderFor(0xD8, 0x04);
        var crc = new Checksum();

        var value = IntegerDecoder.Instance.Read(reader, crc);

        Assert.Equal(300L, value);

        var raw = new Checksum();
        raw.UpdateLong(300L);
        Assert.Equal(raw.GetValue(), crc.GetValue());
    }

    [Fact]
    public void DoubleDecoder_ReadsLittleEndian_AndUpdatesCrc()
    {
        // 1.0 → 00 00 00 00 00 00 F0 3F
        var reader = ReaderFor(0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F);
        var crc = new Checksum();

        var value = DoubleDecoder.Instance.Read(reader, crc);

        Assert.Equal(1.0, value);

        var raw = new Checksum();
        raw.UpdateDouble(1.0);
        Assert.Equal(raw.GetValue(), crc.GetValue());
    }

    [Fact]
    public void FloatDecoder_ReadsLittleEndian_AndUpdatesCrc()
    {
        // -1.0f → 00 00 80 BF
        var reader = ReaderFor(0x00, 0x00, 0x80, 0xBF);
        var crc = new Checksum();

        var value = FloatDecoder.Instance.Read(reader, crc);

        Assert.Equal(-1.0f, value);

        var raw = new Checksum();
        raw.UpdateFloat(-1.0f);
        Assert.Equal(raw.GetValue(), crc.GetValue());
    }

    [Fact]
    public void BooleanDecoder_ReadsTrueFalse_AndUpdatesCrc()
    {
        var crc1 = new Checksum();
        var v1 = BooleanDecoder.Instance.Read(ReaderFor(0x01), crc1);
        Assert.True((bool)v1);

        var raw1 = new Checksum();
        raw1.UpdateBool(true);
        Assert.Equal(raw1.GetValue(), crc1.GetValue());

        var crc0 = new Checksum();
        var v0 = BooleanDecoder.Instance.Read(ReaderFor(0x00), crc0);
        Assert.False((bool)v0);

        var raw0 = new Checksum();
        raw0.UpdateBool(false);
        Assert.Equal(raw0.GetValue(), crc0.GetValue());
    }

    [Fact]
    public void StringDecoder_ReadsLengthPrefixed()
    {
        // length=5 + "hello"
        var reader = ReaderFor(0x05, (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o');
        var crc = new Checksum();

        var value = StringDecoder.Instance.Read(reader, crc);

        Assert.Equal("hello", value);

        var raw = new Checksum();
        raw.Update(new byte[] { (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o' });
        Assert.Equal(raw.GetValue(), crc.GetValue());
    }

    [Fact]
    public void DecimalDecoder_ParsesInvariant()
    {
        // "3.14"
        var bytes = System.Text.Encoding.UTF8.GetBytes("3.14");
        var buffer = new List<byte> { (byte)bytes.Length };
        buffer.AddRange(bytes);

        var reader = ReaderFor(buffer.ToArray());
        var crc = new Checksum();

        var value = DecimalDecoder.Instance.Read(reader, crc);

        Assert.Equal(3.14m, value);

        var raw = new Checksum();
        raw.Update(bytes);
        Assert.Equal(raw.GetValue(), crc.GetValue());
    }

    [Fact]
    public void DateTimeDecoder_ConvertsMillisecondsToLocal()
    {
        // 0 ms = epoch
        var reader = ReaderFor(0x00);
        var crc = new Checksum();

        var value = DateTimeDecoder.Instance.Read(reader, crc);

        var expected = DateTimeDecoder.EpochUtc.AddMilliseconds(0).ToLocalTime();
        Assert.Equal(expected, value);
    }

    [Fact]
    public void DateDecoder_ConvertsDaysSinceEpoch()
    {
        // 1 day = zigzag(1) = 2 → varint 0x02
        var reader = ReaderFor(0x02);
        var crc = new Checksum();

        var value = DateDecoder.Instance.Read(reader, crc);

        Assert.Equal(new DateOnly(1970, 1, 2), value);
    }
}
