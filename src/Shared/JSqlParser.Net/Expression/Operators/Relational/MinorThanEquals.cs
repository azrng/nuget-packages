namespace JSqlParser.Net.Expression.Operators.Relational;

public class MinorThanEquals : ComparisonOperator
{
    public MinorThanEquals() : base("<=") { }
    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
