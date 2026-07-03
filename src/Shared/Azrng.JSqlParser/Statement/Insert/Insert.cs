using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Insert;

/// <summary>
/// Represents an INSERT statement in SQL.
/// </summary>
public class Insert : ASTNodeAccessImpl, Statement
{
    public Table? Table { get; set; }
    public System.Collections.Generic.List<Column>? Columns { get; set; }
    public Select.Select? Select { get; set; }
    public System.Collections.Generic.List<Update.UpdateSet>? DuplicateUpdateSets { get; set; }
    public System.Collections.Generic.List<Update.UpdateSet>? SetUpdateSets { get; set; }
    public bool UseValues { get; set; } = true;

    /// <summary>
    /// Partition references for INSERT INTO ... PARTITION (...) syntax.
    /// </summary>
    public System.Collections.Generic.List<Partition>? Partitions { get; set; }

    /// <summary>
    /// Whether this is an INSERT OVERWRITE statement.
    /// </summary>
    public bool Overwrite { get; set; }

    /// <summary>
    /// The TABLE keyword for INSERT INTO TABLE syntax.
    /// </summary>
    public bool TableKeyword { get; set; }

    /// <summary>
    /// OVERRIDING SYSTEM VALUE / USER VALUE syntax.
    /// </summary>
    public string? Overriding { get; set; }

    /// <summary>RETURNING / RETURN 子句，未指定时为 null。</summary>
    public ReturningClause? Returning { get; set; }

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("INSERT ");
        if (Overwrite) sb.Append("OVERWRITE ");
        else sb.Append("INTO ");
        if (TableKeyword) sb.Append("TABLE ");
        sb.Append(Table);
        if (Partitions != null && Partitions.Count > 0)
        {
            sb.Append(" PARTITION (");
            Partition.AppendPartitionsTo(sb, Partitions);
            sb.Append(')');
        }
        if (Overriding != null) sb.Append(" OVERRIDING ").Append(Overriding).Append(" VALUE");
        if (Columns != null && Columns.Count > 0)
        {
            sb.Append(" (").Append(string.Join(", ", Columns)).Append(')');
        }
        if (Select != null) sb.Append(" ").Append(Select);
        if (Returning != null) sb.Append(Returning);
        return sb.ToString();
    }
}
