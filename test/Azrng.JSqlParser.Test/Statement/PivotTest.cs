using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-13 #1 FROM 子句 PIVOT/UNPIVOT 测试。
/// 对齐上游 Pivot/UnPivot（简化版，不含 PIVOT XML）。
/// </summary>
public class PivotTest
{
    [Fact]
    public void Pivot_RoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT * FROM sales PIVOT (SUM(amount) FOR product IN ('A', 'B'))");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM sales PIVOT (SUM(amount) FOR product IN ('A', 'B'))", stmt!.ToString());
    }

    [Fact]
    public void Pivot_WithAlias_RoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT * FROM t PIVOT (SUM(x) FOR c IN (1, 2)) AS p");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t PIVOT (SUM(x) FOR c IN (1, 2)) AS p", stmt!.ToString());
    }

    [Fact]
    public void Pivot_ShouldBuildCorrectNode()
    {
        var stmt = SqlParser.Parse("SELECT * FROM sales PIVOT (SUM(amount) FOR product IN ('A', 'B'))");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var table = Assert.IsType<Table>(plainSelect.FromItem);

        Assert.NotNull(table.Pivot);
        Assert.Equal("SUM", table.Pivot.Function.Name);
        Assert.Single(table.Pivot.ForColumns);
        Assert.Equal(2, table.Pivot.InItems.Count);
    }

    [Fact]
    public void UnPivot_RoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT * FROM t UNPIVOT (val FOR col IN (c1, c2))");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t UNPIVOT (val FOR col IN (c1, c2))", stmt!.ToString());
    }

    [Fact]
    public void UnPivot_IncludeNulls_RoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT * FROM t UNPIVOT INCLUDE NULLS (val FOR col IN (c1, c2))");

        Assert.NotNull(stmt);
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var table = Assert.IsType<Table>(plainSelect.FromItem);

        Assert.NotNull(table.UnPivot);
        Assert.True(table.UnPivot.IncludeNulls);
    }

    [Fact]
    public void UnPivot_ShouldBuildCorrectNode()
    {
        var stmt = SqlParser.Parse("SELECT * FROM t UNPIVOT (val FOR col IN (c1, c2))");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var table = Assert.IsType<Table>(plainSelect.FromItem);

        Assert.NotNull(table.UnPivot);
        Assert.Single(table.UnPivot.UnpivotClause);
        Assert.Single(table.UnPivot.UnpivotForClause);
        Assert.Equal(2, table.UnPivot.UnpivotInClause.Count);
    }

    /// <summary>
    /// Oracle PIVOT XML 变体应正确解析并保 round-trip（BL-19e）。
    /// </summary>
    [Fact]
    public void PivotXml_ShouldParseAndRoundTrip()
    {
        var sql = "SELECT * FROM sales PIVOT XML (SUM(amount) FOR product IN ('A', 'B'))";
        var stmt = SqlParser.Parse(sql);
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var table = Assert.IsType<Table>(plainSelect.FromItem);

        Assert.NotNull(table.Pivot);
        Assert.True(table.Pivot!.IsXml);
        Assert.Equal(sql, stmt!.ToString());
    }
}
