namespace JSqlParser.Net.Expression.Operators.Relational;

public class GreaterThan : ComparisonOperator
{
    public GreaterThan() : base(">") { }
    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
