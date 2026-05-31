namespace JSqlParser.Net.Expression.Operators.Relational;

public class GreaterThanEquals : ComparisonOperator
{
    public GreaterThanEquals() : base(">=") { }
    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
