using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Statement.Delete;

/// <summary>
/// Represents a DELETE statement in SQL.
/// </summary>
public class Delete : ASTNodeAccessImpl, Statement
{
    public Table? Table { get; set; }

    /// <summary>
    /// PostgreSQL/SQL Server DELETE ... USING 子句中的附加表列表，未指定时为 null。
    /// 例如 <code>DELETE FROM a USING b WHERE a.id = b.aid</code> 中包含 b。
    /// </summary>
    public List<IFromItem>? UsingItems { get; set; }

    public Azrng.JSqlParser.Expression.Expression? Where { get; set; }

    /// <summary>MySQL LOW_PRIORITY 修饰符。</summary>
    public bool ModifierLowPriority { get; set; }

    /// <summary>MySQL QUICK 修饰符。</summary>
    public bool ModifierQuick { get; set; }

    /// <summary>MySQL IGNORE 修饰符。</summary>
    public bool ModifierIgnore { get; set; }

    /// <summary>RETURNING / RETURN 子句，未指定时为 null。</summary>
    public ReturningClause? Returning { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("DELETE");
        if (ModifierLowPriority) sb.Append(" LOW_PRIORITY");
        if (ModifierQuick) sb.Append(" QUICK");
        if (ModifierIgnore) sb.Append(" IGNORE");
        sb.Append(" FROM ").Append(Table);
        if (UsingItems is { Count: > 0 })
        {
            sb.Append(" USING ").Append(string.Join(", ", UsingItems));
        }
        if (Where != null) sb.Append(" WHERE ").Append(Where);
        if (Returning != null) sb.Append(Returning);
        return sb.ToString();
    }
}
