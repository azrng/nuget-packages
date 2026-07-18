using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// 表函数（FROM 子句中的 func(...) [WITH ORDINALITY] [AS] alias[(col,...)]），对齐上游 TableFunction。
/// 形式：<c>SELECT * FROM generate_series(1, 10) AS s</c>；
/// PostgreSQL：<c>SELECT * FROM jsonb_array_elements(d) WITH ORDINALITY ARR(item, pos)</c>。
/// </summary>
public class TableFunction : ASTNodeAccessImpl, IFromItem
{
    /// <summary>函数表达式（Function 节点）。</summary>
    public required Function Function { get; set; }

    /// <summary>别名，可选。</summary>
    public Alias? Alias { get; set; }

    /// <summary>PostgreSQL <c>WITH ORDINALITY</c> 标记（表函数追加序号列）。</summary>
    public bool WithOrdinality { get; set; }

    /// <summary>PostgreSQL 表函数别名后的列别名列表（如 <c>ARR(item, pos)</c> 中的 item/pos）。</summary>
    public List<string>? ColumnAliases { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Function);
        if (WithOrdinality) sb.Append(" WITH ORDINALITY");
        if (Alias != null) sb.Append(' ').Append(Alias);
        if (ColumnAliases is { Count: > 0 })
            sb.Append(" (").Append(string.Join(", ", ColumnAliases)).Append(')');
        return sb.ToString();
    }
}
