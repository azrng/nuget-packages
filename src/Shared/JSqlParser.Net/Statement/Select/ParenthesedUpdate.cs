using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Statement.Select;

/// <summary>
/// Represents a parenthesized UPDATE (for CTE like WITH x AS (UPDATE ... RETURNING ...)).
/// </summary>
public class ParenthesedUpdate : ASTNodeAccessImpl, Statement
{
    public Update.Update Update { get; set; } = null!;

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"({Update})";
}
