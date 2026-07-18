using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 表示 COLLATE 表达式：<c>expr COLLATE collation</c>。
/// 与上游 CollateExpression 对齐。
/// <para>
/// 示例：<c>name COLLATE 'en_US.utf8'</c>、<c>'abc' COLLATE "C"</c>。
/// </para>
/// </summary>
public class CollateExpression : ASTNodeAccessImpl, IExpression
{
    public IExpression? LeftExpression { get; set; }
    public string Collate { get; set; } = "";

    public CollateExpression() { }

    public CollateExpression(IExpression? leftExpression, string collate)
    {
        LeftExpression = leftExpression;
        Collate = collate;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{LeftExpression} COLLATE {Collate}";
}
