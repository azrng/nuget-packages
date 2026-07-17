using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a parenthesized expression in SQL.
/// </summary>
public class Parenthesis : ASTNodeAccessImpl, Expression
{
    public Expression Expression { get; set; } = null!;

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"({Expression})";
}
