using Azrng.NMaxCompute.Tunnel.Wire;
// 别名避开 ITypeEncoder.WireType 属性名与 WireType 常量类的同名冲突
using WireTag = Azrng.NMaxCompute.Tunnel.Wire.WireType;

namespace Azrng.NMaxCompute.Tunnel.Types;

/// <summary>
/// 类型编码器：把单个字段值写入 wire 流并更新 CRC。各 encoder 与同名 decoder 严格互逆。
/// </summary>
public interface ITypeEncoder
{
    /// <summary>该类型在字段 tag 中的 wire type（与 PyODPS varint/fixed32/fixed64/length-delimited 分类一致）。</summary>
    int WireType { get; }

    /// <summary>编码 value 并更新 checksum。</summary>
    void Write(ProtobufWireWriter writer, Checksum checksum, object? value);
}

// ---------- 标量 ----------

/// <summary>整数族（tinyint/smallint/int/bigint）：zigzag varint + crc.UpdateLong。</summary>
public sealed class IntegerEncoder : ITypeEncoder
{
    public static readonly IntegerEncoder Instance = new();
    public int WireType => WireTag.Varint;
    public void Write(ProtobufWireWriter writer, Checksum checksum, object? value)
    {
        var v = Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
        checksum.UpdateLong(v);
        writer.WriteSInt64(v);
    }
}

/// <summary>BOOLEAN：varint + crc.UpdateBool。</summary>
public sealed class BooleanEncoder : ITypeEncoder
{
    public static readonly BooleanEncoder Instance = new();
    public int WireType => WireTag.Varint;
    public void Write(ProtobufWireWriter writer, Checksum checksum, object? value)
    {
        var v = Convert.ToBoolean(value, System.Globalization.CultureInfo.InvariantCulture);
        checksum.UpdateBool(v);
        writer.WriteBool(v);
    }
}

/// <summary>FLOAT：fixed32 + crc.UpdateFloat。</summary>
public sealed class FloatEncoder : ITypeEncoder
{
    public static readonly FloatEncoder Instance = new();
    public int WireType => WireTag.Fixed32;
    public void Write(ProtobufWireWriter writer, Checksum checksum, object? value)
    {
        var v = Convert.ToSingle(value, System.Globalization.CultureInfo.InvariantCulture);
        checksum.UpdateFloat(v);
        writer.WriteFixed32(BitConverter.SingleToUInt32Bits(v));
    }
}

/// <summary>DOUBLE：fixed64 + crc.UpdateDouble。</summary>
public sealed class DoubleEncoder : ITypeEncoder
{
    public static readonly DoubleEncoder Instance = new();
    public int WireType => WireTag.Fixed64;
    public void Write(ProtobufWireWriter writer, Checksum checksum, object? value)
    {
        var v = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
        checksum.UpdateDouble(v);
        writer.WriteFixed64(BitConverter.DoubleToUInt64Bits(v));
    }
}

/// <summary>STRING/BINARY/VARCHAR/CHAR/JSON：UTF-8 length-delimited + crc.Update(bytes)。</summary>
public sealed class StringEncoder : ITypeEncoder
{
    public static readonly StringEncoder Instance = new();
    public int WireType => WireTag.LengthDelimited;
    public void Write(ProtobufWireWriter writer, Checksum checksum, object? value)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(value?.ToString() ?? string.Empty);
        checksum.Update(bytes);
        writer.WriteBytes(bytes);
    }
}

/// <summary>DECIMAL：以字符串传输 + crc.Update(bytes)。</summary>
public sealed class DecimalEncoder : ITypeEncoder
{
    public static readonly DecimalEncoder Instance = new();
    public int WireType => WireTag.LengthDelimited;
    public void Write(ProtobufWireWriter writer, Checksum checksum, object? value)
    {
        var text = Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture)
            .ToString(System.Globalization.CultureInfo.InvariantCulture);
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        checksum.Update(bytes);
        writer.WriteBytes(bytes);
    }
}

// ---------- 日期时间 ----------

/// <summary>DATETIME：距 epoch 的毫秒（sint64）+ crc.UpdateLong。与 DateTimeDecoder 互逆。</summary>
public sealed class DateTimeEncoder : ITypeEncoder
{
    public static readonly DateTimeEncoder Instance = new();
    public int WireType => WireTag.Varint;
    public void Write(ProtobufWireWriter writer, Checksum checksum, object? value)
    {
        var dt = (DateTime)value!;
        // new DateTimeOffset(dt) 按 Kind 自动选 offset（Utc→0，Local/Unspecified→本地）。
        // 原写法 new DateTimeOffset(dt, GetUtcOffset(dt)) 对 Kind=Utc 会抛 ArgumentException（Utc 要求 offset=0）。
        var ms = new DateTimeOffset(dt).ToUnixTimeMilliseconds();
        checksum.UpdateLong(ms);
        writer.WriteSInt64(ms);
    }
}

/// <summary>DATE：距 1970-01-01 的天数（sint64）+ crc.UpdateLong。</summary>
public sealed class DateEncoder : ITypeEncoder
{
    private static readonly DateOnly Epoch = new(1970, 1, 1);
    public static readonly DateEncoder Instance = new();
    public int WireType => WireTag.Varint;
    public void Write(ProtobufWireWriter writer, Checksum checksum, object? value)
    {
        var date = (DateOnly)value!;
        var days = date.DayNumber - Epoch.DayNumber;
        checksum.UpdateLong(days);
        writer.WriteSInt64(days);
    }
}

/// <summary>TIMESTAMP/TIMESTAMP_NTZ：Unix 秒（sint64）+ 纳秒（sint32）。</summary>
public sealed class TimestampEncoder : ITypeEncoder
{
    public static readonly TimestampEncoder Instance = new();
    public int WireType => WireTag.LengthDelimited;
    public void Write(ProtobufWireWriter writer, Checksum checksum, object? value)
    {
        var dto = (DateTimeOffset)value!;
        var seconds = dto.ToUnixTimeSeconds();
        var subSecondTicks = dto.UtcTicks % TimeSpan.TicksPerSecond;   // 0..9_999_999 ticks
        var nanos = (int)(subSecondTicks * 100);                        // ticks(100ns) → nanos
        checksum.UpdateLong(seconds);
        writer.WriteSInt64(seconds);
        checksum.UpdateInt(nanos);
        writer.WriteSInt32(nanos);
    }
}

/// <summary>INTERVAL_DAY_TIME：秒（sint64）+ 纳秒（sint32）。</summary>
public sealed class IntervalDayTimeEncoder : ITypeEncoder
{
    public static readonly IntervalDayTimeEncoder Instance = new();
    public int WireType => WireTag.LengthDelimited;
    public void Write(ProtobufWireWriter writer, Checksum checksum, object? value)
    {
        var ts = (TimeSpan)value!;
        var seconds = ts.Ticks / TimeSpan.TicksPerSecond;
        var nanos = (int)((ts.Ticks % TimeSpan.TicksPerSecond) * 100);
        checksum.UpdateLong(seconds);
        writer.WriteSInt64(seconds);
        checksum.UpdateInt(nanos);
        writer.WriteSInt32(nanos);
    }
}

/// <summary>INTERVAL_YEAR_MONTH：月份数（sint64）。</summary>
public sealed class IntervalYearMonthEncoder : ITypeEncoder
{
    public static readonly IntervalYearMonthEncoder Instance = new();
    public int WireType => WireTag.Varint;
    public void Write(ProtobufWireWriter writer, Checksum checksum, object? value)
    {
        var months = Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
        checksum.UpdateLong(months);
        writer.WriteSInt64(months);
    }
}

// ---------- 复合 ----------

/// <summary>ARRAY&lt;T&gt;：varint(长度) + 逐元素 [null marker bool（不计 CRC），若非 null 则 T]。</summary>
public sealed class ArrayEncoder : ITypeEncoder
{
    private readonly ITypeEncoder _elementEncoder;
    public ArrayEncoder(ITypeEncoder elementEncoder) => _elementEncoder = elementEncoder;
    public int WireType => WireTag.LengthDelimited;

    public void Write(ProtobufWireWriter writer, Checksum checksum, object? value)
    {
        var list = (System.Collections.IList)value!;
        writer.WriteVarUInt32((uint)list.Count);
        foreach (var elem in list)
        {
            if (elem is null)
            {
                writer.WriteBool(true);   // null marker，不计 crc
            }
            else
            {
                writer.WriteBool(false);
                _elementEncoder.Write(writer, checksum, elem);
            }
        }
    }
}

/// <summary>MAP&lt;K,V&gt;：先写 K 数组，再写 V 数组（各走 array 协议），最后 zip。</summary>
public sealed class MapEncoder : ITypeEncoder
{
    private readonly ITypeEncoder _keyEncoder;
    private readonly ITypeEncoder _valueEncoder;
    public MapEncoder(ITypeEncoder keyEncoder, ITypeEncoder valueEncoder)
    {
        _keyEncoder = keyEncoder;
        _valueEncoder = valueEncoder;
    }
    public int WireType => WireTag.LengthDelimited;

    public void Write(ProtobufWireWriter writer, Checksum checksum, object? value)
    {
        var dict = (System.Collections.IDictionary)value!;
        // 先写 keys 数组，再写 values 数组
        WriteArray(writer, checksum, _keyEncoder, dict.Keys);
        WriteArray(writer, checksum, _valueEncoder, dict.Values);
    }

    private static void WriteArray(ProtobufWireWriter writer, Checksum checksum, ITypeEncoder enc, System.Collections.IEnumerable items)
    {
        var list = items.Cast<object?>().ToList();
        writer.WriteVarUInt32((uint)list.Count);
        foreach (var item in list)
        {
            if (item is null) writer.WriteBool(true);
            else { writer.WriteBool(false); enc.Write(writer, checksum, item); }
        }
    }
}

/// <summary>STRUCT&lt;f1:T1,...&gt;：逐字段 [null marker bool，若非 null 则 T]，按声明顺序。</summary>
public sealed class StructEncoder : ITypeEncoder
{
    private readonly ITypeEncoder[] _fieldEncoders;
    public StructEncoder(ITypeEncoder[] fieldEncoders) => _fieldEncoders = fieldEncoders;
    public int WireType => WireTag.LengthDelimited;

    public void Write(ProtobufWireWriter writer, Checksum checksum, object? value)
    {
        var arr = (object?[])value!;
        var n = Math.Min(arr.Length, _fieldEncoders.Length);
        for (var i = 0; i < _fieldEncoders.Length; i++)
        {
            if (i >= n || arr[i] is null)
                writer.WriteBool(true);
            else
            {
                writer.WriteBool(false);
                _fieldEncoders[i].Write(writer, checksum, arr[i]);
            }
        }
    }
}

/// <summary>VECTOR：varint(维度) + dim 个元素（维度不计 CRC）。</summary>
public sealed class VectorEncoder : ITypeEncoder
{
    private readonly ITypeEncoder _elementEncoder;
    public VectorEncoder(ITypeEncoder elementEncoder) => _elementEncoder = elementEncoder;
    public int WireType => WireTag.LengthDelimited;

    public void Write(ProtobufWireWriter writer, Checksum checksum, object? value)
    {
        var arr = (System.Collections.IList)value!;   // double[] 或 IList
        writer.WriteVarUInt32((uint)arr.Count);
        foreach (var elem in arr)
            _elementEncoder.Write(writer, checksum, elem);
    }
}
