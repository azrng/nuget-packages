namespace Azrng.JSqlParser.Expression.Operators.Relational;

public class NotEqualsTo : ComparisonOperator
{
    public NotEqualsTo() : base("<>") { }
    public override T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
