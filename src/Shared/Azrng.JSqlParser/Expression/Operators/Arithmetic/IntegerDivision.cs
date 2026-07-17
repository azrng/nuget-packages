namespace Azrng.JSqlParser.Expression.Operators.Arithmetic;

public class IntegerDivision : BinaryExpression
{
    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string OperatorSymbol => "DIV";
}
