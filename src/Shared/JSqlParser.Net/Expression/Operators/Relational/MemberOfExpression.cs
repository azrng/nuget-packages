using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression.Operators.Relational;

public class MemberOfExpression : ASTNodeAccessImpl, Expression
{
    public Expression LeftExpression { get; set; } = null!;
    public Expression RightExpression { get; set; } = null!;

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{LeftExpression} MEMBER OF({RightExpression})";
}
