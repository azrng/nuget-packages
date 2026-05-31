using JSqlParser.Net.Expression;
using JSqlParser.Net.Expression.Operators.Arithmetic;

namespace JSqlParser.Net.Expression.Operators.Relational;

public class LikeExpression : BinaryExpression
{
    public bool Not { get; set; }

    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string GetStringExpression() => Not ? "NOT LIKE" : "LIKE";
}
