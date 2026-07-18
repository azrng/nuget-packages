using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// PostgreSQL <c>ROWS FROM (...)</c> FROM 项：把多个集合返回函数（SRF）组合成单个 FROM 项，
/// 各函数按行对齐输出。
/// 形式：<c>SELECT * FROM ROWS FROM (generate_series(1,3), generate_series(10,12)) AS t(a,b)</c>。
/// </summary>
public class RowsFrom : ASTNodeAccessImpl, IFromItem
{
    /// <summary>ROWS FROM 内的表函数列表。</summary>
    public required List<TableFunction> TableFunctions { get; set; }

    /// <summary>别名（AS t），可选。</summary>
    public Alias? Alias { get; set; }

    /// <summary>别名后的列别名列表（如 <c>t(a,b)</c> 中的 a/b），可选。</summary>
    public List<string>? ColumnAliases { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("ROWS FROM (");
        sb.Append(string.Join(", ", TableFunctions));
        sb.Append(')');
        if (Alias != null) sb.Append(' ').Append(Alias);
        if (ColumnAliases is { Count: > 0 })
            sb.Append(" (").Append(string.Join(", ", ColumnAliases)).Append(')');
        return sb.ToString();
    }
}
