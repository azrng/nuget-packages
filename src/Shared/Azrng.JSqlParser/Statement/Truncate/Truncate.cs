using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Truncate;

/// <summary>
/// Represents a TRUNCATE TABLE statement in SQL.
/// </summary>
public class Truncate : ASTNodeAccessImpl, Statement
{
    public Table? Table { get; set; }

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"TRUNCATE TABLE {Table}";
}
