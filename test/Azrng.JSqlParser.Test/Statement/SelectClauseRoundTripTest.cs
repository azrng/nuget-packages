using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// SELECT 子句方言与边缘特性 round-trip 测试，覆盖 T091 三维核查发现的 P0/P1 缺口。
/// </summary>
public class SelectClauseRoundTripTest
{
    private static void AssertRoundTrip(string sql)
    {
        var stmt = CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal(sql, stmt.ToString());
    }

    // ── P0: WINDOW 命名窗口（此前 grammar 解析但 visitor 丢弃，round-trip 丢数据） ──

    [Fact]
    public void WindowClause_NamedWindow_ShouldRoundTrip()
        => AssertRoundTrip("SELECT RANK() OVER w FROM t WINDOW w AS (PARTITION BY dept)");

    [Fact]
    public void WindowClause_MultipleWindows_ShouldRoundTrip()
        => AssertRoundTrip("SELECT RANK() OVER w1, DENSE_RANK() OVER w2 FROM t WINDOW w1 AS (PARTITION BY a ORDER BY b), w2 AS (PARTITION BY c)");

    [Fact]
    public void WindowClause_ShouldPopulateWindowDefinitions()
    {
        var stmt = (Select)CCJSqlParserUtil.Parse("SELECT RANK() OVER w FROM t WINDOW w AS (PARTITION BY dept)")!;
        var plain = Assert.IsType<PlainSelect>(stmt);
        Assert.NotNull(plain.WindowDefinitions);
        Assert.Single(plain.WindowDefinitions);
    }

    // ── P0: QUALIFY 子句（此前 grammar 解析但 visitor 丢弃） ──

    [Fact]
    public void QualifyClause_Basic_ShouldRoundTrip()
        => AssertRoundTrip("SELECT * FROM t QUALIFY ROW_NUMBER() OVER (PARTITION BY a ORDER BY b) > 1");

    [Fact]
    public void QualifyClause_ShouldPopulateQualify()
    {
        var stmt = (Select)CCJSqlParserUtil.Parse("SELECT * FROM t QUALIFY ROW_NUMBER() OVER (PARTITION BY a ORDER BY b) > 1")!;
        var plain = Assert.IsType<PlainSelect>(stmt);
        Assert.NotNull(plain.Qualify);
    }

    // ── P1-1: GROUP BY ROLLUP/CUBE/GROUPING SETS ──
    // 注：ROLLUP(a,b)/CUBE(a,b) 作为函数式表达式解析（走 GroupByExpressions），round-trip 一致

    [Fact]
    public void GroupBy_Rollup_ShouldRoundTrip()
        => AssertRoundTrip("SELECT a, b, COUNT(*) FROM t GROUP BY ROLLUP(a, b)");

    [Fact]
    public void GroupBy_Cube_ShouldRoundTrip()
        => AssertRoundTrip("SELECT a, b, COUNT(*) FROM t GROUP BY CUBE(a, b)");

    [Fact]
    public void GroupBy_GroupingSets_ShouldRoundTrip()
        => AssertRoundTrip("SELECT a, b, COUNT(*) FROM t GROUP BY GROUPING SETS ((a, b), (a), ())");

    [Fact]
    public void GroupBy_MysqlWithRollup_ShouldRoundTrip()
        => AssertRoundTrip("SELECT a, COUNT(*) FROM t GROUP BY a WITH ROLLUP");

    [Fact]
    public void GroupBy_Rollup_ShouldParseAsFunction()
    {
        var stmt = (Select)CCJSqlParserUtil.Parse("SELECT a, b FROM t GROUP BY ROLLUP(a, b)")!;
        var plain = Assert.IsType<PlainSelect>(stmt);
        Assert.NotNull(plain.GroupBy);
        Assert.Single(plain.GroupBy!.GroupByExpressions);  // ROLLUP(a,b) 作为单个函数表达式
    }

    [Fact]
    public void GroupBy_GroupingSets_ShouldPopulateGroupingSets()
    {
        var stmt = (Select)CCJSqlParserUtil.Parse("SELECT a, b FROM t GROUP BY GROUPING SETS ((a, b), (a))")!;
        var plain = Assert.IsType<PlainSelect>(stmt);
        Assert.NotNull(plain.GroupBy?.GroupingSets);
        Assert.Equal(2, plain.GroupBy!.GroupingSets!.Count);
    }

    // ── P1-6: REFRESH MATERIALIZED VIEW ──

    [Fact]
    public void RefreshMaterializedView_Basic_ShouldRoundTrip()
        => AssertRoundTrip("REFRESH MATERIALIZED VIEW mv");

    [Fact]
    public void RefreshMaterializedView_WithData_ShouldRoundTrip()
        => AssertRoundTrip("REFRESH MATERIALIZED VIEW mv WITH DATA");

    [Fact]
    public void RefreshMaterializedView_WithNoData_ShouldRoundTrip()
        => AssertRoundTrip("REFRESH MATERIALIZED VIEW mv WITH NO DATA");

    [Fact]
    public void RefreshMaterializedView_Concurrently_ShouldRoundTrip()
        => AssertRoundTrip("REFRESH MATERIALIZED VIEW CONCURRENTLY mv");

    // ── P1-3: SUBSTRING/POSITION/OVERLAY 命名参数语法 ──

    [Fact]
    public void Substring_FromFor_ShouldRoundTrip()
        => AssertRoundTrip("SELECT SUBSTRING(x FROM 1 FOR 3) FROM t");

    [Fact]
    public void Substring_FromOnly_ShouldRoundTrip()
        => AssertRoundTrip("SELECT SUBSTRING(x FROM 1) FROM t");

    [Fact]
    public void Position_In_ShouldRoundTrip()
        => AssertRoundTrip("SELECT POSITION(a IN b) FROM t");

    [Fact]
    public void Overlay_PlacingFromFor_ShouldRoundTrip()
        => AssertRoundTrip("SELECT OVERLAY(x PLACING y FROM 1 FOR 2) FROM t");

    [Fact]
    public void Substring_ShouldPopulateNamedParameters()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT SUBSTRING(x FROM 1 FOR 3) FROM t")!;
        Assert.NotNull(stmt.ToString());
        Assert.Contains("SUBSTRING(x FROM 1 FOR 3)", stmt.ToString()!);
    }
}


