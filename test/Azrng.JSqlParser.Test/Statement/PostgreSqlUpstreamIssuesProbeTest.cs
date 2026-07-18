using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// 上游 PostgreSQL 专项 issue（issue 分类清单 ④，12 条）移植版现状探针。
///
/// 数据来源：issue/jsqlparser/issue分类清单.md 第 ④ 类「PostgreSQL 专项」[12 条]。
/// 每个测试方法对应一条上游 issue，用上游报告里的代表性 SQL 验证 Azrng 移植版
/// 是否复现同样的解析缺陷（仅断言「能解析 + ToString 不抛异常」，不做 round-trip 等值要求）。
///
/// 探针定位的是「是否存在上游同类缺陷」，不要求 AST 结构完全正确。
/// 失败 = 复现上游缺陷；通过 = 移植版已不存在该缺陷。
/// </summary>
public class PostgreSqlUpstreamIssuesProbeTest
{
    #region #2432 LIKE ANY / ALL (ARRAY[...])

    [Fact]
    public void Issue2432_LikeAnyArray_ShouldParse()
    {
        var sql = "SELECT * FROM t WHERE col LIKE ANY (ARRAY['%a%', '%b%'])";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    [Fact]
    public void Issue2432_ILikeAnyArray_ShouldParse()
    {
        var sql = "SELECT * FROM t WHERE col ILIKE ANY (ARRAY['%a%', '%b%'])";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    [Fact]
    public void Issue2432_LikeAllArray_ShouldParse()
    {
        var sql = "SELECT * FROM t WHERE col LIKE ALL (ARRAY['%a%', '%b%'])";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    [Fact]
    public void Issue2432_LikeAnyInCaseWhen_ShouldParse()
    {
        var sql =
            "SELECT CASE WHEN category = 'stock' " +
            "AND transaction_type LIKE ANY (ARRAY['%deposit%', '%inflow%', '%dividend%']) " +
            "THEN 1 ELSE 0 END AS inflow_count FROM transactions";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    #endregion

    #region #2431 窗口帧 GROUPS

    [Fact]
    public void Issue2431_WindowFrameGroups_ShouldParse()
    {
        var sql =
            "SELECT id, SUM(val) OVER (" +
            "ORDER BY ts GROUPS BETWEEN 1 PRECEDING AND CURRENT ROW" +
            ") AS s FROM events";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    #endregion

    #region #2430 窗口帧 EXCLUDE TIES

    [Fact]
    public void Issue2430_WindowFrameExcludeTies_ShouldParse()
    {
        var sql =
            "SELECT id, SUM(amount) OVER (" +
            "ORDER BY amount RANGE BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW EXCLUDE TIES" +
            ") AS s FROM sales";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    [Fact]
    public void Issue2430_WindowFrameExcludeNoOthers_ShouldParse()
    {
        var sql =
            "SELECT SUM(amount) OVER (ORDER BY amount RANGE BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW EXCLUDE NO OTHERS) FROM sales";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    #endregion

    #region #2412 json_populate_record 行展开 (expr).*

    [Fact]
    public void Issue2412_RowExpansionStar_ShouldParse()
    {
        var sql = "INSERT INTO users SELECT (json_populate_record(NULL::users, data)).* FROM staging_users";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    #endregion

    #region #2411 ROWS FROM

    [Fact]
    public void Issue2411_RowsFrom_ShouldParse()
    {
        var sql = "SELECT * FROM ROWS FROM (generate_series(1,3), generate_series(10,12)) AS t(a,b)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    #endregion

    #region #2342 深层嵌套导致 NPE

    [Fact]
    public void Issue2342_DeepParenNesting_ShouldNotThrow()
    {
        // 上游报告：68 层括号嵌套 (1+1) 触发 NPE/栈问题
        var deep = new string('(', 68) + "1+1" + new string(')', 68);
        var sql = "SELECT " + deep;
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    [Fact]
    public void Issue2342_DeepFunctionNesting_ShouldNotThrow()
    {
        // 上游报告：20 层 abs(abs(...abs(1)...)) 触发 NPE
        var inner = "1";
        for (var i = 0; i < 20; i++)
        {
            inner = "abs(" + inner + ")";
        }
        var sql = "SELECT " + inner;
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    #endregion

    #region #2326 XMLTable 函数

    [Fact]
    public void Issue2326_XmlTable_ShouldParse()
    {
        var sql =
            "SELECT xmltable.* FROM xmldata, " +
            "XMLTABLE('//ROWS/ROW' PASSING data COLUMNS (" +
            "id int PATH '@id', ordinality FOR ORDINALITY, " +
            "COUNTRY_NAME text, country_id text PATH 'COUNTRY_ID')) ";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    #endregion

    #region #2233 dollar-quoted 带标签字符串 $tag$...$tag$

    [Fact]
    public void Issue2233_DollarQuotedWithTag_ShouldParse()
    {
        var sql = "SELECT $json$\n[{\"some\":\"json\"}]\n$json$::jsonb";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    [Fact]
    public void Issue2233_DollarQuotedPlain_ShouldParse()
    {
        // 无标签的 $$ ... $$ 形式
        var sql = "SELECT $$hello world$$";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    #endregion

    #region #1728 CREATE TABLE 内 interval hour to minute

    [Fact]
    public void Issue1728_IntervalHourToMinuteInCreateTable_ShouldParse()
    {
        var sql =
            "CREATE TABLE films (" +
            "code char(5), title varchar(40), did integer, date_prod date, " +
            "kind varchar(10), len interval hour to minute, " +
            "CONSTRAINT production UNIQUE(date_prod))";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    #endregion

    #region #1511 JSONB_ARRAY_ELEMENTS() WITH ORDINALITY ARR(...)

    [Fact]
    public void Issue1511_JsonbArrayElementsWithOrdinality_ShouldParse()
    {
        // 聚焦 #1511 的核心特性：表函数后缀 WITH ORDINALITY alias(col, ...)
        var sql =
            "SELECT ARR.ITEM " +
            "FROM NET_FAULT_INFO, " +
            "JSONB_ARRAY_ELEMENTS(MAIN_FAULT_INFO) WITH ORDINALITY ARR(ITEM, POS)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    #endregion

    #region #1416 EXPLAIN 缺新 flag

    [Fact]
    public void Issue1416_ExplainParenOptions_ShouldParse()
    {
        var sql = "EXPLAIN (ANALYZE, VERBOSE, COSTS, BUFFERS) SELECT 1";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    [Fact]
    public void Issue1416_ExplainFormatJson_ShouldParse()
    {
        var sql = "EXPLAIN (FORMAT JSON) SELECT 1";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    [Fact]
    public void Issue1416_ExplainSummary_ShouldParse()
    {
        var sql = "EXPLAIN (SUMMARY) SELECT 1";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    [Fact]
    public void Issue1416_ExplainBooleanOn_ShouldParse()
    {
        var sql = "EXPLAIN (ANALYZE TRUE, VERBOSE ON, COSTS OFF, BUFFERS FALSE) SELECT 1";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    #endregion

    #region #187 FTS 全文查询与函数索引

    [Fact]
    public void Issue187_FtsAtAtOperator_ShouldParse()
    {
        var sql = "SELECT to_tsvector('fat cats ate fat rats') @@ to_tsquery('fat & rat')";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    [Fact]
    public void Issue187_FtsAtAtAtOperator_ShouldParse()
    {
        var sql =
            "SELECT * FROM mytable WHERE (namets @@@ plainto_tsquery('english', 'word')) " +
            "OR (name % 'another') ORDER BY similarity(name, 'blah') DESC";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    [Fact]
    public void Issue187_GistIndex_ShouldParse()
    {
        var sql = "CREATE INDEX idx1 ON mytable USING gist(col1)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.False(string.IsNullOrEmpty(stmt!.ToString()));
    }

    #endregion
}
