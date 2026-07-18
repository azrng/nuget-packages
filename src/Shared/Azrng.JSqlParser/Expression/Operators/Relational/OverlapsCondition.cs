using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// OVERLAPS 谓词，对齐上游 OverlapsCondition。
/// SQL 标准形式 <c>(start1, end1) OVERLAPS (start2, end2)</c>，当前 Azrng grammar
/// 两侧均按单元素 ExpressionList 包装（多元素列表形式需后续扩展 grammar）。
/// </summary>
public class OverlapsCondition : ASTNodeAccessImpl, IExpression
{
    public required ExpressionList LeftExpression { get; set; }
    public required ExpressionList RightExpression { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{LeftExpression} OVERLAPS {RightExpression}";
}
