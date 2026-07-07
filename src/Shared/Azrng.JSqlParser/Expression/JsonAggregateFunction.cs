using System.Text;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// JSON 聚合函数 JSON_OBJECTAGG / JSON_ARRAYAGG。
/// 与上游 JsonAggregateFunction 对齐（Azrng 继承 Function 复用 FILTER/OVER 能力）。
/// </summary>
public class JsonAggregateFunction : Function
{
    public enum AggregateType
    {
        /// <summary>JSON_OBJECTAGG(key VALUE/:/, value)</summary>
        OBJECT,
        /// <summary>JSON_ARRAYAGG(expr [ORDER BY ...])</summary>
        ARRAY
    }

    public AggregateType AggregateFunctionType { get; set; }

    // OBJECTAGG 字段
    public object? Key { get; set; }

    public bool UsingKeyKeyword { get; set; }

    public Expression? Value { get; set; }

    public bool UsingValueKeyword { get; set; }

    /// <summary>OBJECTAGG 分隔符：true=VALUE 关键字，false=冒号/逗号。</summary>
    public bool UsingValueSeparator { get; set; }

    public bool UsingFormatJson { get; set; }

    public JsonFunction.OnNullType? OnNull { get; set; }

    public JsonFunction.UniqueKeysType? UniqueKeys { get; set; }

    // ARRAYAGG 字段
    /// <summary>ARRAYAGG 的聚合表达式。</summary>
    public Expression? AggregateExpression { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        if (AggregateFunctionType == AggregateType.OBJECT)
        {
            AppendObjectAgg(sb);
        }
        else
        {
            AppendArrayAgg(sb);
        }
        return sb.ToString();
    }

    private void AppendObjectAgg(StringBuilder sb)
    {
        sb.Append("JSON_OBJECTAGG( ");
        if (UsingKeyKeyword) sb.Append("KEY ");
        sb.Append(Key);
        sb.Append(UsingValueSeparator ? " VALUE " : ":");
        sb.Append(Value);
        if (UsingFormatJson) sb.Append(" FORMAT JSON");
        if (OnNull != null)
        {
            sb.Append(OnNull == JsonFunction.OnNullType.ABSENT ? " ABSENT ON NULL" : " NULL ON NULL");
        }
        if (UniqueKeys != null)
        {
            sb.Append(UniqueKeys == JsonFunction.UniqueKeysType.WITH ? " WITH UNIQUE KEYS" : " WITHOUT UNIQUE KEYS");
        }
        sb.Append(" )");

        if (FilterExpression != null)
        {
            sb.Append(" FILTER (WHERE ").Append(FilterExpression).Append(')');
        }
    }

    private void AppendArrayAgg(StringBuilder sb)
    {
        sb.Append("JSON_ARRAYAGG( ");
        sb.Append(AggregateExpression);
        if (UsingFormatJson) sb.Append(" FORMAT JSON");
        if (OrderByElements is { Count: > 0 })
        {
            sb.Append(" ORDER BY ").Append(string.Join(", ", OrderByElements));
        }
        if (OnNull != null)
        {
            sb.Append(OnNull == JsonFunction.OnNullType.ABSENT ? " ABSENT ON NULL" : " NULL ON NULL");
        }
        sb.Append(")");

        if (FilterExpression != null)
        {
            sb.Append(" FILTER (WHERE ").Append(FilterExpression).Append(')');
        }
    }
}
