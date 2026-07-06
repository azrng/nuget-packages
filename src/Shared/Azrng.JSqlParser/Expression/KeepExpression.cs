using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Oracle KEEP 表达式：<c>KEEP (DENSE_RANK FIRST|LAST ORDER BY ...)</c>，
/// 用于在聚合函数上选择首/末排名行。与上游 KeepExpression 对齐。
/// <para>
/// 示例：<c>MAX(salary) KEEP (DENSE_RANK FIRST ORDER BY hire_date) OVER (PARTITION BY dept)</c>
/// </para>
/// </summary>
public class KeepExpression : ASTNodeAccessImpl, Expression
{
    /// <summary>排名函数名（通常是 DENSE_RANK）。</summary>
    public string Name { get; set; } = "";

    /// <summary>true 表示 FIRST，false 表示 LAST。</summary>
    public bool First { get; set; }

    /// <summary>ORDER BY 元素列表。</summary>
    public List<OrderByElement>? OrderByElements { get; set; }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("KEEP (").Append(Name).Append(' ');
        sb.Append(First ? "FIRST" : "LAST").Append(' ');
        if (OrderByElements is { Count: > 0 })
        {
            sb.Append("ORDER BY ").Append(string.Join(", ", OrderByElements));
        }
        sb.Append(')');
        return sb.ToString();
    }
}
