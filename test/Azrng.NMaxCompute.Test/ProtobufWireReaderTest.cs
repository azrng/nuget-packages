using Azrng.NMaxCompute.Tunnel.Wire;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class ProtobufWireReaderTest
{
    private static Stream ToStream(params byte[] bytes) => new MemoryStream(bytes);

    [Fact]
    public void ReadVarUInt32_SingleByte()
    {
         var reader = new ProtobufWireReader(ToStream(0x01));
        Assert.Equal(1u, reader.ReadVarUInt32());
    }

    [Fact]
    public void ReadVarUInt32_MultiByte()
    {
        // 300 = 0b100101100 → varint: 0xAC 0x02
         var reader = new ProtobufWireReader(ToStream(0xAC, 0x02));
        Assert.Equal(300u, reader.ReadVarUInt32());
    }

    [Fact]
    public void ReadVarUInt64_LargeValue()
    {
        // ulong.MaxValue = 0xFFFFFFFFFFFFFFFF
        // varint = 11 bytes of 0xFF + 0x01
        var bytes = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01 };
         var reader = new ProtobufWireReader(ToStream(bytes));
        Assert.Equal(ulong.MaxValue, reader.ReadVarUInt64());
    }

    [Theory]
    [InlineData(new byte[] { 0x00 }, 0)]   // 0 zigzag → 0
    [InlineData(new byte[] { 0x01 }, -1)]  // 1 zigzag → -1
    [InlineData(new byte[] { 0x02 }, 1)]   // 2 zigzag → 1
    [InlineData(new byte[] { 0x03 }, -2)]  // 3 zigzag → -2
    public void ReadSInt32_ZigZag(byte[] wire, int expected)
    {
        var reader = new ProtobufWireReader(ToStream(wire));
        Assert.Equal(expected, reader.ReadSInt32());
    }

    [Fact]
    public void ReadFixed32_LittleEndian()
    {
        // 0x12345678 little-endian: 78 56 34 12
         var reader = new ProtobufWireReader(ToStream(0x78, 0x56, 0x34, 0x12));
        Assert.Equal(0x12345678u, reader.ReadFixed32());
    }

    [Fact]
    public void ReadFixed64_LittleEndian()
    {
        // 0x123456789ABCDEF0 little-endian: F0 DE BC 9A 78 56 34 12
         var reader = new ProtobufWireReader(ToStream(0xF0, 0xDE, 0xBC, 0x9A, 0x78, 0x56, 0x34, 0x12));
        Assert.Equal(0x123456789ABCDEF0u, reader.ReadFixed64());
    }

    [Fact]
    public void ReadDouble_One()
    {
        // 1.0 as IEEE 754 little-endian: 00 00 00 00 00 00 F0 3F
         var reader = new ProtobufWireReader(ToStream(0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F));
        Assert.Equal(1.0, reader.ReadDouble());
    }

    [Fact]
    public void ReadFloat_MinusOne()
    {
        // -1.0f as IEEE 754 little-endian: 00 00 80 BF
         var reader = new ProtobufWireReader(ToStream(0x00, 0x00, 0x80, 0xBF));
        Assert.Equal(-1.0f, reader.ReadFloat());
    }

    [Fact]
    public void ReadString_Hello()
    {
        // length=5 + "hello"
         var reader = new ProtobufWireReader(ToStream(0x05, (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o'));
        Assert.Equal("hello", reader.ReadString());
    }

    [Fact]
    public void ReadTag_SplitsFieldAndWireType()
    {
        // field_number=1, wire_type=0 (VARINT): tag = (1<<3) | 0 = 0x08
         var reader = new ProtobufWireReader(ToStream(0x08));
        var (field, wire) = reader.ReadTag();
        Assert.Equal(1, field);
        Assert.Equal(WireType.Varint, wire);
    }

    [Fact]
    public void ReadTag_LengthDelimited()
    {
        // field_number=2, wire_type=2 (LENGTH_DELIMITED): tag = (2<<3) | 2 = 0x12
         var reader = new ProtobufWireReader(ToStream(0x12));
        var (field, wire) = reader.ReadTag();
        Assert.Equal(2, field);
        Assert.Equal(WireType.LengthDelimited, wire);
    }

    [Fact]
    public void Position_AdvancesCorrectly()
    {
         var reader = new ProtobufWireReader(ToStream(0xAC, 0x02));
        Assert.Equal(0, reader.Position);

        reader.ReadVarUInt32();

        Assert.Equal(2, reader.Position);
    }

    [Fact]
    public void ReadBool_ZeroAndOne()
    {
         var reader1 = new ProtobufWireReader(ToStream(0x00));
        Assert.False(reader1.ReadBool());

         var reader2 = new ProtobufWireReader(ToStream(0x01));
        Assert.True(reader2.ReadBool());
    }

    [Fact]
    public void TruncatedVarint_Throws()
    {
        // Continuation bit set but no more bytes
         var reader = new ProtobufWireReader(ToStream(0x80));
        Assert.Throws<InvalidDataException>(() => reader.ReadVarUInt32());
    }
}
