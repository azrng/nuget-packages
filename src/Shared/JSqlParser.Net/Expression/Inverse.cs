using System.Text;
using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression;

/// <summary>
/// Exasol Skyline INVERSE(expression) syntax.
/// </summary>
public class Inverse : ASTNodeAccessImpl, Expression
{
    public Expression Expression { get; set; } = null!;

    public Inverse() { }

    public Inverse(Expression expression)
    {
        Expression = expression;
    }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        return builder.Append("INVERSE(").Append(Expression).Append(')');
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
