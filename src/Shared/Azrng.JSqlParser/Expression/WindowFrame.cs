using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 窗口框架子句（ROWS/RANGE/GROUPS BETWEEN ... AND ...），定义窗口函数的计算行集范围。
/// 对应上游 WindowElement + WindowOffset + WindowRange。
/// <para>
/// 典型语法：
/// <code>
/// SUM(x) OVER (ORDER BY id ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)
/// SUM(x) OVER (ORDER BY id ROWS BETWEEN 1 PRECEDING AND 1 FOLLOWING)
/// SUM(x) OVER (PARTITION BY k ORDER BY id RANGE INTERVAL '1' DAY PRECEDING)
/// </code>
/// </para>
/// </summary>
public class WindowFrame : ASTNodeAccessImpl, IModel
{
    /// <summary>框架类型：ROWS / RANGE / GROUPS。</summary>
    public FrameType Type { get; set; }

    /// <summary>起始边界（BETWEEN 之前的单边界也用此字段）。</summary>
    public FrameBound Start { get; set; } = new();

    /// <summary>结束边界，未指定 BETWEEN 时为 null（表示默认到 CURRENT ROW）。</summary>
    public FrameBound? End { get; set; }

    /// <summary>EXCLUDE 子句，未指定时为 null。</summary>
    public ExcludeType? Exclude { get; set; }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(Type.ToString().ToUpperInvariant());
        if (End != null)
        {
            // BETWEEN ... AND ... 形式
            sb.Append(" BETWEEN ").Append(Start).Append(" AND ").Append(End);
        }
        else
        {
            // 单边界形式
            sb.Append(' ').Append(Start);
        }
        if (Exclude.HasValue)
        {
            sb.Append(" EXCLUDE ").Append(Exclude.Value.ToString().ToUpperInvariant().Replace('_', ' '));
        }
        return sb.ToString();
    }
}

/// <summary>窗口框架的单个边界（UNBOUNDED PRECEDING / CURRENT ROW / N PRECEDING / N FOLLOWING）。</summary>
public class FrameBound : ASTNodeAccessImpl, IModel
{
    /// <summary>边界类型。</summary>
    public BoundType Kind { get; set; }

    /// <summary>
    /// 当 Kind 为 Preceding 或 Following 且带数值偏移时，存放偏移表达式；
    /// UNBOUNDED 或 CURRENT ROW 时为 null。
    /// </summary>
    public IExpression? Offset { get; set; }

    public FrameBound() { }

    public FrameBound(BoundType kind) => Kind = kind;

    public override string ToString()
    {
        return Kind switch
        {
            BoundType.UnboundedPreceding => "UNBOUNDED PRECEDING",
            BoundType.UnboundedFollowing => "UNBOUNDED FOLLOWING",
            BoundType.CurrentRow => "CURRENT ROW",
            BoundType.Preceding => Offset != null ? $"{Offset} PRECEDING" : "PRECEDING",
            BoundType.Following => Offset != null ? $"{Offset} FOLLOWING" : "FOLLOWING",
            _ => Kind.ToString().ToUpperInvariant()
        };
    }
}

/// <summary>窗口框架类型。</summary>
public enum FrameType
{
    /// <summary>ROWS：按物理行计数。</summary>
    Rows,

    /// <summary>RANGE：按逻辑值范围（与 ORDER BY 值比较）。</summary>
    Range,

    /// <summary>GROUPS：按 peer group 计数。</summary>
    Groups
}

/// <summary>窗口框架边界类型。</summary>
public enum BoundType
{
    UnboundedPreceding,
    UnboundedFollowing,
    CurrentRow,
    /// <summary>带偏移的前导（N PRECEDING）。</summary>
    Preceding,
    /// <summary>带偏移的后继（N FOLLOWING）。</summary>
    Following
}

/// <summary>EXCLUDE 子句类型。</summary>
public enum ExcludeType
{
    CurrentRow,
    Group,
    Ties,
    NoOthers
}
