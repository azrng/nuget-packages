using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Lambda expression: x -> x + 1 or (x, y) -> x + y
/// </summary>
public class LambdaExpression : ASTNodeAccessImpl, Expression
{
    public List<string> Identifiers { get; set; } = new();
    public Expression Expression { get; set; } = null!;

    public LambdaExpression() { }

    public LambdaExpression(string identifier, Expression expression)
    {
        Identifiers = new List<string> { identifier };
        Expression = expression;
    }

    public LambdaExpression(List<string> identifiers, Expression expression)
    {
        Identifiers = identifiers;
        Expression = expression;
    }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        if (Identifiers.Count == 1)
        {
            builder.Append(Identifiers[0]);
        }
        else
        {
            builder.Append("( ");
            for (int i = 0; i < Identifiers.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(Identifiers[i]);
            }
            builder.Append(" )");
        }
        return builder.Append(" -> ").Append(Expression);
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
