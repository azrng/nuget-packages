using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents a LIMIT clause in a SQL statement.
/// 支持 ClickHouse LIMIT n BY expr / LIMIT offset, n BY expr。
/// </summary>
public class Limit : ASTNodeAccessImpl
{
    public Expression.IExpression? RowCount { get; set; }
    public Expression.IExpression? Offset { get; set; }

    /// <summary>
    /// ClickHouse LIMIT BY 表达式列表（如 LIMIT 10 BY user_id）。对齐上游 Limit.byExpressions。
    /// 未指定时为 null。
    /// </summary>
    public List<Expression.IExpression>? ByExpressions { get; set; }

    public Limit() { }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder(" LIMIT ");
        if (RowCount != null) sb.Append(RowCount);
        if (Offset != null) sb.Append(" OFFSET ").Append(Offset);
        // ClickHouse LIMIT n BY cols
        if (ByExpressions is { Count: > 0 })
        {
            sb.Append(" BY ").Append(string.Join(", ", ByExpressions));
        }
        return sb.ToString();
    }
}
