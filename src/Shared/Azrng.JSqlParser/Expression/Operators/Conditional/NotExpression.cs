using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Conditional;

public class NotExpression : ASTNodeAccessImpl, IExpression
{
    public required IExpression Expression { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"NOT {Expression}";
}
