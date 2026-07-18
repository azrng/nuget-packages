using System.Diagnostics.CodeAnalysis;
using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Exasol Skyline INVERSE(expression) syntax.
/// </summary>
public class Inverse : ASTNodeAccessImpl, IExpression
{
    public required IExpression Expression { get; set; }

    [SetsRequiredMembers]
    public Inverse(IExpression expression)
    {
        Expression = expression;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        return builder.Append("INVERSE(").Append(Expression).Append(')');
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
