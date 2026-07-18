namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// PostgreSQL 数组/范围被包含运算符 &lt;&amp;（左侧被右侧包含）。
/// 对齐上游 <c>ContainedBy extends ComparisonOperator</c>，符号为 <c>&lt;&amp;</c>。
/// 注意：JSON 路径运算符 <c>&lt;@</c> 归 <see cref="JsonOperator"/>，不要混淆。
/// </summary>
public class ContainedBy : ComparisonOperator
{
    public ContainedBy() : base("<&") { }

    public ContainedBy(string op) : base(op) { }

    public override T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
