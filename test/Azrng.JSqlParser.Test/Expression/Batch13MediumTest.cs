using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// BL-13 中等组测试：SIMILAR TO + STRAIGHT_JOIN。
/// </summary>
public class Batch13MediumTest
{
    #region SIMILAR TO

    [Fact]
    public void SimilarTo_RoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t WHERE name SIMILAR TO 'A%'");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t WHERE name SIMILAR TO 'A%'", stmt!.ToString());
    }

    [Fact]
    public void SimilarTo_Not_RoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t WHERE name NOT SIMILAR TO 'A%'");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t WHERE name NOT SIMILAR TO 'A%'", stmt!.ToString());
    }

    [Fact]
    public void SimilarTo_ShouldBuildCorrectNode()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t WHERE name SIMILAR TO 'A%'");
        Assert.NotNull(stmt);
        // 验证 round-trip 含 SIMILAR TO
        Assert.Contains("SIMILAR TO", stmt!.ToString());
    }

    #endregion

    #region STRAIGHT_JOIN

    [Fact]
    public void StraightJoin_RoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t1 STRAIGHT_JOIN t2 ON t1.id = t2.id");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t1 STRAIGHT_JOIN t2 ON t1.id = t2.id", stmt!.ToString());
    }

    [Fact]
    public void StraightJoin_ShouldSetStraightFlag()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t1 STRAIGHT_JOIN t2 ON t1.id = t2.id");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var join = Assert.Single(plainSelect.Joins!);

        Assert.True(join.Straight);
        Assert.False(join.Inner);
        Assert.False(join.Left);
    }

    [Fact]
    public void StraightJoin_MultipleTables_RoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t1 STRAIGHT_JOIN t2 ON t1.id = t2.id STRAIGHT_JOIN t3 ON t2.id = t3.id");

        Assert.NotNull(stmt);
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        Assert.Equal(2, plainSelect.Joins!.Count);
        Assert.True(plainSelect.Joins[0].Straight);
        Assert.True(plainSelect.Joins[1].Straight);
    }

    #endregion
}
