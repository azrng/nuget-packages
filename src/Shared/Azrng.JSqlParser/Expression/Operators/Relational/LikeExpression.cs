using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

public class LikeExpression : BinaryExpression
{
    public bool Not { get; set; }

    public override T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string OperatorSymbol => Not ? "NOT LIKE" : "LIKE";
}
