using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test;

/// <summary>
/// 探针测试：把 SynyiTools.Service.NativeSqlParserService 的核心解析逻辑（NativeSqlParseResult 装配）
/// 拷贝到测试内部，验证类库对真实业务 SQL 的解析能力。
/// </summary>
/// <remarks>
/// 这里的 DTO 与解析逻辑与 C:\Work\temp-tools\SynyiTools\Service\NativeSqlParserService.cs 保持一致，
/// 仅做两点最小适配：JExpression -> IExpression、IsNotNullOrWhiteSpace -> !string.IsNullOrWhiteSpace。
/// 测试目的：确认类库能正确解析这两段 SQL，并通过 NativeSqlParseResult 暴露足够信息。
/// </remarks>
public class ProbeExtractTest
{
    /// <summary>SQL1：多表 JOIN + CASE WHEN + 复杂 WHERE（IN / 正则 ~ / !~ / DATE_TRUNC）。</summary>
    private const string MultiJoinAndComplexWhereSql = """
                                                       SELECT a.org_code,
                                                              a.hos_area,
                                                              b.org_alias,
                                                              a.source_case_diagnose_id   AS ID,
                                                              c.source_patient_id         AS HZID,
                                                              c.source_inpatient_visit_id AS ZYID,
                                                              c.source_inpatient_case_id  AS CASEID,
                                                              a.diag_class_name_orig      AS yszdlb,
                                                              a.diag_class_name           AS ptzdlb,
                                                              a.is_main_flag              AS ptzzd,
                                                              CASE
                                                                  WHEN a.org_code IN ('JNZQ_010', 'FKZY_013') THEN a.happen_no
                                                                  ELSE a.happen_no - 1
                                                                  END                     AS XH,
                                                              a.diag_name_orig            AS JBMC
                                                       FROM cases.inpatient_case_info c
                                                                INNER JOIN cases.inpatient_case_diagnose a
                                                                           ON a.source_inpatient_case_id = c.source_inpatient_case_id
                                                                               AND a.org_code = c.org_code
                                                                               AND a.hos_area = c.hos_area
                                                                               AND a.source_app = c.source_app
                                                                               AND a.is_valid = '1'
                                                                INNER JOIN mdm.organization b
                                                                           ON a.org_code = b.zuhao
                                                                               AND a.hos_area = b.hos_area
                                                                               AND b.zuhao != 'HDYY_020'
                                                                               AND b.note = '1'
                                                       WHERE c.is_valid = '1'
                                                         AND c.org_code IN ('NKYY_015')
                                                           AND ( c.record_type_name IN ('会诊记录', '会诊其他记录')
                                                           OR  c.record_title ~ '危急值|医患沟通'
                                                           OR ( c.record_title ~ '多学科' AND  c.record_title !~ '申请|同意')
                                                           )
                                                         AND c.out_time >= DATE_TRUNC('month', CURRENT_DATE - INTERVAL '5 month')
                                                         AND c.out_time < DATE_TRUNC('month', CURRENT_DATE - INTERVAL '4 month')
                                                       """;

    /// <summary>SQL2：SELECT 列表中嵌入 EXISTS 相关子查询。</summary>
    private const string ExistsSubQuerySql = """
                                             SELECT a.inpatient_case_id,
                                                    CASE
                                                        WHEN EXISTS(SELECT 1
                                                                    FROM cases.inpatient_case_operation o
                                                                    WHERE o.is_valid = 1
                                                                      AND o.group_operation_class = '手术'
                                                                      AND a.source_inpatient_case_id = o.source_inpatient_case_id
                                                                      AND a.org_code = o.org_code
                                                                      AND a.source_app = o.source_app) THEN 1
                                                        ELSE 2 END AS is_operation_flag
                                             FROM cases.inpatient_case_info a
                                             """;

    [Fact]
    public void MultiJoinAndComplexWhere_Parse_ShouldExtractTablesColumnsAndOperators()
    {
        var result = NativeSqlProbe.Parse(MultiJoinAndComplexWhereSql);

        // 三个表别名 -> 全限定名（schema.table）
        Assert.Equal(3, result.TableNameList.Count);
        Assert.Equal("cases.inpatient_case_info", result.TableNameList["c"]);
        Assert.Equal("cases.inpatient_case_diagnose", result.TableNameList["a"]);
        Assert.Equal("mdm.organization", result.TableNameList["b"]);

        // SELECT 列：12 个输出列（其中 CASE...END AS XH 为虚拟列）
        Assert.Equal(12, result.ColumnList.Count);
        AssertColumn(result.ColumnList, tableAlias: "a", columnName: "org_code", columnAlias: null);
        AssertColumn(result.ColumnList, tableAlias: "a", columnName: "diag_class_name_orig", columnAlias: "yszdlb");
        AssertColumn(result.ColumnList, tableAlias: "c", columnName: "source_patient_id", columnAlias: "HZID");

        // CASE WHEN ... AS XH 为虚拟列
        var xh = Assert.Single(result.ColumnList, c => c.ColumnAlias == "XH");
        Assert.True(xh.IsVirtual);
        Assert.Equal("XH", xh.ColumnName);

        // 条件操作符：NativeSqlParserService 仅收集"右侧含命名参数(:param/@param)"的操作符，
        // 本 SQL 全部使用字面量（字符串/数字/IN 列表），无命名参数，故 OperatorList 为空。
        // 此处断言该设计行为，避免误判为类库缺陷。
        Assert.Empty(result.WhereList);

        // 但 WHERE 表达式本身应被类库正确解析为 AndExpression（5 段 AND 链）。
        var plain = (PlainSelect)(Select)SqlParser.Parse(MultiJoinAndComplexWhereSql)!;
        Assert.IsType<AndExpression>(plain.Where);
        Assert.Contains("c.is_valid = '1'", plain.Where!.ToString());
        Assert.Contains("c.org_code IN ('NKYY_015')", plain.Where.ToString());
        Assert.Contains("c.out_time >= DATE_TRUNC('month', CURRENT_DATE - INTERVAL '5 month')", plain.Where.ToString());
    }

    [Fact]
    public void ExistsSubQuery_Parse_ShouldExtractMainTableAndColumns()
    {
        var result = NativeSqlProbe.Parse(ExistsSubQuerySql);

        // 主表只有一张；EXISTS 子查询表不在 FROM 顶层，不进 TableNameList
        Assert.Single(result.TableNameList);
        Assert.Equal("cases.inpatient_case_info", result.TableNameList["a"]);

        // 输出列 2 个：a.inpatient_case_id（普通列） + CASE...END AS is_operation_flag（虚拟列）
        Assert.Equal(2, result.ColumnList.Count);
        AssertColumn(result.ColumnList, tableAlias: "a", columnName: "inpatient_case_id", columnAlias: null);

        var opFlag = Assert.Single(result.ColumnList, c => c.ColumnAlias == "is_operation_flag");
        Assert.True(opFlag.IsVirtual);
    }

    private static void AssertColumn(IEnumerable<NativeSqlSelectColumnInfo> columns, string tableAlias, string columnName,
                                     string? columnAlias)
    {
        var hit = Assert.Single(columns, c => c.TableAlias == tableAlias && c.ColumnName == columnName);
        Assert.Equal(columnAlias, hit.ColumnAlias);
        Assert.False(hit.IsVirtual);
    }
}

/// <summary>
/// 探针：从 SQL 装配 NativeSqlParseResult。
/// 与 SynyiTools.Service.NativeSqlParserService 的 ParseSelect 逻辑一致。
/// </summary>
internal static class NativeSqlProbe
{
    public static NativeSqlParseResult Parse(string sql)
    {
        var statement = SqlParser.Parse(sql) ?? throw new ArgumentException("解析失败");
        if (statement is not Select select)
            throw new ArgumentException("仅支持SELECT语句");

        var context = new NativeSqlParseResult();
        CollectSelect(select, context, includeColumns: true);

        foreach (var operatorInfo in context.WhereList)
        {
            operatorInfo.TableName = !string.IsNullOrWhiteSpace(operatorInfo.Alias) &&
                                     context.TableNameList.TryGetValue(operatorInfo.Alias, out var tableName)
                ? tableName
                : string.Empty;
        }

        return new NativeSqlParseResult
               {
                   TableNameList = context.TableNameList,
                   ColumnList = context.ColumnList,
                   WhereList = context.WhereList.Where(t => !string.IsNullOrWhiteSpace(t.Alias)).ToList()
               };
    }

    private static void CollectSelect(Select select, NativeSqlParseResult context, bool includeColumns)
    {
        switch (select)
        {
            case PlainSelect plainSelect:
                CollectPlainSelect(plainSelect, context, includeColumns);
                break;

            case SetOperationList setOperationList:
                for (var index = 0; index < setOperationList.Selects.Count; index++)
                {
                    CollectSelect(setOperationList.Selects[index], context, includeColumns && index == 0);
                }

                break;

            default:
                throw new ArgumentException($"不支持的SELECT类型:{select.GetType().Name}");
        }
    }

    private static void CollectPlainSelect(PlainSelect select, NativeSqlParseResult context, bool includeColumns)
    {
        CollectFromItem(select.FromItem, context);
        if (select.Joins != null)
        {
            foreach (var join in select.Joins)
            {
                CollectFromItem(join.RightItem, context);
            }
        }

        if (includeColumns)
            CollectSelectItems(select.SelectItems, context);

        CollectOperators(select.Where, context, string.Empty);
        CollectOperators(select.Having, context, string.Empty);
    }

    private static void CollectFromItem(IFromItem? fromItem, NativeSqlParseResult context)
    {
        switch (fromItem)
        {
            case Table table:
                AddTable(table, context);
                break;

            case ParenthesedSelect parenthesedSelect:
                CollectSelect(parenthesedSelect.Select, context, false);
                break;
        }
    }

    private static void AddTable(Table table, NativeSqlParseResult context)
    {
        var alias = table.Alias?.Name;
        if (string.IsNullOrWhiteSpace(alias))
            alias = table.Name;

        if (!context.TableNameList.ContainsKey(alias))
            context.TableNameList.Add(alias, table.GetFullyQualifiedName());
    }

    private static void CollectSelectItems(List<SelectItem>? selectItems, NativeSqlParseResult context)
    {
        if (selectItems == null)
            return;

        foreach (var item in selectItems)
        {
            switch (item.Expression)
            {
                case AllColumns:
                    context.ColumnList.Add(new NativeSqlSelectColumnInfo { TableAlias = "*", ColumnName = "*" });
                    break;

                case AllTableColumns allTableColumns:
                    context.ColumnList.Add(new NativeSqlSelectColumnInfo { TableAlias = allTableColumns.Table.Name, ColumnName = "*" });
                    break;

                case Column column:
                    context.ColumnList.Add(new NativeSqlSelectColumnInfo
                                           {
                                               TableAlias = GetColumnTableAlias(column, context),
                                               ColumnName = column.ColumnName,
                                               ColumnAlias = item.Alias?.Name
                                           });
                    break;

                default:
                    context.ColumnList.Add(BuildVirtualColumn(item, context));
                    break;
            }
        }
    }

    private static NativeSqlSelectColumnInfo BuildVirtualColumn(SelectItem item, NativeSqlParseResult context)
    {
        var alias = item.Alias?.Name;
        if (string.IsNullOrWhiteSpace(alias))
            throw new ArgumentException($"表达式列必须指定别名:{item.Expression}");

        var columns = CollectColumns(item.Expression)
                      .Select(column => new ColumnRef { TableAlias = GetColumnTableAlias(column, context), ColumnName = column.ColumnName })
                      .DistinctBy(column => $"{column.TableAlias}.{column.ColumnName}")
                      .ToList();
        var sourceColumn = columns.Count == 1 ? columns[0] : null;

        return new NativeSqlSelectColumnInfo
               {
                   TableAlias = sourceColumn?.TableAlias,
                   ColumnName = alias,
                   ColumnAlias = alias,
                   IsVirtual = true,
                   ExpressionSql = item.Expression.ToString(),
                   ColumnType = item.Expression is CastExpression castExpression ? castExpression.DataType : null,
                   SourceTableAlias = sourceColumn?.TableAlias,
                   SourceColumnName = sourceColumn?.ColumnName
               };
    }

    private static void CollectOperators(IExpression? expression, NativeSqlParseResult context, string linkType)
    {
        if (expression == null)
            return;

        switch (expression)
        {
            case AndExpression andExpression:
                CollectOperators(andExpression.LeftExpression, context, linkType);
                CollectOperators(andExpression.RightExpression, context, "AND");
                break;

            case OrExpression orExpression:
                CollectOperators(orExpression.LeftExpression, context, linkType);
                CollectOperators(orExpression.RightExpression, context, "OR");
                break;

            case BinaryExpression binaryExpression:
                AddBinaryOperator(binaryExpression, context, linkType);
                break;

            case InExpression inExpression:
                AddOperator(inExpression.LeftExpression, inExpression.RightExpression, inExpression.Not ? "NOT IN" : "IN",
                    context, linkType, inExpression.ToString() ?? string.Empty);
                break;

            case Between between:
                AddOperator(between.LeftExpression, between.BetweenExpressionStart, between.Not ? "NOT BETWEEN" : "BETWEEN",
                    context, linkType, between.ToString() ?? string.Empty);
                AddOperator(between.LeftExpression, between.BetweenExpressionEnd, between.Not ? "NOT BETWEEN" : "BETWEEN",
                    context, "AND", between.ToString() ?? string.Empty);
                break;
        }
    }

    private static void AddBinaryOperator(BinaryExpression binaryExpression, NativeSqlParseResult context, string linkType)
    {
        AddOperator(binaryExpression.LeftExpression, binaryExpression.RightExpression, binaryExpression.OperatorSymbol,
            context, linkType, binaryExpression.ToString() ?? string.Empty);
    }

    private static void AddOperator(IExpression leftExpression, IExpression rightExpression, string operatorText,
                                    NativeSqlParseResult context, string linkType, string sqlInfo)
    {
        var column = leftExpression as Column;
        var parameterExpression = rightExpression;
        if (column == null && rightExpression is Column rightColumn)
        {
            column = rightColumn;
            parameterExpression = leftExpression;
        }

        if (column == null)
            return;

        var parameters = CollectParameters(parameterExpression);
        foreach (var parameter in parameters)
        {
            context.WhereList.Add(new NativeSqlOperatorInfo
                                  {
                                      Alias = GetColumnTableAlias(column, context),
                                      LinkType = linkType,
                                      LeftExpression =
                                          new NativeSqlExpressionInfo
                                          {
                                              ColumnName = column.ColumnName,
                                              FullyQualifiedName = column.GetFullyQualifiedName(),
                                              Value = column.ToString()
                                          },
                                      RightExpression = new NativeSqlExpressionInfo { Name = parameter, Value = parameter },
                                      StringExpression = operatorText,
                                      SqlInfo = sqlInfo,
                                      Order = context.WhereList.Count + 1
                                  });
        }
    }

    private static string GetColumnTableAlias(Column column, NativeSqlParseResult context)
    {
        var tableAlias = column.Table?.Name;
        if (!string.IsNullOrWhiteSpace(tableAlias))
            return tableAlias;

        return context.TableNameList.Count == 1 ? context.TableNameList.Keys.First() : string.Empty;
    }

    private static List<Column> CollectColumns(IExpression expression)
    {
        return expression.Descendants<Column>().ToList();
    }

    private static List<string> CollectParameters(IExpression expression)
    {
        return expression.Descendants<JdbcNamedParameter>()
                         .Select(parameter => parameter.Name)
                         .Where(parameterName => !string.IsNullOrWhiteSpace(parameterName))
                         .ToList();
    }
}

/// <summary>从 SynyiTools.Service.NativeSqlParserService 拷贝的结果 DTO。</summary>
internal sealed class NativeSqlParseResult
{
    public IDictionary<string, string> TableNameList { get; set; } = new Dictionary<string, string>();

    public List<NativeSqlSelectColumnInfo> ColumnList { get; set; } = [];

    public List<NativeSqlOperatorInfo> WhereList { get; set; } = [];
}

internal sealed class NativeSqlSelectColumnInfo
{
    public string? TableAlias { get; set; }

    public string? ColumnName { get; set; }

    public string? ColumnAlias { get; set; }

    public bool IsVirtual { get; set; }

    public string? ExpressionSql { get; set; }

    public string? ColumnType { get; set; }

    public string? SourceTableAlias { get; set; }

    public string? SourceColumnName { get; set; }
}

internal sealed class NativeSqlOperatorInfo
{
    public string? Alias { get; set; }

    public string? LinkType { get; set; }

    public string? TableName { get; set; }

    public NativeSqlExpressionInfo? LeftExpression { get; set; }

    public NativeSqlExpressionInfo? RightExpression { get; set; }

    public string? StringExpression { get; set; }

    public string? SqlInfo { get; set; }

    public int Order { get; set; }
}

internal sealed class NativeSqlExpressionInfo
{
    public string? Name { get; set; }

    public string? Value { get; set; }

    public string? ColumnName { get; set; }

    public string? FullyQualifiedName { get; set; }
}

internal sealed class ColumnRef
{
    public string? TableAlias { get; set; }

    public string? ColumnName { get; set; }
}