namespace JSqlParser.Net.Expression.Operators.Arithmetic;

public class BitwiseXor : BinaryExpression
{
    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string GetStringExpression() => "^";
}
