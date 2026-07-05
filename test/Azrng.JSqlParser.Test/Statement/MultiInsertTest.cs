using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Insert;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// Oracle INSERT ALL/FIRST 多表插入测试。
/// 对应上游 commit 4f982e74 / issue #2394。
/// </summary>
public class MultiInsertTest
{
    [Fact]
    public void MultiInsert_All_Unconditional_ShouldParse()
    {
        // 无 WHEN 条件的 INSERT ALL（每条源行入所有目标表）
        var sql = "INSERT ALL INTO a (id) VALUES (id) INTO b (id) VALUES (id) SELECT id FROM src";
        var stmt = (MultiInsert)CCJSqlParserUtil.Parse(sql)!;
        Assert.False(stmt.IsFirst);
        Assert.Equal(2, stmt.Branches.Count);
        Assert.Null(stmt.Branches[0].WhenCondition);
        Assert.Equal("a", stmt.Branches[0].Table!.Name);
        Assert.Equal("b", stmt.Branches[1].Table!.Name);
        Assert.NotNull(stmt.Select);
    }

    [Fact]
    public void MultiInsert_All_Conditional_ShouldHaveWhen()
    {
        var sql = "INSERT ALL WHEN age > 18 THEN INTO adults (id) VALUES (id) ELSE INTO minors (id) VALUES (id) SELECT id, age FROM src";
        var stmt = (MultiInsert)CCJSqlParserUtil.Parse(sql)!;
        Assert.False(stmt.IsFirst);
        Assert.Equal(2, stmt.Branches.Count);
        Assert.NotNull(stmt.Branches[0].WhenCondition);
        Assert.Null(stmt.Branches[1].WhenCondition); // ELSE
        Assert.Equal("adults", stmt.Branches[0].Table!.Name);
        Assert.Equal("minors", stmt.Branches[1].Table!.Name);
    }

    [Fact]
    public void MultiInsert_First_ShouldSetIsFirstFlag()
    {
        var sql = "INSERT FIRST WHEN x = 1 THEN INTO t1 (a) VALUES (a) WHEN x = 2 THEN INTO t2 (a) VALUES (a) SELECT a, x FROM src";
        var stmt = (MultiInsert)CCJSqlParserUtil.Parse(sql)!;
        Assert.True(stmt.IsFirst);
        Assert.Equal(2, stmt.Branches.Count);
        Assert.NotNull(stmt.Branches[0].WhenCondition);
        Assert.NotNull(stmt.Branches[1].WhenCondition);
    }

    [Fact]
    public void MultiInsert_RoundTrip_ShouldPreserveStructure()
    {
        var sql = "INSERT ALL WHEN age > 18 THEN INTO adults (id) VALUES (id) ELSE INTO minors (id) VALUES (id) SELECT id, age FROM src";
        var stmt = (MultiInsert)CCJSqlParserUtil.Parse(sql)!;
        var output = stmt.ToString()!;
        Assert.Contains("INSERT ALL", output);
        Assert.Contains("WHEN age > 18 THEN", output);
        Assert.Contains("INTO adults (id)", output);
        Assert.Contains("ELSE INTO minors (id)", output);
        Assert.Contains("VALUES (id)", output);
    }

    [Fact]
    public void MultiInsert_First_RoundTrip_ShouldRenderFIRST()
    {
        var sql = "INSERT FIRST WHEN x = 1 THEN INTO t1 (a) VALUES (a) SELECT a, x FROM src";
        var stmt = (MultiInsert)CCJSqlParserUtil.Parse(sql)!;
        Assert.Contains("INSERT FIRST", stmt.ToString()!);
    }

    [Fact]
    public void MultiInsert_WithSubSelectBranch_ShouldParse()
    {
        // 每个 INTO 后面也可以是 SELECT 子查询而非 VALUES（Oracle 多表插入允许）
        var sql = "INSERT ALL INTO sales_hist SELECT * FROM sales WHERE region = 'US' SELECT * FROM sales";
        var stmt = (MultiInsert)CCJSqlParserUtil.Parse(sql)!;
        Assert.Single(stmt.Branches);
        Assert.Null(stmt.Branches[0].ValuesItems);
        Assert.NotNull(stmt.Branches[0].Select);
    }
}
