using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Schema;

/// <summary>
/// Represents a sequence reference in SQL.
/// </summary>
public class Sequence : ASTNodeAccessImpl
{
    public string? Database { get; set; }
    public string? SchemaName { get; set; }
    public string Name { get; set; } = "";

    /// <summary>序列参数列表（CREATE/ALTER SEQUENCE 时填充），未指定时为 null。</summary>
    public List<SequenceParameter>? Parameters { get; set; }

    public string GetFullyQualifiedName()
    {
        var parts = new System.Collections.Generic.List<string>();
        if (Database != null) parts.Add(Database);
        if (SchemaName != null) parts.Add(SchemaName);
        parts.Add(Name);
        return string.Join(".", parts);
    }

    public override string ToString() => GetFullyQualifiedName();
}

/// <summary>
/// 序列参数类型（CREATE/ALTER SEQUENCE）。
/// 移植自上游 JSqlParser 5.4 Sequence.ParameterType，含 PG 简写(INCREMENT/START)。
/// </summary>
public enum SequenceParameterType
{
    INCREMENT_BY,
    INCREMENT,
    START_WITH,
    START,
    RESTART_WITH,
    MAXVALUE,
    NOMAXVALUE,
    MINVALUE,
    NOMINVALUE,
    CYCLE,
    NOCYCLE,
    CACHE,
    NOCACHE,
    ORDER,
    NOORDER,
    KEEP,
    NOKEEP
}

/// <summary>
/// 序列参数（选项 + 可选值）。
/// </summary>
public class SequenceParameter
{
    public SequenceParameterType Option { get; set; }
    public long? Value { get; set; }

    public SequenceParameter() { }
    public SequenceParameter(SequenceParameterType option) { Option = option; }

    /// <summary>格式化为 SQL 文本。</summary>
    public string FormatParameter()
    {
        return Option switch
        {
            SequenceParameterType.INCREMENT_BY => $"INCREMENT BY {Value}",
            SequenceParameterType.INCREMENT => $"INCREMENT {Value}",
            SequenceParameterType.START_WITH => $"START WITH {Value}",
            SequenceParameterType.START => $"START {Value}",
            SequenceParameterType.RESTART_WITH => Value != null ? $"RESTART WITH {Value}" : "RESTART",
            SequenceParameterType.MAXVALUE => $"MAXVALUE {Value}",
            SequenceParameterType.MINVALUE => $"MINVALUE {Value}",
            SequenceParameterType.CACHE => $"CACHE {Value}",
            // 无值参数直接输出枚举名
            _ => Option.ToString()
        };
    }

    public SequenceParameter WithValue(long value)
    {
        Value = value;
        return this;
    }
}
