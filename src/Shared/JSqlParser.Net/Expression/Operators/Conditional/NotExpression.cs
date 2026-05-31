using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression.Operators.Conditional;

public class NotExpression : ASTNodeAccessImpl, Expression
{
    public Expression Expression { get; set; } = null!;

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"NOT {Expression}";
}
