using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 时间关键字表达式，如 <c>CURRENT_DATE</c>、<c>CURRENT_TIMESTAMP</c>、<c>LOCALTIME</c> 等。
/// 与上游 TimeKeyExpression 对齐。
/// </summary>
public class TimeKeyExpression : ASTNodeAccessImpl, Expression
{
    public string StringValue { get; set; } = "";

    public TimeKeyExpression() { }

    public TimeKeyExpression(string value)
    {
        StringValue = value;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => StringValue;
}
