using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// 表函数（FROM 子句中的 func(...) AS alias），对齐上游 TableFunction。
/// 形式：<c>SELECT * FROM generate_series(1, 10) AS s</c>。
/// </summary>
public class TableFunction : ASTNodeAccessImpl, FromItem
{
    /// <summary>函数表达式（Function 节点）。</summary>
    public Function Function { get; set; } = null!;

    /// <summary>别名，可选。</summary>
    public Alias? Alias { get; set; }

    public Alias? GetAlias() => Alias;
    public void SetAlias(Alias alias) => Alias = alias;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Function);
        if (Alias != null) sb.Append(' ').Append(Alias);
        return sb.ToString();
    }
}
