namespace JSqlParser.Net.Expression.Operators.Relational;

public class EqualsTo : ComparisonOperator
{
    public EqualsTo() : base("=") { }
    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
