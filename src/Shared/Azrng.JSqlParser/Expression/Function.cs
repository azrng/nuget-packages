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

    /// <summary>
    /// 通用关键字参数列表（如 <code>func(expr SEPARATOR ',')</code> 中的 SEPARATOR 部分，
    /// 或 BigQuery <code>ML.PREDICT(MODEL m, STRUCT(...))</code> 中的关键字参数）。
    /// 对应上游 commit cd71aada / Function.KeywordArgument。
    /// <para>
    /// 注意：<see cref="Separator"/> 字段是 MySQL GROUP_CONCAT 的快捷特例；
    /// 其他通用关键字参数通过此列表表达。
    /// </para>
    /// </summary>
    public List<KeywordArgument>? KeywordArguments { get; set; }

    /// <summary>
    /// Oracle KEEP 子句：<c>MAX(x) KEEP (DENSE_RANK FIRST ORDER BY ...)</c>。
    /// 未指定时为 null。对应上游 KeepExpression。
    /// </summary>
    public KeepExpression? Keep { get; set; }

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

        if (Keep != null) sb.Append(' ').Append(Keep);

        if (FilterExpression != null)
            sb.Append(" FILTER (WHERE ").Append(FilterExpression).Append(')');

        // 通用关键字参数输出（在 ) 之后，按上游 Function.toString 的顺序）
        if (KeywordArguments != null)
        {
            foreach (var ka in KeywordArguments)
            {
                sb.Append(ka);
            }
        }

        return sb.ToString();
    }
}

/// <summary>
/// 函数调用的通用关键字参数，如 <code>func(arg1 SEPARATOR ',')</code> 中的 <c>SEPARATOR ','</c>。
/// 对应上游 commit cd71aada / Function.KeywordArgument。
/// </summary>
public class KeywordArgument : ASTNodeAccessImpl, Model
{
    /// <summary>关键字名称（如 SEPARATOR、COST、USING）。</summary>
    public string Keyword { get; set; } = "";

    /// <summary>关键字后的表达式。</summary>
    public Expression? Expression { get; set; }

    public KeywordArgument() { }

    public KeywordArgument(string keyword, Expression? expression)
    {
        Keyword = keyword;
        Expression = expression;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(' ').Append(Keyword);
        if (Expression != null) sb.Append(' ').Append(Expression);
        return sb.ToString();
    }
}
