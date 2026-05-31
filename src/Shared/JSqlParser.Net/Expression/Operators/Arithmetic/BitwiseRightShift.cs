namespace JSqlParser.Net.Expression.Operators.Arithmetic;

public class BitwiseRightShift : BinaryExpression
{
    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string GetStringExpression() => ">>";
}
