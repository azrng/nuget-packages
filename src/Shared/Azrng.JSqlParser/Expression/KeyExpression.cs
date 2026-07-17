using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 方言相关的 KEY 前缀表达式，如 MySQL 的 <c>KEY chain.entity</c>。
/// 移植自上游 JSqlParser commit bfcb8b75 的 KeyExpression。
/// </summary>
public class KeyExpression : ASTNodeAccessImpl, Expression
{
    public Expression? Expression { get; set; }

    public KeyExpression() { }

    public KeyExpression(Expression expression)
    {
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"KEY {Expression}";
}
