using System.Text;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents a CTE (Common Table Expression) item in a WITH clause.
/// </summary>
public class WithItem : Select
{
    public List<SelectItem>? WithItemList { get; set; }
    public bool Recursive { get; set; }
    public Alias? Alias { get; set; }
    public Select? Select { get; set; }

    /// <summary>
    /// CTE materialization hint (true = MATERIALIZED, false = NOT MATERIALIZED, null = unspecified).
    /// </summary>
    public bool? Materialized { get; set; }

    /// <summary>
    /// Parenthesized DML statement for CTE (INSERT/UPDATE/DELETE with RETURNING).
    /// When set, this takes precedence over Select.
    /// </summary>
    public ParenthesedInsert? ParenthesedInsert { get; set; }
    public ParenthesedUpdate? ParenthesedUpdate { get; set; }
    public ParenthesedDelete? ParenthesedDelete { get; set; }

    public override T Accept<T, S>(SelectVisitor<T> selectVisitor, S context)
    {
        return selectVisitor.Visit(this, context);
    }

    public override StringBuilder AppendSelectBodyTo(StringBuilder builder)
    {
        builder.Append(Recursive ? "RECURSIVE " : "");
        if (Alias != null) builder.Append(Alias.Name);
        if (WithItemList != null && WithItemList.Count > 0)
        {
            builder.Append(" (").Append(string.Join(", ", WithItemList)).Append(')');
        }
        builder.Append(" AS ");
        if (Materialized == true) builder.Append("MATERIALIZED ");
        else if (Materialized == false) builder.Append("NOT MATERIALIZED ");

        if (ParenthesedInsert != null) builder.Append(ParenthesedInsert);
        else if (ParenthesedUpdate != null) builder.Append(ParenthesedUpdate);
        else if (ParenthesedDelete != null) builder.Append(ParenthesedDelete);
        else if (Select != null) Select.AppendTo(builder);
        return builder;
    }
}
