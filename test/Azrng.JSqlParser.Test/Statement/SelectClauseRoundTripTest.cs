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
}
