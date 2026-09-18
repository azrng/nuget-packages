using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// ClickHouse ARRAY JOIN 项（#2482）：表达式 [AS alias]（数组展开为多行）。
/// </summary>
public class ArrayJoinItem
{
    public IExpression? Expression { get; set; }

    /// <summary>展开结果别名，未指定时为 null。</summary>
    public Alias? Alias { get; set; }

    public override string ToString()
        => Alias != null ? $"{Expression} {Alias}" : $"{Expression}";
}

/// <summary>
/// ClickHouse WITH FILL 间隙填充子句（#2469）：
/// WITH FILL [FROM e] [TO e] [STEP e] [STALENESS e]，挂在 ORDER BY 元素上。
/// </summary>
public class WithFillClause
{
    public IExpression? From { get; set; }
    public IExpression? To { get; set; }
    public IExpression? Step { get; set; }

    /// <summary>STALENESS 表达式（新版 ClickHouse），未指定时为 null。</summary>
    public IExpression? Staleness { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder("WITH FILL");
        if (From != null) sb.Append(" FROM ").Append(From);
        if (To != null) sb.Append(" TO ").Append(To);
        if (Step != null) sb.Append(" STEP ").Append(Step);
        if (Staleness != null) sb.Append(" STALENESS ").Append(Staleness);
        return sb.ToString();
    }
}

/// <summary>
/// ClickHouse INTERPOLATE 元素（#2469）：col [AS expr]（相邻行线性插值）。
/// </summary>
public class InterpolateElement
{
    public string ColumnName { get; set; } = "";

    /// <summary>插值表达式，未指定时为 null（默认线性插值）。</summary>
    public IExpression? Expression { get; set; }

    public override string ToString()
        => Expression != null ? $"{ColumnName} AS {Expression}" : ColumnName;
}
