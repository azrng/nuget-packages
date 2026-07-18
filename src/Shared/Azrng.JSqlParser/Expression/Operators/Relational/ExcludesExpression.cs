using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// Array EXCLUDES operator: left EXCLUDES right
/// </summary>
public class ExcludesExpression : ASTNodeAccessImpl, IExpression
{
    public IExpression LeftExpression { get; set; } = null!;
    public IExpression RightExpression { get; set; } = null!;

    public ExcludesExpression() { }

    public ExcludesExpression(IExpression leftExpression, IExpression rightExpression)
    {
        LeftExpression = leftExpression;
        RightExpression = rightExpression;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{LeftExpression} EXCLUDES {RightExpression}";
}
