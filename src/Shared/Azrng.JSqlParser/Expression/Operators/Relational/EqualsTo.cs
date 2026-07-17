namespace Azrng.JSqlParser.Expression.Operators.Relational;

public class EqualsTo : ComparisonOperator
{
    public EqualsTo() : base("=") { }
    public override T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
