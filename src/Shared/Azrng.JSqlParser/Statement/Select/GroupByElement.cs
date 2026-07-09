namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents a GROUP BY clause in a SQL statement.
/// 对齐上游 GroupByElement，支持 GROUPING SETS / ROLLUP / CUBE 扩展。
/// </summary>
public class GroupByElement
{
    public List<Expression.Expression> GroupByExpressions { get; set; } = new();
    public bool MySqlWithRollup { get; set; }

    /// <summary>GROUPING SETS 分组集合（每组为表达式列表的原始文本，保 round-trip）。对齐上游 groupingSets。</summary>
    public List<string>? GroupingSets { get; set; }

    /// <summary>ROLLUP(a, b, ...) 列表（原始文本），为 null 表示无 ROLLUP。对齐上游 rollup。</summary>
    public List<string>? RollupExpressions { get; set; }

    /// <summary>CUBE(a, b, ...) 列表（原始文本），为 null 表示无 CUBE。对齐上游 cube。</summary>
    public List<string>? CubeExpressions { get; set; }

    public GroupByElement() { }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("GROUP BY ");

        // 普通分组表达式
        if (GroupByExpressions is { Count: > 0 })
            sb.Append(string.Join(", ", GroupByExpressions));

        // GROUPING SETS ((a, b), (c))
        if (GroupingSets is { Count: > 0 })
        {
            if (sb.Length > 0 && sb[^1] != ' ') sb.Append(", ");
            sb.Append("GROUPING SETS (").Append(string.Join(", ", GroupingSets)).Append(')');
        }

        // ROLLUP(a, b) — 作为独立分组函数（无空格，对齐函数式 round-trip）
        if (RollupExpressions is { Count: > 0 })
        {
            if (sb.Length > 0 && sb[^1] != ' ') sb.Append(", ");
            sb.Append("ROLLUP(").Append(string.Join(", ", RollupExpressions)).Append(')');
        }

        // CUBE(a, b)
        if (CubeExpressions is { Count: > 0 })
        {
            if (sb.Length > 0 && sb[^1] != ' ') sb.Append(", ");
            sb.Append("CUBE(").Append(string.Join(", ", CubeExpressions)).Append(')');
        }

        if (MySqlWithRollup) sb.Append(" WITH ROLLUP");
        return sb.ToString();
    }
}
