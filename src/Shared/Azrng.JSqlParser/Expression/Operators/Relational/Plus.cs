using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// Exasol Skyline PLUS keyword binary operator.
/// </summary>
public class Plus : BinaryExpression
{
    public Plus() { }

    public Plus(IExpression leftExpression, IExpression rightExpression)
    {
        LeftExpression = leftExpression;
        RightExpression = rightExpression;
    }

    public override string OperatorSymbol => "PLUS";

    public override T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
