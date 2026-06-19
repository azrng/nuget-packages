using Azrng.NMaxCompute.Tunnel.Wire;

namespace Azrng.NMaxCompute.Tunnel.Types;

/// <summary>
/// ARRAY&lt;T&gt;：先 uint32 长度，再逐元素 [null marker bool, 若非 null 则 T]。
/// <para>
/// 对应 PyODPS <c>_read_array</c>。<b>关键</b>：长度与 null marker <b>不</b>计入 CRC，
/// 只有元素的实际值（通过 <paramref name="elementDecoder"/> 内部的 crc 更新）计入。
/// </para>
/// </summary>
public sealed class ArrayDecoder : ITypeDecoder
{
    private readonly ITypeDecoder _elementDecoder;

    public ArrayDecoder(ITypeDecoder elementDecoder)
    {
        _elementDecoder = elementDecoder ?? throw new ArgumentNullException(nameof(elementDecoder));
    }

    public object Read(ProtobufWireReader reader, Checksum checksum)
    {
        var size = reader.ReadVarUInt32();
        var list = new List<object?>(checked((int)size));
        for (var i = 0u; i < size; i++)
        {
            // null marker：不计入 crc（对齐 PyODPS read_bool 后直接判断）
            if (reader.ReadBool())
                list.Add(null);
            else
                list.Add(_elementDecoder.Read(reader, checksum));
        }
        return list;
    }
}

/// <summary>
/// MAP&lt;K,V&gt;：先读 K 数组，再读 V 数组（均走 array 协议），最后 zip 成字典。
/// <para>对应 PyODPS <c>_read_field</c> 中 <c>types.Map</c> 分支。</para>
/// </summary>
public sealed class MapDecoder : ITypeDecoder
{
    private readonly ITypeDecoder _keyDecoder;
    private readonly ITypeDecoder _valueDecoder;

    public MapDecoder(ITypeDecoder keyDecoder, ITypeDecoder valueDecoder)
    {
        _keyDecoder = keyDecoder ?? throw new ArgumentNullException(nameof(keyDecoder));
        _valueDecoder = valueDecoder ?? throw new ArgumentNullException(nameof(valueDecoder));
    }

    public object Read(ProtobufWireReader reader, Checksum checksum)
    {
        var keys = ReadArray(reader, checksum, _keyDecoder);
        var values = ReadArray(reader, checksum, _valueDecoder);

        var dict = new Dictionary<object, object?>();
        var count = Math.Min(keys.Count, values.Count);
        for (var i = 0; i < count; i++)
            dict[keys[i]!] = values[i];
        return dict;
    }

    private static List<object?> ReadArray(ProtobufWireReader reader, Checksum checksum, ITypeDecoder decoder)
    {
        var size = reader.ReadVarUInt32();
        var list = new List<object?>(checked((int)size));
        for (var i = 0u; i < size; i++)
        {
            if (reader.ReadBool())
                list.Add(null);
            else
                list.Add(decoder.Read(reader, checksum));
        }
        return list;
    }
}

/// <summary>
/// STRUCT&lt;f1:T1,f2:T2,...&gt;：逐字段 [null marker bool, 若非 null 则 T]，按声明顺序。
/// <para>对应 PyODPS <c>_read_struct</c>。null marker 不计入 CRC。</para>
/// <para>返回 <c>object?[]</c>，下标对应字段声明顺序。</para>
/// </summary>
public sealed class StructDecoder : ITypeDecoder
{
    private readonly ITypeDecoder[] _fieldDecoders;
    private readonly string[] _fieldNames;

    public StructDecoder(string[] fieldNames, ITypeDecoder[] fieldDecoders)
    {
        if (fieldNames.Length != fieldDecoders.Length)
            throw new ArgumentException("fieldNames 和 fieldDecoders 长度不一致");
        _fieldNames = fieldNames;
        _fieldDecoders = fieldDecoders;
    }

    public object Read(ProtobufWireReader reader, Checksum checksum)
    {
        var result = new object?[_fieldDecoders.Length];
        for (var i = 0; i < _fieldDecoders.Length; i++)
        {
            // null marker：不计入 crc
            if (!reader.ReadBool())
                result[i] = _fieldDecoders[i].Read(reader, checksum);
            else
                result[i] = null;
        }
        return result;
    }

    /// <summary>
    /// 字段名集合（仅供调试 / 上层包装为字典时使用）。
    /// </summary>
    public IReadOnlyList<string> FieldNames => _fieldNames;
}
