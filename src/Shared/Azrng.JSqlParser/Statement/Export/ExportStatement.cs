using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Export;

/// <summary>
/// Exasol EXPORT 语句（简化透传版），对齐上游 Export。
/// 语法：<c>EXPORT [table (cols) | (select)] INTO &lt;destination&gt;</c>
/// destination 为透传文本（LOCAL CSV FILE '...' / EXA AT ... / SCRIPT ... 等），保 round-trip。
/// </summary>
public class ExportStatement : ASTNodeAccessImpl, Statement
{
    /// <summary>导出目标表（与 Select 互斥）。</summary>
    public Table? Table { get; set; }

    /// <summary>列列表（可选）。</summary>
    public System.Collections.Generic.List<Column>? Columns { get; set; }

    /// <summary>导出的 SELECT 查询（与 Table 互斥）。</summary>
    public Select.Select? Select { get; set; }

    /// <summary>INTO 目标透传文本（如 LOCAL CSV FILE 'file.csv'），保 round-trip。</summary>
    public string? IntoItem { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("EXPORT ");
        if (Select != null)
        {
            sb.Append('(').Append(Select).Append(')');
        }
        else
        {
            sb.Append(Table);
            if (Columns is { Count: > 0 })
                sb.Append(" (").Append(string.Join(", ", Columns)).Append(')');
        }
        if (IntoItem != null) sb.Append(" INTO ").Append(IntoItem);
        return sb.ToString();
    }
}
