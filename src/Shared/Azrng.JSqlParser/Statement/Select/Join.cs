using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents a JOIN clause in a SQL statement.
/// </summary>
public class Join : ASTNodeAccessImpl
{
    public bool Outer { get; set; }
    public bool Right { get; set; }
    public bool Left { get; set; }
    public bool Natural { get; set; }
    public bool Full { get; set; }
    public bool Inner { get; set; }
    public bool Simple { get; set; }
    public bool Cross { get; set; }
    public bool Semi { get; set; }

    /// <summary>ClickHouse/MySQL STRAIGHT_JOIN（强制连接顺序）。</summary>
    public bool Straight { get; set; }

    /// <summary>是否为 JPQL/HQL 的 JOIN FETCH（预加载关联）。</summary>
    public bool Fetch { get; set; }

    public FromItem RightItem { get; set; } = null!;

    /// <summary>JOIN 的 ON 表达式列表，支持多个 ON（如 JOIN t ON a ON b）。对齐上游 onExpressions。</summary>
    public List<Expression.Expression> OnExpressions { get; set; } = new();

    /// <summary>单个 ON 表达式（兼容旧 API，取 OnExpressions 首项或 null）。</summary>
    public Expression.Expression? OnExpression
    {
        get => OnExpressions.Count > 0 ? OnExpressions[0] : null;
        set
        {
            OnExpressions.Clear();
            if (value != null) OnExpressions.Add(value);
        }
    }

    public List<Column> UsingColumns { get; set; } = new();

    public bool IsInnerJoin()
    {
        return Inner || !(Left || Right || Full || Outer || Cross || Natural);
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();

        if (Simple)
        {
            if (RightItem == null)
                throw new InvalidOperationException("Simple join requires a right item.");

            sb.Append(", ").Append(RightItem);
            return sb.ToString();
        }

        if (Inner) sb.Append("INNER ");
        else if (Left) sb.Append("LEFT ");
        else if (Right) sb.Append("RIGHT ");
        else if (Full) sb.Append("FULL ");
        if (Outer) sb.Append("OUTER ");
        if (Natural) sb.Append("NATURAL ");
        if (Cross) sb.Append("CROSS ");
        if (Semi) sb.Append("SEMI ");

        if (RightItem == null)
            throw new InvalidOperationException("Join requires a right item.");

        // STRAIGHT_JOIN 是独立关键字（非 INNER/LEFT 等 join 类型修饰）
        sb.Append(Straight ? "STRAIGHT_JOIN " : "JOIN ");
        if (Fetch) sb.Append("FETCH ");
        sb.Append(RightItem);

        // 支持多个 ON 表达式（JOIN t ON a ON b），对齐上游 onExpressions
        foreach (var onExpr in OnExpressions)
        {
            sb.Append(" ON ").Append(onExpr);
        }
        if (OnExpressions.Count == 0 && UsingColumns.Count > 0)
            sb.Append(" USING (").Append(string.Join(", ", UsingColumns)).Append(")");

        return sb.ToString();
    }
}
