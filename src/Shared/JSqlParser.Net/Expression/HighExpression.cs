using System.Text;
using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression;

/// <summary>
/// Exasol Skyline HIGH expression syntax.
/// </summary>
public class HighExpression : ASTNodeAccessImpl, Expression
{
    public Expression Expression { get; set; } = null!;

    public HighExpression() { }

    public HighExpression(Expression expression)
    {
        Expression = expression;
    }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        return builder.Append("HIGH ").Append(Expression);
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
