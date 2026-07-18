namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// PostgreSQL 数组/范围包含运算符 &amp;&gt;（左侧包含右侧）。
/// 对齐上游 <c>Contains extends ComparisonOperator</c>，符号为 <c>&amp;&gt;</c>。
/// 注意：JSON 路径运算符 <c>@&gt;</c> 归 <see cref="JsonOperator"/>，不要混淆。
/// </summary>
public class Contains : ComparisonOperator
{
    public Contains() : base("&>") { }

    public Contains(string op) : base(op) { }

    public override T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
}
