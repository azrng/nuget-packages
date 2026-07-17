namespace Azrng.JSqlParser.Expression.Operators.Relational;

public class GreaterThan : ComparisonOperator
{
    public GreaterThan() : base(">") { }
    public override T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
