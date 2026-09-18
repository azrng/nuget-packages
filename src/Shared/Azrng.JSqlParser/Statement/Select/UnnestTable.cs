using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// BigQuery UNNEST 表项（#2642）：UNNEST(array_expr) [WITH OFFSET [AS offset_alias]] [AS alias]。
/// </summary>
public class UnnestTable : ASTNodeAccessImpl, IFromItem
{
    /// <summary>被展开的数组表达式。</summary>
    public IExpression? Expression { get; set; }

    /// <summary>WITH OFFSET 的偏移列别名，未指定时为 null（WITH OFFSET 存在但无 AS 时为空串）。</summary>
    public string? OffsetAlias { get; set; }

    /// <summary>WITH OFFSET 标志。</summary>
    public bool WithOffset { get; set; }

    public Alias? Alias { get; set; }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("UNNEST(").Append(Expression).Append(')');
        if (Alias != null) sb.Append(' ').Append(Alias);
        if (WithOffset)
        {
            sb.Append(" WITH OFFSET");
            if (!string.IsNullOrEmpty(OffsetAlias)) sb.Append(" AS ").Append(OffsetAlias);
        }
        return sb.ToString();
    }
}
