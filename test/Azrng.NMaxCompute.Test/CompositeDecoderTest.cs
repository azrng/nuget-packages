using System.Text;
using Azrng.NMaxCompute.Tunnel.Types;
using Azrng.NMaxCompute.Tunnel.Wire;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class CompositeDecoderTest
{
    private static ProtobufWireReader Reader(params byte[] bytes)
        => new(new MemoryStream(bytes));

    private static void WriteVarUInt(List<byte> b, uint v)
    {
        while (v > 0x7F) { b.Add((byte)(v | 0x80)); v >>= 7; }
        b.Add((byte)v);
    }

    private static void WriteSInt64(List<byte> b, long v)
    {
        var zz = (ulong)((v << 1) ^ (v >> 63));
        while (zz > 0x7F) { b.Add((byte)(zz | 0x80)); zz >>= 7; }
        b.Add((byte)zz);
    }

    [Fact]
    public void ArrayDecoder_ReadsElements_NullMarkersNotInCrc()
    {
        // array<bigint>：size=3，元素 [10, null, 30]
        // null marker 和 size 不计入 crc，只有 10 和 30 计入
        var bytes = new List<byte>();
        WriteVarUInt(bytes, 3);       // size
        bytes.Add(0);                 // null marker = false
        WriteSInt64(bytes, 10);       // 10
        bytes.Add(1);                 // null marker = true (skip)
        bytes.Add(0);                 // null marker = false
        WriteSInt64(bytes, 30);       // 30

        var crc = new Checksum();
        var decoder = new ArrayDecoder(IntegerDecoder.Instance);
        var value = decoder.Read(Reader(bytes.ToArray()), crc);

        var list = Assert.IsType<List<object?>>(value);
        Assert.Equal(3, list.Count);
        Assert.Equal(10L, list[0]);
        Assert.Null(list[1]);
        Assert.Equal(30L, list[2]);

        // CRC 应只包含 10 和 30（两个 long）
        var expected = new Checksum();
        expected.UpdateLong(10L);
        expected.UpdateLong(30L);
        Assert.Equal(expected.GetValue(), crc.GetValue());
    }

    [Fact]
    public void MapDecoder_ZipsKeysAndValues()
    {
        // map<string,bigint>：keys=[a,b]，values=[1,2]
        var bytes = new List<byte>();

        // keys array: size=2
        WriteVarUInt(bytes, 2);
        bytes.Add(0);
        bytes.AddRange(EncodeString("a"));
        bytes.Add(0);
        bytes.AddRange(EncodeString("b"));

        // values array: size=2
        WriteVarUInt(bytes, 2);
        bytes.Add(0);
        WriteSInt64(bytes, 1);
        bytes.Add(0);
        WriteSInt64(bytes, 2);

        var crc = new Checksum();
        var decoder = new MapDecoder(StringDecoder.Instance, IntegerDecoder.Instance);
        var value = decoder.Read(Reader(bytes.ToArray()), crc);

        var dict = Assert.IsType<Dictionary<object, object?>>(value);
        Assert.Equal(1L, dict["a"]);
        Assert.Equal(2L, dict["b"]);
    }

    [Fact]
    public void StructDecoder_ReadsFieldsInOrder()
    {
        // struct<name:string,age:bigint>：两个字段，第一个非 null，第二个 null
        var bytes = new List<byte>();
        // field 1: null marker false + string "x"
        bytes.Add(0);
        bytes.AddRange(EncodeString("x"));
        // field 2: null marker true (skip)
        bytes.Add(1);

        var crc = new Checksum();
        var decoder = new StructDecoder(
            new[] { "name", "age" },
            new ITypeDecoder[] { StringDecoder.Instance, IntegerDecoder.Instance });
        var value = decoder.Read(Reader(bytes.ToArray()), crc);

        var arr = Assert.IsType<object?[]>(value);
        Assert.Equal("x", arr[0]);
        Assert.Null(arr[1]);
    }

    private static IEnumerable<byte> EncodeString(string s)
    {
        var b = new List<byte>();
        var data = Encoding.UTF8.GetBytes(s);
        WriteVarUInt(b, (uint)data.Length);
        b.AddRange(data);
        return b;
    }

    [Fact]
    public void TimestampDecoder_SwallowsNanoseconds()
    {
        // seconds = 0, nanoSecs = 0 → epoch
        var bytes = new List<byte>();
        WriteSInt64(bytes, 0);   // seconds
        bytes.Add(0);            // nano (zigzag 0)
        var crc = new Checksum();
        var value = TimestampDecoder.UtcInstance.Read(Reader(bytes.ToArray()), crc);

        var dto = Assert.IsType<DateTimeOffset>(value);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(0), dto);
    }
}
