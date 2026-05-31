using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Statement.Select;

/// <summary>
/// Represents a parenthesized INSERT (for CTE like WITH x AS (INSERT ... RETURNING ...)).
/// </summary>
public class ParenthesedInsert : ASTNodeAccessImpl, Statement
{
    public Insert.Insert Insert { get; set; } = null!;

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"({Insert})";
}
