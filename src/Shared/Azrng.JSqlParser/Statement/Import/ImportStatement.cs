using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Import;

/// <summary>
/// Exasol IMPORT 语句（简化透传版），对齐上游 Import。
/// 语法：<c>IMPORT [INTO table (cols)] FROM &lt;source&gt;</c>
/// source 为透传文本（LOCAL CSV FILE '...' / JDBC DRIVER ... / EXA ... 等），保 round-trip。
/// </summary>
public class ImportStatement : ASTNodeAccessImpl, IStatement
{
    /// <summary>导入目标表（可选，IMPORT FROM ... 可无 INTO）。</summary>
    public Table? Table { get; set; }

    /// <summary>列列表（可选）。</summary>
    public System.Collections.Generic.List<Column>? Columns { get; set; }

    /// <summary>FROM 来源透传文本（如 LOCAL CSV FILE 'file.csv'），保 round-trip。</summary>
    public string? IFromItem { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("IMPORT");
        if (Table != null)
        {
            sb.Append(" INTO ").Append(Table);
            if (Columns is { Count: > 0 })
                sb.Append(" (").Append(string.Join(", ", Columns)).Append(')');
        }
        if (IFromItem != null) sb.Append(" FROM ").Append(IFromItem);
        return sb.ToString();
    }
}
