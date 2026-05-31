using JSqlParser.Net.Expression;
using JSqlParser.Net.Expression.Operators.Arithmetic;

namespace JSqlParser.Net.Expression.Operators.Conditional;

public class XorExpression : BinaryExpression
{
    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string GetStringExpression() => "XOR";
}
