using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a SQL function call.
/// </summary>
public class Function : ASTNodeAccessImpl, Expression
{
    public string Name { get; set; } = "";
    public Expression? Parameters { get; set; }
    public bool AllColumns { get; set; }
    public List<OrderByElement>? WithinGroupOrderByElements { get; set; }
    public Expression? FilterExpression { get; set; }

    /// <summary>
    /// GROUP_CONCAT 内部的 DISTINCT 标志。MySQL 专用：<code>GROUP_CONCAT(DISTINCT col SEPARATOR ',')</code>
    /// 对应上游 commit ff28f826。
    /// </summary>
    public bool Distinct { get; set; }

    /// <summary>
    /// GROUP_CONCAT 内部的 ORDER BY 元素。MySQL 专用：<code>GROUP_CONCAT(col ORDER BY id DESC)</code>
    /// </summary>
    public List<OrderByElement>? OrderByElements { get; set; }

    /// <summary>
    /// GROUP_CONCAT 的 SEPARATOR 表达式。MySQL 专用：<code>GROUP_CONCAT(col SEPARATOR ', ')</code>
    /// 未指定时 MySQL 默认逗号分隔。
    /// </summary>
    public Expression? Separator { get; set; }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Name).Append('(');
        if (AllColumns)
        {
            sb.Append('*');
        }
        else
        {
            if (Distinct) sb.Append("DISTINCT ");
            sb.Append(Parameters);
            if (OrderByElements is { Count: > 0 })
            {
                sb.Append(" ORDER BY ").Append(string.Join(", ", OrderByElements));
            }
            if (Separator != null)
            {
                sb.Append(" SEPARATOR ").Append(Separator);
            }
        }
        sb.Append(')');

        if (WithinGroupOrderByElements != null && WithinGroupOrderByElements.Count > 0)
            sb.Append(" WITHIN GROUP (ORDER BY ").Append(string.Join(", ", WithinGroupOrderByElements)).Append(')');

        if (FilterExpression != null)
            sb.Append(" FILTER (WHERE ").Append(FilterExpression).Append(')');

        return sb.ToString();
    }
}
