namespace Azrng.NMaxCompute.Tunnel.Types;

/// <summary>
/// 类型字符串 → <see cref="ITypeDecoder"/> 注册中心。
/// <para>
/// 对应 PyODPS <c>validate_data_type</c> 与 <c>_read_field</c> 中的分支选择。
/// 当前仅注册 6 种基础类型（S1 范围）；复杂类型留待 S2 阶段。
/// </para>
/// </summary>
public static class TypeDecoderFactory
{
    private static readonly Dictionary<string, ITypeDecoder> Decoders = new(StringComparer.OrdinalIgnoreCase)
    {
        { "tinyint", IntegerDecoder.Instance },
        { "smallint", IntegerDecoder.Instance },
        { "int", IntegerDecoder.Instance },
        { "int_", IntegerDecoder.Instance },
        { "integer", IntegerDecoder.Instance },
        { "bigint", IntegerDecoder.Instance },
        { "long", IntegerDecoder.Instance },
        { "float", FloatDecoder.Instance },
        { "float_", FloatDecoder.Instance },
        { "double", DoubleDecoder.Instance },
        { "boolean", BooleanDecoder.Instance },
        { "bool", BooleanDecoder.Instance },
        { "string", StringDecoder.Instance },
        { "binary", StringDecoder.Instance },
        { "varchar", StringDecoder.Instance },
        { "char", StringDecoder.Instance },
        { "json", StringDecoder.Instance },
        { "datetime", DateTimeDecoder.Instance },
        { "date", DateDecoder.Instance },
        { "decimal", DecimalDecoder.Instance }
    };

    /// <summary>
    /// 根据 MaxCompute 类型字符串获取 decoder。
    /// </summary>
    /// <exception cref="NotSupportedException">类型暂不支持</exception>
    public static ITypeDecoder GetDecoder(string typeString)
    {
        if (string.IsNullOrWhiteSpace(typeString))
            throw new ArgumentException("Type string is empty.", nameof(typeString));

        // 简单类型直接查表；复合类型（带 &lt;&gt;）暂不支持，留待 S2
        var key = typeString.Trim().ToLowerInvariant();
        if (Decoders.TryGetValue(key, out var decoder))
            return decoder;

        // decimal(p,s) 去掉精度说明
        if (key.StartsWith("decimal(", StringComparison.Ordinal))
            return DecimalDecoder.Instance;

        throw new NotSupportedException($"MaxCompute type '{typeString}' is not supported yet.");
    }
}
