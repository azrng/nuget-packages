using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Exasol Skyline LOW expression syntax.
/// </summary>
public class LowExpression : ASTNodeAccessImpl, Expression
{
    public Expression Expression { get; set; } = null!;

    public LowExpression() { }

    public LowExpression(Expression expression)
    {
        Expression = expression;
    }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        return builder.Append("LOW ").Append(Expression);
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
