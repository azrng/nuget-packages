using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// 表示 IN / NOT IN 表达式。可选 <c>GLOBAL</c> 修饰（ClickHouse 全局 IN）。
/// 对齐上游 <c>InExpression.global</c> 字段。
/// </summary>
public class InExpression : ASTNodeAccessImpl, IExpression
{
    public required IExpression LeftExpression { get; set; }
    public IExpression? RightExpression { get; set; }
    public bool Not { get; set; }

    /// <summary>
    /// ClickHouse GLOBAL IN / GLOBAL NOT IN 修饰。对齐上游 <c>global</c> 字段。
    /// 为 true 时 ToString 在 IN 前输出 <c>GLOBAL </c>。
    /// </summary>
    public bool Global { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        // 对齐上游 InExpression.java:95-109 输出顺序：GLOBAL → NOT → IN
        var prefix = Global ? "GLOBAL " : "";
        var op = Not ? "NOT IN" : "IN";
        return $"{LeftExpression} {prefix}{op} ({RightExpression})";
    }
}
