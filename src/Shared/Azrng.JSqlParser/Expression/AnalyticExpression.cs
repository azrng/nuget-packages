using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents an analytic/window function expression (e.g., ROW_NUMBER() OVER (...)).
/// </summary>
public class AnalyticExpression : ASTNodeAccessImpl, Expression
{
    public string Name { get; set; } = "";
    public Expression? Expression { get; set; }
    public Expression? Offset { get; set; }
    public Expression? DefaultValue { get; set; }
    public bool AllColumns { get; set; }
    public bool Distinct { get; set; }
    public bool IgnoreNulls { get; set; }
    public List<OrderByElement>? OrderByElements { get; set; }
    public List<Expression>? PartitionExpressionList { get; set; }
    public string? WindowName { get; set; }
    public List<OrderByElement>? WithinGroupOrderByElements { get; set; }
    public Expression? FilterExpression { get; set; }

    /// <summary>
    /// OVER 子句中的窗口框架（ROWS/RANGE/GROUPS BETWEEN ...）。
    /// 未指定时为 null。对应上游 WindowElement。
    /// </summary>
    public WindowFrame? WindowFrame { get; set; }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder(Name);
        sb.Append('(');
        if (Distinct) sb.Append("DISTINCT ");
        if (AllColumns) sb.Append('*');
        else if (Expression != null) sb.Append(Expression);
        if (Offset != null) sb.Append(", ").Append(Offset);
        if (DefaultValue != null) sb.Append(", ").Append(DefaultValue);
        sb.Append(')');

        if (IgnoreNulls) sb.Append(" IGNORE NULLS");

        if (WithinGroupOrderByElements != null && WithinGroupOrderByElements.Count > 0)
            sb.Append(" WITHIN GROUP (ORDER BY ").Append(string.Join(", ", WithinGroupOrderByElements)).Append(')');

        if (FilterExpression != null)
            sb.Append(" FILTER (WHERE ").Append(FilterExpression).Append(')');

        sb.Append(" OVER (");
        if (WindowName != null) sb.Append(WindowName);
        else
        {
            if (PartitionExpressionList != null && PartitionExpressionList.Count > 0)
                sb.Append("PARTITION BY ").Append(string.Join(", ", PartitionExpressionList));
            if (OrderByElements != null && OrderByElements.Count > 0)
            {
                if (PartitionExpressionList != null && PartitionExpressionList.Count > 0) sb.Append(' ');
                sb.Append("ORDER BY ").Append(string.Join(", ", OrderByElements));
            }
        }
        // 输出窗口框架（ROWS/RANGE/GROUPS BETWEEN ...）
        if (WindowFrame != null)
        {
            sb.Append(' ').Append(WindowFrame);
        }
        sb.Append(')');

        return sb.ToString();
    }
}
