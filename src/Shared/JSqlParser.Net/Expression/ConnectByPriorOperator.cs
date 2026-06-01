using System.Text;
using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression;

/// <summary>
/// Oracle PRIOR column expression for CONNECT BY.
/// </summary>
public class ConnectByPriorOperator : ASTNodeAccessImpl, Expression
{
    public Expression Expression { get; set; } = null!;

    public ConnectByPriorOperator() { }

    public ConnectByPriorOperator(Expression expression)
    {
        Expression = expression;
    }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        return builder.Append("PRIOR ").Append(Expression);
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
