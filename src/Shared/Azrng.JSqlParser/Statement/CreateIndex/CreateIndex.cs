using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.CreateIndex;

/// <summary>
/// Represents a CREATE INDEX statement in SQL.
/// </summary>
public class CreateIndex : ASTNodeAccessImpl, Statement
{
    public Azrng.JSqlParser.Schema.Index? Index { get; set; }
    public Table? Table { get; set; }
    public bool Unique { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("CREATE ");
        if (Unique) sb.Append("UNIQUE ");
        sb.Append("INDEX ");
        if (Index != null) sb.Append(Index.Name);
        if (Table != null) sb.Append(" ON ").Append(Table);
        return sb.ToString();
    }
}
