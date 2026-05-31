using JSqlParser.Net.Expression;
using JSqlParser.Net.Expression.Operators.Arithmetic;

namespace JSqlParser.Net.Expression.Operators.Relational;

/// <summary>
/// Exasol Skyline PLUS keyword binary operator.
/// </summary>
public class Plus : BinaryExpression
{
    public Plus() { }

    public Plus(Expression leftExpression, Expression rightExpression)
    {
        LeftExpression = leftExpression;
        RightExpression = rightExpression;
    }

    public override string GetStringExpression() => "PLUS";

    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
