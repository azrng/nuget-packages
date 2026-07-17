namespace Azrng.JSqlParser.Expression.Operators.Relational;

public class MinorThanEquals : ComparisonOperator
{
    public MinorThanEquals() : base("<=") { }
    public override T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
