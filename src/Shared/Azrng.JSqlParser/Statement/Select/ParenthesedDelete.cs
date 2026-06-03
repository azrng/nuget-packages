using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents a parenthesized DELETE (for CTE like WITH x AS (DELETE ... RETURNING ...)).
/// </summary>
public class ParenthesedDelete : ASTNodeAccessImpl, Statement
{
    public Delete.Delete Delete { get; set; } = null!;

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"({Delete})";
}
