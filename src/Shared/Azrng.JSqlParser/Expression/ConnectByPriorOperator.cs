using System.Diagnostics.CodeAnalysis;
using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Oracle PRIOR column expression for CONNECT BY.
/// </summary>
public class ConnectByPriorOperator : ASTNodeAccessImpl, IExpression
{
    public required IExpression Expression { get; set; }

    [SetsRequiredMembers]
    public ConnectByPriorOperator(IExpression expression)
    {
        Expression = expression;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        return builder.Append("PRIOR ").Append(Expression);
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
