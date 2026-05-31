using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression.Operators.Relational;

/// <summary>
/// Array INCLUDES operator: left INCLUDES right
/// </summary>
public class IncludesExpression : ASTNodeAccessImpl, Expression
{
    public Expression LeftExpression { get; set; } = null!;
    public Expression RightExpression { get; set; } = null!;

    public IncludesExpression() { }

    public IncludesExpression(Expression leftExpression, Expression rightExpression)
    {
        LeftExpression = leftExpression;
        RightExpression = rightExpression;
    }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{LeftExpression} INCLUDES {RightExpression}";
}
