namespace Azrng.JSqlParser.Expression.Operators.Relational;

public class MinorThan : ComparisonOperator
{
    public MinorThan() : base("<") { }
    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
