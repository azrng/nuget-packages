using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents a plain SELECT statement (without UNION/INTERSECT/EXCEPT).
/// </summary>
public class PlainSelect : Select
{
    public Distinct? Distinct { get; set; }
    public bool All { get; set; }

    /// <summary>SQL Server / Informix 风格的 SELECT TOP n 量词，未指定时为 null。</summary>
    public Top? Top { get; set; }

    /// <summary>Informix FIRST n 量词（SELECT FIRST n ...），未指定时为 null。与 Top 互斥使用。</summary>
    public Expression.Expression? First { get; set; }

    /// <summary>Informix SKIP n 量词（SELECT SKIP n FIRST m ...），未指定时为 null。</summary>
    public Expression.Expression? Skip { get; set; }

    /// <summary>DB2 OPTIMIZE FOR n ROWS 子句，未指定时为 null。</summary>
    public long? OptimizeFor { get; set; }
    public List<SelectItem>? SelectItems { get; set; }
    public FromItem? FromItem { get; set; }
    public List<Join>? Joins { get; set; }
    public Expression.Expression? Where { get; set; }
    public PreferringClause? Preferring { get; set; }
    public GroupByElement? GroupBy { get; set; }
    public Expression.Expression? Having { get; set; }

    /// <summary>MySQL INTO OUTFILE/DUMPFILE 子句（前置或尾部位置），未指定时为 null。</summary>
    public MySqlIntoOutfile? MySqlIntoOutfile { get; set; }

    /// <summary>SQL Server FOR XML PATH('name') 的路径名，无 FOR XML PATH 时为 null，空字符串表示无参数的 FOR XML PATH。</summary>
    public string? ForXmlPath { get; set; }

    /// <summary>Oracle 优化器提示（SELECT 关键字后紧跟的 /*+ ... */ 或 --+ ... 注释）。</summary>
    public OracleHint? OracleHint { get; set; }

    /// <summary>命名窗口定义（WINDOW w AS (...)），透传原始文本保 round-trip。对齐上游 windowDefinitions。</summary>
    public System.Collections.Generic.List<string>? WindowDefinitions { get; set; }

    /// <summary>QUALIFY 过滤表达式（Snowflake/Teradata），对齐上游 qualify。</summary>
    public Expression.Expression? Qualify { get; set; }

    public override T Accept<T, S>(SelectVisitor<T> selectVisitor, S context)
    {
        return selectVisitor.Visit(this, context);
    }

    public override StringBuilder AppendSelectBodyTo(StringBuilder builder)
    {
        builder.Append("SELECT ");
        if (OracleHint != null) builder.Append(OracleHint).Append(' ');
        if (Top != null) builder.Append(Top).Append(' ');
        if (Skip != null) builder.Append("SKIP ").Append(Skip).Append(' ');
        if (First != null) builder.Append("FIRST ").Append(First).Append(' ');
        if (Distinct != null) builder.Append(Distinct).Append(' ');
        else if (All) builder.Append("ALL ");
        if (SelectItems != null) builder.Append(string.Join(", ", SelectItems));

        // MySQL INTO OUTFILE/DUMPFILE 前置位置（FROM 之前）
        if (MySqlIntoOutfile is { BeforeFrom: true }) builder.Append(' ').Append(MySqlIntoOutfile);

        if (FromItem != null)
        {
            builder.Append(" FROM ").Append(FromItem);
        }

        if (Joins != null)
        {
            foreach (var join in Joins)
            {
                if (join.Simple) builder.Append(join);
                else builder.Append(' ').Append(join);
            }
        }

        if (Where != null) builder.Append(" WHERE ").Append(Where);
        if (GroupBy != null) builder.Append(' ').Append(GroupBy);
        if (Having != null) builder.Append(" HAVING ").Append(Having);

        // WINDOW 命名窗口定义（HAVING 之后）：WINDOW w1 AS (...), w2 AS (...)
        if (WindowDefinitions is { Count: > 0 })
            builder.Append(" WINDOW ").Append(string.Join(", ", WindowDefinitions));

        // QUALIFY 过滤（Snowflake/Teradata，ORDER BY/LIMIT 之前但在 WINDOW 之后）
        if (Qualify != null) builder.Append(" QUALIFY ").Append(Qualify);

        // MySQL INTO OUTFILE/DUMPFILE 尾部位置
        if (MySqlIntoOutfile is { BeforeFrom: false }) builder.Append(' ').Append(MySqlIntoOutfile);

        // DB2 OPTIMIZE FOR n ROWS
        if (OptimizeFor.HasValue) builder.Append(" OPTIMIZE FOR ").Append(OptimizeFor.Value).Append(" ROWS");

        return builder;
    }

    public static string OrderByToString(List<OrderByElement> orderByElements)
    {
        if (orderByElements == null || orderByElements.Count == 0) return "";
        return " ORDER BY " + string.Join(", ", orderByElements);
    }

    public static string GetStringList<T>(IEnumerable<T> list, bool useComma = true, bool useBrackets = false)
    {
        if (list == null) return "";
        var items = list.Select(x => x?.ToString() ?? "");
        var result = useComma ? string.Join(", ", items) : string.Join(" ", items);
        if (useBrackets) result = $"({result})";
        return result;
    }
}
