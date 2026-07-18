using System.Diagnostics.CodeAnalysis;
using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Exasol Skyline HIGH expression syntax.
/// </summary>
public class HighExpression : ASTNodeAccessImpl, IExpression
{
    public required IExpression Expression { get; set; }

    [SetsRequiredMembers]
    public HighExpression(IExpression expression)
    {
        Expression = expression;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        return builder.Append("HIGH ").Append(Expression);
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
