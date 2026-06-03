using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a signed expression (e.g., -1, +1) in SQL.
/// </summary>
public class SignedExpression : ASTNodeAccessImpl, Expression
{
    public char Sign { get; set; }
    public Expression Expression { get; set; } = null!;

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{Sign}{Expression}";
}
