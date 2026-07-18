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
    public Expression.IExpression? First { get; set; }

    /// <summary>Informix SKIP n 量词（SELECT SKIP n FIRST m ...），未指定时为 null。</summary>
    public Expression.IExpression? Skip { get; set; }

    /// <summary>DB2 OPTIMIZE FOR n ROWS 子句，未指定时为 null。</summary>
    public long? OptimizeFor { get; set; }

    public List<SelectItem>? SelectItems { get; set; }

    public IFromItem? FromItem { get; set; }

    public List<Join>? Joins { get; set; }

    public Expression.IExpression? Where { get; set; }

    public PreferringClause? Preferring { get; set; }

    public GroupByElement? GroupBy { get; set; }

    public Expression.IExpression? Having { get; set; }

    /// <summary>MySQL INTO OUTFILE/DUMPFILE 子句（前置或尾部位置），未指定时为 null。</summary>
    public MySqlIntoOutfile? MySqlIntoOutfile { get; set; }

    /// <summary>SQL Server FOR XML PATH('name') 的路径名，无 FOR XML PATH 时为 null，空字符串表示无参数的 FOR XML PATH。</summary>
    public string? ForXmlPath { get; set; }

    /// <summary>
    /// SQL Server FOR CLAUSE 透传文本（FOR BROWSE / FOR XML RAW|AUTO|EXPLICIT|PATH / FOR JSON AUTO|PATH），
    /// 含 FOR 关键字后的完整子句文本。与 ForXmlPath 互斥（ForClause 优先）。对齐上游 ForClause。
    /// </summary>
    public string? ForClause { get; set; }

    /// <summary>Oracle 优化器提示（SELECT 关键字后紧跟的 /*+ ... */ 或 --+ ... 注释）。</summary>
    public OracleHint? OracleHint { get; set; }

    /// <summary>命名窗口定义（WINDOW w AS (...)），透传原始文本保 round-trip。对齐上游 windowDefinitions。</summary>
    public List<string>? WindowDefinitions { get; set; }

    /// <summary>ksqlDB 流式窗口（WINDOW HOPPING/TUMBLING/SESSION），对齐上游 ksqlWindow。与 WindowDefinitions 互斥。</summary>
    public KSQLWindow? KsqlWindow { get; set; }

    /// <summary>ksqlDB EMIT CHANGES 标志，对齐上游 emitChanges。</summary>
    public bool EmitChanges { get; set; }

    /// <summary>QUALIFY 过滤表达式（Snowflake/Teradata），对齐上游 qualify。</summary>
    public Expression.IExpression? Qualify { get; set; }

    /// <summary>Oracle 层次查询（START WITH ... CONNECT BY ...），对齐上游 oracleHierarchical。</summary>
    public OracleHierarchicalExpression? OracleHierarchical { get; set; }

    /// <summary>
    /// PostgreSQL/Informix <c>SELECT ... INTO target_table</c> 的目标表列表。对齐上游 intoTables。
    /// 与 <see cref="IntoTempTable"/> 互斥；与 <see cref="MySqlIntoOutfile"/> 也互斥。
    /// 未指定时为 null。
    /// </summary>
    public List<Table>? IntoTables { get; set; }

    /// <summary>
    /// Informix <c>SELECT ... INTO TEMP tmp_table</c> 的临时表。对齐上游 intoTempTable。
    /// 与 <see cref="IntoTables"/> 互斥。未指定时为 null。
    /// </summary>
    public Table? IntoTempTable { get; set; }

    /// <summary>
    /// MySQL <c>SELECT ... INTO @var, @var2</c> 的用户变量列表。对齐上游 PlainSelect.intoParams。
    /// 与 <see cref="IntoTables"/>、<see cref="IntoTempTable"/>、<see cref="MySqlIntoOutfile"/> 互斥。未指定时为 null。
    /// </summary>
    public List<string>? IntoVariables { get; set; }

    /// <summary>
    /// SQL Server 查询级 OPTION 提示文本（不含 OPTION 关键字），如 <c>MAXRECURSION 2, HASH JOIN</c>。
    /// 整体透传，未指定时为 null。
    /// </summary>
    public string? OptionHints { get; set; }

    /// <summary>
    /// MySQL <c>SELECT ... PROCEDURE function(...)</c> 子句（5.7 弃用、8.0 移除，仍保留兼容老 SQL）。
    /// 透传 PROCEDURE 关键字之后的完整文本，未指定时为 null。
    /// </summary>
    public string? MySqlProcedure { get; set; }

    public override T Accept<T, S>(ISelectVisitor<T> selectVisitor, S context)
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

        // PostgreSQL/Informix SELECT ... INTO target / INTO TEMP tmp（FROM 之前）
        // 对齐上游 PlainSelect.java:566-573 / 648-649
        if (IntoTables is { Count: > 0 })
        {
            builder.Append(" INTO ").Append(string.Join(", ", IntoTables));
        }
        else if (IntoTempTable != null)
        {
            builder.Append(" INTO TEMP ").Append(IntoTempTable);
        }
        else if (IntoVariables is { Count: > 0 })
        {
            // MySQL SELECT ... INTO @x, @y（FROM 之前，与 INTO target_table 同位）
            builder.Append(" INTO ").Append(string.Join(", ", IntoVariables));
        }

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

        // ksqlDB 流式窗口（FROM/JOIN 之后、WHERE 之前）
        if (KsqlWindow != null) builder.Append(" WINDOW ").Append(KsqlWindow);

        if (Where != null) builder.Append(" WHERE ").Append(Where);

        // Oracle 层次查询（WHERE 之后、GROUP BY 之前）
        if (OracleHierarchical != null) builder.Append(' ').Append(OracleHierarchical);
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

        // MySQL SELECT ... PROCEDURE function(...)（OPTIMIZE FOR 之后、OPTION 之前）
        if (MySqlProcedure != null) builder.Append(" PROCEDURE ").Append(MySqlProcedure);

        // SQL Server OPTION (...)
        if (OptionHints != null) builder.Append(" OPTION (").Append(OptionHints).Append(')');

        return builder;
    }

    public static string OrderByToString(List<OrderByElement> orderByElements)
    {
        if (orderByElements == null || orderByElements.Count == 0) return "";
        return " ORDER BY " + string.Join(", ", orderByElements);
    }
}