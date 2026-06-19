using Azrng.NMaxCompute.Tunnel.Wire;

namespace Azrng.NMaxCompute.Tunnel.Types;

/// <summary>
/// BIGINT/INT/SMALLINT/TINYINT：zigzag varint + crc.UpdateLong。
/// <para>对应 PyODPS <c>_read_field</c> 中 <c>types.integer_types</c> 分支。</para>
/// </summary>
public sealed class IntegerDecoder : ITypeDecoder
{
    public static readonly IntegerDecoder Instance = new();

    public object Read(ProtobufWireReader reader, Checksum checksum)
    {
        var value = reader.ReadSInt64();
        checksum.UpdateLong(value);
        return value;
    }
}

/// <summary>
/// FLOAT：fixed32 IEEE 754 + crc.UpdateFloat。
/// </summary>
public sealed class FloatDecoder : ITypeDecoder
{
    public static readonly FloatDecoder Instance = new();

    public object Read(ProtobufWireReader reader, Checksum checksum)
    {
        var value = reader.ReadFloat();
        checksum.UpdateFloat(value);
        return value;
    }
}

/// <summary>
/// DOUBLE：fixed64 IEEE 754 + crc.UpdateDouble。
/// </summary>
public sealed class DoubleDecoder : ITypeDecoder
{
    public static readonly DoubleDecoder Instance = new();

    public object Read(ProtobufWireReader reader, Checksum checksum)
    {
        var value = reader.ReadDouble();
        checksum.UpdateDouble(value);
        return value;
    }
}

/// <summary>
/// BOOLEAN：单字节 varint + crc.UpdateBool。
/// </summary>
public sealed class BooleanDecoder : ITypeDecoder
{
    public static readonly BooleanDecoder Instance = new();

    public object Read(ProtobufWireReader reader, Checksum checksum)
    {
        var value = reader.ReadBool();
        checksum.UpdateBool(value);
        return value;
    }
}

/// <summary>
/// STRING/BINARY/VARCHAR/CHAR/JSON：length-delimited UTF-8 + crc.Update(bytes)。
/// </summary>
public sealed class StringDecoder : ITypeDecoder
{
    public static readonly StringDecoder Instance = new();

    public object Read(ProtobufWireReader reader, Checksum checksum)
    {
        var bytes = reader.ReadBytes();
        checksum.Update(bytes);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}

/// <summary>
/// DECIMAL：以字符串形式传输，原样返回 decimal。
/// </summary>
public sealed class DecimalDecoder : ITypeDecoder
{
    public static readonly DecimalDecoder Instance = new();

    public object Read(ProtobufWireReader reader, Checksum checksum)
    {
        var bytes = reader.ReadBytes();
        checksum.Update(bytes);
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        return decimal.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
    }
}
