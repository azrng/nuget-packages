namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents an ORDER BY element in a SQL statement.
/// </summary>
public class OrderByElement
{
    public enum NullOrdering
    {
        NULLS_FIRST,
        NULLS_LAST
    }

    public required Expression.IExpression Expression { get; set; }
    public bool Asc { get; set; } = true;
    public bool AscDescPresent { get; set; }
    public NullOrdering? NullOrder { get; set; }

    /// <summary>COLLATE 排序规则名（含引号），未指定时为 null。</summary>
    public string? CollateName { get; set; }

    /// <summary>
    /// MySQL ORDER BY ... WITH ROLLUP 标记。对齐上游 mysqlWithRollup。
    /// 为 true 时 ToString 末尾输出 <c> WITH ROLLUP</c>。
    /// </summary>
    public bool MysqlWithRollup { get; set; }

    public OrderByElement() { }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder(Expression.ToString());
        if (CollateName != null) sb.Append(" COLLATE ").Append(CollateName);
        if (!Asc) sb.Append(" DESC");
        else if (AscDescPresent) sb.Append(" ASC");
        if (NullOrder.HasValue)
            sb.Append(' ').Append(NullOrder == NullOrdering.NULLS_FIRST ? "NULLS FIRST" : "NULLS LAST");
        if (MysqlWithRollup) sb.Append(" WITH ROLLUP");
        return sb.ToString();
    }
}
