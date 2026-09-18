using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// SQL:2016 MATCH_RECOGNIZE 行模式识别（#2634）。
/// PARTITION BY / ORDER BY / MEASURES / SKIP / DEFINE 结构化，PATTERN 变量表达式原文透传。
/// </summary>
public class MatchRecognize : ASTNodeAccessImpl, IFromItem
{
    /// <summary>MATCH_RECOGNIZE 修饰的来源 FROM 项（如普通表），渲染在 MATCH_RECOGNIZE 之前。</summary>
    public IFromItem? Input { get; set; }

    /// <summary>PARTITION BY 表达式列表，未指定时为 null。</summary>
    public List<IExpression>? PartitionKeys { get; set; }

    /// <summary>ORDER BY 元素，未指定时为 null。</summary>
    public List<OrderByElement>? OrderByElements { get; set; }

    /// <summary>MEASURES 输出项列表，未指定时为 null。</summary>
    public List<SelectItem>? Measures { get; set; }

    /// <summary>行输出模式：true=ONE ROW PER MATCH，false=ALL ROWS [PER MATCH]，null=未指定。</summary>
    public bool? RowsPerMatch { get; set; }

    /// <summary>PER MATCH 关键字标志（配合 RowsPerMatch）。</summary>
    public bool PerMatch { get; set; }

    /// <summary>AFTER MATCH 跳过子句，未指定时为 null。</summary>
    public AfterMatchSkip? Skip { get; set; }

    /// <summary>PATTERN 变量表达式原文（不含外层括号），透传，未指定时为 null。</summary>
    public string? PatternText { get; set; }

    /// <summary>SUBSET 定义列表，未指定时为 null。</summary>
    public List<SubsetDefinition>? Subsets { get; set; }

    /// <summary>DEFINE 定义列表，未指定时为 null。</summary>
    public List<MatchDefine>? Defines { get; set; }

    public Alias? Alias { get; set; }

    public override string ToString()
    {
        var parts = new List<string>();
        if (PartitionKeys is { Count: > 0 })
            parts.Add("PARTITION BY " + string.Join(", ", PartitionKeys));
        if (OrderByElements is { Count: > 0 })
            parts.Add("ORDER BY " + string.Join(", ", OrderByElements));
        if (Measures is { Count: > 0 })
            parts.Add("MEASURES " + string.Join(", ", Measures));
        if (RowsPerMatch != null)
        {
            var rows = RowsPerMatch == true ? "ONE ROW" : "ALL ROWS";
            parts.Add(PerMatch ? $"{rows} PER MATCH" : rows);
        }
        if (Skip != null) parts.Add("AFTER MATCH " + Skip);
        if (PatternText != null) parts.Add($"PATTERN ({PatternText})");
        if (Subsets is { Count: > 0 })
            parts.Add("SUBSET " + string.Join(", ", Subsets));
        if (Defines is { Count: > 0 })
            parts.Add("DEFINE " + string.Join(", ", Defines));

        var sb = new StringBuilder();
        if (Input != null) sb.Append(Input).Append(' ');
        sb.Append("MATCH_RECOGNIZE (");
        if (parts.Count > 0) sb.Append(string.Join(" ", parts));
        sb.Append(')');
        if (Alias != null) sb.Append(' ').Append(Alias);
        return sb.ToString();
    }
}

/// <summary>AFTER MATCH 跳过子句。</summary>
public class AfterMatchSkip
{
    public SkipMode Mode { get; set; }

    /// <summary>Skip 关键字标志（SQL 标准形式 AFTER MATCH SKIP TO LAST x），默认输出。</summary>
    public bool SkipKeyword { get; set; } = true;

    /// <summary>TO FIRST/TO LAST/FIRST/LAST 的参照变量，未指定时为 null。</summary>
    public string? Variable { get; set; }

    public override string ToString() => Mode switch
    {
        SkipMode.PastLastRow => SkipKeyword ? "SKIP PAST LAST ROW" : "PAST LAST ROW",
        SkipMode.NextRow => SkipKeyword ? "SKIP NEXT ROW" : "NEXT ROW",
        SkipMode.ToFirst => SkipKeyword ? $"SKIP TO FIRST {Variable}" : $"TO FIRST {Variable}",
        SkipMode.ToLast => SkipKeyword ? $"SKIP TO LAST {Variable}" : $"TO LAST {Variable}",
        SkipMode.First => SkipKeyword ? $"SKIP FIRST {Variable}" : $"FIRST {Variable}",
        _ => SkipKeyword ? $"SKIP LAST {Variable}" : $"LAST {Variable}"
    };
}

/// <summary>AFTER MATCH 跳过模式。</summary>
public enum SkipMode
{
    PastLastRow,
    NextRow,
    ToFirst,
    ToLast,
    First,
    Last
}

/// <summary>SUBSET 名 = (变量列表)。</summary>
public class SubsetDefinition
{
    public string Name { get; set; } = "";
    public List<string> Variables { get; set; } = new();

    public override string ToString()
        => $"{Name} = ({string.Join(", ", Variables)})";
}

/// <summary>DEFINE 变量 AS 条件。</summary>
public class MatchDefine
{
    public string Variable { get; set; } = "";
    public IExpression? Expression { get; set; }

    public override string ToString()
        => $"{Variable} AS {Expression}";
}
