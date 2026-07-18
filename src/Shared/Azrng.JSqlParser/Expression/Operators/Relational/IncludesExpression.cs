using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// Array INCLUDES operator: left INCLUDES right
/// </summary>
public class IncludesExpression : ASTNodeAccessImpl, IExpression
{
    public IExpression LeftExpression { get; set; } = null!;
    public IExpression RightExpression { get; set; } = null!;

    public IncludesExpression() { }

    public IncludesExpression(IExpression leftExpression, IExpression rightExpression)
    {
        LeftExpression = leftExpression;
        RightExpression = rightExpression;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{LeftExpression} INCLUDES {RightExpression}";
}
