using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// FROM 子句 UNPIVOT，对齐上游 UnPivot。
/// 形式：<c>UNPIVOT [INCLUDE NULLS] (value_col FOR pivot_col IN (col1, col2)) [AS alias]</c>。
/// </summary>
public class UnPivot
{
    /// <summary>是否 INCLUDE NULLS。</summary>
    public bool IncludeNulls { get; set; }

    /// <summary>值列（UNPIVOT 后的值列名）。</summary>
    public List<Column> UnpivotClause { get; set; } = new();

    /// <summary>FOR 列。</summary>
    public List<Column> UnpivotForClause { get; set; } = new();

    /// <summary>IN 值列表。</summary>
    public List<Expression.IExpression> UnpivotInClause { get; set; } = new();

    /// <summary>别名，可选。</summary>
    public Alias? Alias { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder("UNPIVOT");
        if (IncludeNulls) sb.Append(" INCLUDE NULLS");
        sb.Append(" (");
        sb.Append(string.Join(", ", UnpivotClause));
        sb.Append(" FOR ");
        sb.Append(string.Join(", ", UnpivotForClause));
        sb.Append(" IN (");
        sb.Append(string.Join(", ", UnpivotInClause));
        sb.Append("))");
        if (Alias != null) sb.Append(' ').Append(Alias);
        return sb.ToString();
    }
}
