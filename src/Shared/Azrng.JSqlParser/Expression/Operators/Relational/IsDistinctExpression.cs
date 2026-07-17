using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

public class IsDistinctExpression : BinaryExpression
{
    public bool Not { get; set; }

    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string OperatorSymbol => Not ? "IS NOT DISTINCT FROM" : "IS DISTINCT FROM";
}
