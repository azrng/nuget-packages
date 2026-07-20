using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;
using Azrng.JSqlParser.Test.Helpers.NativeSqlParse;

namespace Azrng.JSqlParser.Test;

/// <summary>
/// 探针测试：通过 <see cref="NativeSqlParserService"/>（SynyiTools 上层消费者样板）
/// 验证类库对真实业务 SQL 的解析能力。
/// </summary>
/// <remarks>
/// NativeSqlParserService 的解析逻辑与 DTO 拷贝自
/// <c>C:\Work\temp-tools\SynyiTools\Service\NativeSqlParserService.cs</c>，
/// 位于 <c>Helpers/NativeSqlParse/</c> 目录。
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
        var result = NativeSqlParserService.Parse(MultiJoinAndComplexWhereSql);

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
        // 本 SQL 全部使用字面量（字符串/数字/IN 列表），无命名参数，故 WhereList 为空。
        // 此处断言该设计行为，避免误判为类库缺陷。
        Assert.Empty(result.WhereList);

        // 但 WHERE 表达式本身应被类库正确解析为 AndExpression（5 段 AND 链）。
        var plain = (PlainSelect)(Select)SqlParser.Parse(MultiJoinAndComplexWhereSql)!;
        Assert.IsType<AndExpression>(plain.Where);
        Assert.Contains("c.is_valid = '1'", plain.Where!.ToString());
        Assert.Contains("c.org_code IN ('NKYY_015')", plain.Where.ToString());
        Assert.Contains("c.out_time >= DATE_TRUNC('month', CURRENT_DATE - INTERVAL '5 month')", plain.Where.ToString());

        // PostgreSQL 正则运算符 ~ / !~ 不再被误解析为 =（visitor TILDE 分支修复后）
        Assert.Contains("c.record_title ~ '危急值|医患沟通'", plain.Where.ToString());
        Assert.Contains("c.record_title !~ '申请|同意'", plain.Where.ToString());
        Assert.DoesNotContain("c.record_title = '危急值", plain.Where.ToString());
    }

    [Fact]
    public void ExistsSubQuery_Parse_ShouldExtractMainTableAndColumns()
    {
        var result = NativeSqlParserService.Parse(ExistsSubQuerySql);

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

    // === 分支补强测试：覆盖 NativeSqlParserService 的其余代码路径 ===

    [Fact]
    public void Star_Select_ShouldProduceAllColumnsEntry()
    {
        var result = NativeSqlParserService.Parse("SELECT * FROM users");

        Assert.Equal("users", result.TableNameList["users"]);
        var col = Assert.Single(result.ColumnList);
        Assert.Equal("*", col.TableAlias);
        Assert.Equal("*", col.ColumnName);
        Assert.False(col.IsVirtual);
    }

    [Fact]
    public void TableStar_Select_ShouldCarryTableAlias()
    {
        var result = NativeSqlParserService.Parse("SELECT u.* FROM users u");

        Assert.Equal("users", result.TableNameList["u"]);
        var col = Assert.Single(result.ColumnList);
        Assert.Equal("u", col.TableAlias);
        Assert.Equal("*", col.ColumnName);
    }

    [Fact]
    public void SingleTable_ColumnWithoutPrefix_ShouldFallbackToTableName()
    {
        // 单表场景下，列无显式表前缀时，GetColumnTableAlias 回退为唯一表名
        var result = NativeSqlParserService.Parse("SELECT id, name FROM users");

        AssertColumn(result.ColumnList, tableAlias: "users", columnName: "id", columnAlias: null);
        AssertColumn(result.ColumnList, tableAlias: "users", columnName: "name", columnAlias: null);
    }

    [Fact]
    public void MultiTable_ColumnWithoutPrefix_ShouldYieldEmptyAlias()
    {
        // 多表场景下，列无显式表前缀时，GetColumnTableAlias 返回空（无法归属）
        var result = NativeSqlParserService.Parse("SELECT id FROM users u JOIN orders o ON u.id = o.uid");

        var col = Assert.Single(result.ColumnList);
        Assert.Equal("", col.TableAlias);
        Assert.Equal("id", col.ColumnName);
    }

    [Fact]
    public void NamedParameter_Where_ShouldProduceOperatorWithTableNameLookup()
    {
        // 核心路径：右侧为命名参数(:param / @param) 才会产出 OperatorList 记录，
        // 并通过 Alias 反查填 TableName。
        var result = NativeSqlParserService.Parse("SELECT id FROM users WHERE id = :userId AND name = @nm");

        Assert.Equal(2, result.WhereList.Count);
        var first = result.WhereList[0];
        Assert.Equal("=", first.StringExpression);
        Assert.Equal("", first.LinkType); // 链首连接符为空
        Assert.Equal("id", first.LeftExpression!.ColumnName);
        Assert.Equal("userId", first.RightExpression!.Name);
        Assert.Equal("users", first.TableName); // Alias=users 反查到表名
        Assert.Equal(1, first.Order);

        var second = result.WhereList[1];
        Assert.Equal("AND", second.LinkType);
        Assert.Equal("name", second.LeftExpression!.ColumnName);
        Assert.Equal("nm", second.RightExpression!.Name);
        Assert.Equal(2, second.Order);
    }

    [Fact]
    public void CastExpression_VirtualColumn_ShouldCaptureDataTypeAndSource()
    {
        // 单一来源列：虚拟列填充 SourceTableAlias / SourceColumnName / ColumnType
        var result = NativeSqlParserService.Parse("SELECT CAST(age AS VARCHAR) AS a FROM users u");

        var col = Assert.Single(result.ColumnList, c => c.ColumnAlias == "a");
        Assert.True(col.IsVirtual);
        Assert.Equal("VARCHAR", col.ColumnType);
        Assert.Equal("u", col.SourceTableAlias);
        Assert.Equal("age", col.SourceColumnName);
        Assert.Equal("CAST(age AS VARCHAR)", col.ExpressionSql);
    }

    [Fact]
    public void ExpressionColumn_WithoutAlias_ShouldThrow()
    {
        // 异常路径：表达式列必须指定别名
        var ex = Assert.Throws<ArgumentException>(() => NativeSqlParserService.Parse("SELECT id + 1 FROM users"));
        Assert.Contains("表达式列必须指定别名", ex.Message);
    }

    [Fact]
    public void NonSelectStatement_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentException>(() => NativeSqlParserService.Parse("DELETE FROM users WHERE id = 1"));
        Assert.Contains("仅支持SELECT语句", ex.Message);
    }

    [Fact]
    public void Union_ShouldCollectBothTablesButOnlyFirstBranchColumns()
    {
        // SetOperationList：两分支表都收集，列只取首分支（设计行为）
        var result = NativeSqlParserService.Parse("SELECT id FROM users UNION SELECT id FROM orders");

        Assert.Equal(2, result.TableNameList.Count);
        Assert.Equal("users", result.TableNameList["users"]);
        Assert.Equal("orders", result.TableNameList["orders"]);
        Assert.Single(result.ColumnList);
    }

    /// <summary>
    /// JOIN 的 ON 连接条件也参与操作符收集（含命名参数时进入 WhereList）。
    /// 修复前 CollectPlainSelect 未遍历 join.OnExpressions，导致 ON 条件丢失。
    /// </summary>
    [Fact]
    public void JoinOnCondition_ShouldBeCollectedIntoWhereList()
    {
        var result = NativeSqlParserService.Parse("SELECT a.id FROM users a JOIN orders b ON a.id = b.uid WHERE b.is_valid = :v");

        // ON a.id = b.uid 右侧是列（非命名参数），按 AddOperator 设计不产出记录；
        // 仅 WHERE 的 b.is_valid = :v 产出一条。
        var op = Assert.Single(result.WhereList);
        Assert.Equal("is_valid", op.LeftExpression!.ColumnName);
        Assert.Equal("v", op.RightExpression!.Name);
    }

    /// <summary>
    /// JOIN ON 含命名参数时，连接条件也进入 WhereList（验证 ON 收集修复）。
    /// </summary>
    [Fact]
    public void JoinOnCondition_WithNamedParameter_ShouldBeCollected()
    {
        var result = NativeSqlParserService.Parse("SELECT a.id FROM users a JOIN orders b ON a.id = :uid WHERE b.is_valid = :v");

        Assert.Equal(2, result.WhereList.Count);
        Assert.Contains(result.WhereList, w => w.LeftExpression!.ColumnName == "id" && w.RightExpression!.Name == "uid");
        Assert.Contains(result.WhereList, w => w.LeftExpression!.ColumnName == "is_valid" && w.RightExpression!.Name == "v");
    }

    [Fact]
    public void WithTableWhere_Test()
    {
        var sql = """
                  WITH base_cases AS (SELECT c.org_code                  as org_code,
                                             c.hos_area                  as hos_area,
                                             c.source_patient_id         as source_patient_id,
                                             c.source_inpatient_visit_id as source_inpatient_visit_id,
                                             c.source_inpat_no           as source_inpat_no
                                      FROM cases.inpatient_case_info c
                                      WHERE c.is_valid = '1'
                                        AND c.org_code IN (@org_code)
                                        AND c.out_time >= @begin_time
                                        AND c.out_time < @end_time)
                  SELECT t1.org_code                  AS org_code,
                         t1.hos_area                  AS hos_area,
                         a.begin_time                 AS yzkssj,
                         CASE
                             WHEN a.order_type_code = '2' THEN a.begin_time
                             ELSE a.end_time
                             END                      AS yzjssj,
                         '0' || a.order_type_code     AS yzlb,
                         a.order_class_code           AS yzxmflbm,
                         a.specs                      AS ypgg,
                         NULL                         AS yfdw,
                         a.once_dose                  AS yl
                                      FROM base_cases t1
                                          INNER JOIN orders.inpat_undrug_order a
                                      ON t1.source_inpatient_visit_id = a.source_order_id
                                          AND t1.org_code = a.org_code
                                          AND a.source_app != '019_DH_HIS_015'
                                          AND a.is_valid = '1'

                  """;
        var result = NativeSqlParserService.Parse(sql);
        Assert.True(result.WhereList.Count > 0);
    }
}