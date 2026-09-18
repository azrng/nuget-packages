using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// ClickHouse C 风格三元条件表达式（#2436/#2466）：cond ? then : else。
/// 对齐上游 TernaryExpression；右结合，else 分支可嵌套三元。
/// </summary>
public class TernaryExpression : ASTNodeAccessImpl, IExpression
{
    public required IExpression Condition { get; set; }
    public required IExpression ThenExpression { get; set; }
    public required IExpression ElseExpression { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{Condition} ? {ThenExpression} : {ElseExpression}";
}
