using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents a parenthesized UPDATE (for CTE like WITH x AS (UPDATE ... RETURNING ...)).
/// </summary>
public class ParenthesedUpdate : ASTNodeAccessImpl, IStatement
{
    public Update.Update Update { get; set; } = null!;

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"({Update})";
}
