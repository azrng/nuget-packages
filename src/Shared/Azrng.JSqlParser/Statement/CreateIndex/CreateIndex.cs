using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.CreateIndex;

/// <summary>
/// Represents a CREATE INDEX statement in SQL.
/// </summary>
public class CreateIndex : ASTNodeAccessImpl, IStatement
{
    public Azrng.JSqlParser.Schema.Index? Index { get; set; }
    public Table? Table { get; set; }
    public bool Unique { get; set; }

    /// <summary>索引方法（PostgreSQL <c>USING btree|gist|gin|...</c>），未指定时为 null。</summary>
    public string? UsingMethod { get; set; }

    /// <summary>索引列定义原始文本列表（含 ASC/DESC/表达式/opclass），保 round-trip。</summary>
    public List<string> ColumnNames { get; } = new();

    /// <summary>部分索引的 WHERE 谓词（<c>WHERE ...</c>），未指定时为 null。</summary>
    public IExpression? Where { get; set; }

    /// <summary>
    /// SQL Server 索引 WITH 选项原始文本列表（#2020），如 <c>PAD_INDEX = OFF</c>、<c>FILLFACTOR = 80</c>。
    /// 未指定时为 null。
    /// </summary>
    public List<string>? WithOptions { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("CREATE ");
        if (Unique) sb.Append("UNIQUE ");
        sb.Append("INDEX ");
        if (Index != null) sb.Append(Index.Name);
        if (Table != null) sb.Append(" ON ").Append(Table);
        if (UsingMethod != null) sb.Append(" USING ").Append(UsingMethod);
        if (ColumnNames.Count > 0)
            sb.Append(" (").Append(string.Join(", ", ColumnNames)).Append(')');
        if (Where != null) sb.Append(" WHERE ").Append(Where);
        if (WithOptions is { Count: > 0 })
            sb.Append(" WITH (").Append(string.Join(", ", WithOptions)).Append(')');
        return sb.ToString();
    }
}
