using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

public class SimilarToExpression : BinaryExpression
{
    public bool Not { get; set; }

    public override T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string GetStringExpression() => Not ? "NOT SIMILAR TO" : "SIMILAR TO";
}
