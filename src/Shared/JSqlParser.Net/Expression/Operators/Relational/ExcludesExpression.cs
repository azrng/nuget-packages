using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression.Operators.Relational;

/// <summary>
/// Array EXCLUDES operator: left EXCLUDES right
/// </summary>
public class ExcludesExpression : ASTNodeAccessImpl, Expression
{
    public Expression LeftExpression { get; set; } = null!;
    public Expression RightExpression { get; set; } = null!;

    public ExcludesExpression() { }

    public ExcludesExpression(Expression leftExpression, Expression rightExpression)
    {
        LeftExpression = leftExpression;
        RightExpression = rightExpression;
    }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{LeftExpression} EXCLUDES {RightExpression}";
}
