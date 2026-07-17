namespace Azrng.JSqlParser.Expression.Operators.Arithmetic;

public class Concat : BinaryExpression
{
    public override T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string OperatorSymbol => "||";
}
