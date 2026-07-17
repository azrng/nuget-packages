using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 范围表达式 <c>start : end</c>，主要用于 ClickHouse 数组初始化（如 <c>[1:10]</c>）。
/// 与上游 RangeExpression 对齐。
/// </summary>
public class RangeExpression : ASTNodeAccessImpl, Expression
{
    public Expression? StartExpression { get; set; }

    public Expression? EndExpression { get; set; }

    public RangeExpression() { }

    public RangeExpression(Expression? startExpression, Expression? endExpression)
    {
        StartExpression = startExpression;
        EndExpression = endExpression;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{StartExpression}:{EndExpression}";
}
