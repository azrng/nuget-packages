using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression;

/// <summary>
/// Represents a parenthesized expression in SQL.
/// </summary>
public class Parenthesis : ASTNodeAccessImpl, Expression
{
    public Expression Expression { get; set; } = null!;

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"({Expression})";
}
