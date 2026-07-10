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

    /// <summary>ClickHouse GLOBAL JOIN（分布式查询全局聚合），对齐上游 isGlobal()。</summary>
    public bool Global { get; set; }

    /// <summary>ClickHouse ANY JOIN（保留首条匹配），对齐上游 isAny()。</summary>
    public bool Any { get; set; }

    /// <summary>ClickHouse ALL JOIN（保留全部匹配），对齐上游 isAll()。</summary>
    public bool All { get; set; }

    /// <summary>是否为 JPQL/HQL 的 JOIN FETCH（预加载关联）。</summary>
    public bool Fetch { get; set; }

    /// <summary>SQL Server Join 提示（LOOP/HASH/MERGE），强制连接策略。未指定时为 null。对齐上游 JoinHint。</summary>
    public string? JoinHint { get; set; }

    public FromItem RightItem { get; set; } = null!;

    /// <summary>ksqlDB 流式 JOIN 的 WITHIN 窗口，对齐上游 joinWindow。在 RightItem 之后、ON 之前输出。</summary>
    public KSQLJoinWindow? JoinWindow { get; set; }

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

            // LATERAL VIEW 用空格连接（Hive 语义），普通 Simple Join 用逗号（MySQL 多表）
            sb.Append(RightItem is LateralView ? " " : ", ").Append(RightItem);
            return sb.ToString();
        }

        // ClickHouse 修饰顺序：GLOBAL → NATURAL → ANY|ALL → 方向(LEFT/RIGHT/FULL/CROSS) → OUTER/INNER/SEMI
        if (Global) sb.Append("GLOBAL ");
        if (Natural) sb.Append("NATURAL ");
        if (Any) sb.Append("ANY ");
        else if (All) sb.Append("ALL ");

        if (Inner) sb.Append("INNER ");
        else if (Left) sb.Append("LEFT ");
        else if (Right) sb.Append("RIGHT ");
        else if (Full) sb.Append("FULL ");
        if (Outer) sb.Append("OUTER ");
        if (Cross) sb.Append("CROSS ");
        if (Semi) sb.Append("SEMI ");
        // SQL Server Join 提示（方向词后、JOIN 前）
        if (JoinHint != null) sb.Append(JoinHint).Append(' ');

        if (RightItem == null)
            throw new InvalidOperationException("Join requires a right item.");

        // STRAIGHT_JOIN 是独立关键字（非 INNER/LEFT 等 join 类型修饰）
        sb.Append(Straight ? "STRAIGHT_JOIN " : "JOIN ");
        if (Fetch) sb.Append("FETCH ");
        sb.Append(RightItem);

        // ksqlDB WITHIN 窗口（RightItem 之后、ON/USING 之前）
        if (JoinWindow != null) sb.Append(" WITHIN ").Append(JoinWindow);

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
