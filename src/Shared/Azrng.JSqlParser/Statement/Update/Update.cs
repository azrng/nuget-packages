using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Statement.Update;

/// <summary>
/// Represents an UPDATE statement in SQL.
/// </summary>
public class Update : ASTNodeAccessImpl, Statement
{
    public Table? Table { get; set; }
    public List<Join>? Joins { get; set; }
    public Azrng.JSqlParser.Expression.Expression? Where { get; set; }
    public System.Collections.Generic.List<UpdateSet> UpdateSets { get; set; } = new();

    /// <summary>MySQL LOW_PRIORITY 修饰符。</summary>
    public bool ModifierLowPriority { get; set; }

    /// <summary>MySQL IGNORE 修饰符。</summary>
    public bool ModifierIgnore { get; set; }

    /// <summary>RETURNING / RETURN 子句，未指定时为 null。</summary>
    public ReturningClause? Returning { get; set; }

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("UPDATE");
        if (ModifierLowPriority) sb.Append(" LOW_PRIORITY");
        if (ModifierIgnore) sb.Append(" IGNORE");
        sb.Append(' ').Append(Table);
        if (Joins != null)
        {
            foreach (var join in Joins) sb.Append(' ').Append(join);
        }
        sb.Append(" SET ");
        sb.Append(string.Join(", ", UpdateSets));
        if (Where != null) sb.Append(" WHERE ").Append(Where);
        if (Returning != null) sb.Append(Returning);
        return sb.ToString();
    }
}
