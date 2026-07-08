using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-13 #3 通用表函数作 FromItem 测试。
/// 对齐上游 TableFunction，支持 FROM func(...) AS alias。
/// </summary>
public class TableFunctionTest
{
    [Fact]
    public void TableFunction_NoArgs_RoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM generate_series()");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM generate_series()", stmt!.ToString());
    }

    [Fact]
    public void TableFunction_WithArgs_RoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM generate_series(1, 10) AS s");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM generate_series(1, 10) AS s", stmt!.ToString());
    }

    [Fact]
    public void TableFunction_ShouldBuildCorrectNode()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM unnest(arr) AS u");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var tableFn = Assert.IsType<TableFunction>(plainSelect.FromItem);

        Assert.Equal("unnest", tableFn.Function.Name);
        Assert.NotNull(tableFn.Alias);
        Assert.Equal("u", tableFn.Alias.Name);
    }

    [Fact]
    public void TableFunction_InJoin_RoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t1 JOIN generate_series(1, 5) AS g ON 1 = 1");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t1 JOIN generate_series(1, 5) AS g ON 1 = 1", stmt!.ToString());
    }

    [Fact]
    public void TableFunction_StarArg_RoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM my_func(*)");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM my_func(*)", stmt!.ToString());
    }
}
