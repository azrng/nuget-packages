using JSqlParser.Net.Expression;
using JSqlParser.Net.Expression.Operators.Arithmetic;

namespace JSqlParser.Net.Expression.Operators.Relational;

/// <summary>
/// PRIOR TO keyword binary operator.
/// </summary>
public class PriorTo : BinaryExpression
{
    public PriorTo() { }

    public PriorTo(Expression leftExpression, Expression rightExpression)
    {
        LeftExpression = leftExpression;
        RightExpression = rightExpression;
    }

    public override string GetStringExpression() => "PRIOR TO";

    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
