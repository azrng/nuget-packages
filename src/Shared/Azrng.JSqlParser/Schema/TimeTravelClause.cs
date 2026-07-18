namespace Azrng.JSqlParser.Schema;

/// <summary>
/// 时间旅行子句（Snowflake AT/BEFORE），对齐上游。
/// 形式：<c>AT (TIMESTAMP|OFFSET|STATEMENT) =&gt; expression</c> 或 <c>BEFORE (STATEMENT =&gt; expression)</c>。
/// </summary>
public class TimeTravelClause
{
    /// <summary>是否为 BEFORE（false 表示 AT）。</summary>
    public bool IsBefore { get; set; }

    /// <summary>时间旅行类型（TIMESTAMP / OFFSET / STATEMENT）。</summary>
    public string TravelType { get; set; } = "";

    /// <summary>时间表达式。</summary>
    public Expression.IExpression Expression { get; set; } = null!;

    public override string ToString()
    {
        // Snowflake 标准形式：AT (TIMESTAMP => expr) / BEFORE (STATEMENT => expr)
        var sb = new System.Text.StringBuilder();
        sb.Append(IsBefore ? "BEFORE (" : "AT (");
        if (!string.IsNullOrEmpty(TravelType)) sb.Append(TravelType).Append(" => ");
        sb.Append(Expression);
        sb.Append(')');
        return sb.ToString();
    }
}
