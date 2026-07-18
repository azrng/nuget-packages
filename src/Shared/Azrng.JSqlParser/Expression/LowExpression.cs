using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Exasol Skyline LOW expression syntax.
/// </summary>
public class LowExpression : ASTNodeAccessImpl, IExpression
{
    public IExpression Expression { get; set; } = null!;

    public LowExpression() { }

    public LowExpression(IExpression expression)
    {
        Expression = expression;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        return builder.Append("LOW ").Append(Expression);
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
