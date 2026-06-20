namespace Azrng.NMaxCompute.Tunnel.Types;

/// <summary>
/// 类型字符串 → <see cref="ITypeDecoder"/> 注册中心。
/// <para>
/// 对应 PyODPS <c>validate_data_type</c> 与 <c>_read_field</c> 中的分支选择。
/// 基础类型走查表，复合类型（<c>array</c>/<c>map</c>/<c>struct</c>）经 <see cref="TypeStringParser"/> 递归解析。
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
        { "json", JsonDecoder.Instance },
        { "datetime", DateTimeDecoder.Instance },
        { "date", DateDecoder.Instance },
        { "timestamp", TimestampDecoder.LocalInstance },
        { "timestamp_ntz", TimestampDecoder.UtcInstance },
        { "decimal", DecimalDecoder.Instance },
        { "interval_day_time", IntervalDayTimeDecoder.Instance },
        { "interval_year_month", IntervalYearMonthDecoder.Instance }
    };

    /// <summary>
    /// 根据 MaxCompute 类型字符串获取 decoder。支持基础类型与复合类型。
    /// </summary>
    /// <exception cref="NotSupportedException">类型暂不支持</exception>
    public static ITypeDecoder GetDecoder(string typeString)
    {
        if (string.IsNullOrWhiteSpace(typeString))
            throw new ArgumentException("Type string is empty.", nameof(typeString));

        var trimmed = typeString.Trim();

        // decimal(p,s) 是带精度的简单类型，不是复合类型，优先短路
        if (trimmed.StartsWith("decimal(", StringComparison.OrdinalIgnoreCase))
            return DecimalDecoder.Instance;

        // vector(elem,dim) 用括号语法（无尖括号），需路由到 parser
        if (trimmed.StartsWith("vector", StringComparison.OrdinalIgnoreCase))
            return TypeStringParser.Parse(trimmed);

        // 含尖括号的视为复合类型，交给 parser
        if (trimmed.IndexOfAny(new[] { '<', '>' }) >= 0)
            return TypeStringParser.Parse(trimmed);

        return GetPrimitiveDecoder(trimmed);
    }

    /// <summary>
    /// 仅查表获取基础类型 decoder（不做复合解析）。
    /// </summary>
    public static ITypeDecoder GetPrimitiveDecoder(string typeString)
    {
        if (string.IsNullOrWhiteSpace(typeString))
            throw new ArgumentException("Type string is empty.", nameof(typeString));

        var key = typeString.Trim().ToLowerInvariant();
        if (Decoders.TryGetValue(key, out var decoder))
            return decoder;

        // 带长度/精度的基本类型（varchar(n) / char(n) / decimal(p,s) 等）：去掉 (..) 后按基名查表
        var parenIdx = key.IndexOf('(');
        if (parenIdx > 0)
        {
            var baseKey = key[..parenIdx];
            if (Decoders.TryGetValue(baseKey, out var baseDecoder))
                return baseDecoder;
        }

        throw new NotSupportedException($"MaxCompute type '{typeString}' is not supported yet.");
    }
}
