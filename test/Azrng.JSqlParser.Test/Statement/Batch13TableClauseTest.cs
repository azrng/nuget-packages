using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-13 #2 TABLESAMPLE + #9 时间旅行 测试。
/// </summary>
public class Batch13TableClauseTest
{
    #region TABLESAMPLE

    [Fact]
    public void TableSample_RoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t TABLESAMPLE (100)");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t TABLESAMPLE (100)", stmt!.ToString());
    }

    [Fact]
    public void TableSample_WithMethod_RoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t TABLESAMPLE BERNOULLI (50) PERCENT");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t TABLESAMPLE BERNOULLI (50) PERCENT", stmt!.ToString());
    }

    [Fact]
    public void TableSample_ShouldBuildCorrectNode()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t TABLESAMPLE SYSTEM (1000)");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var table = Assert.IsType<Table>(plainSelect.IFromItem);

        Assert.NotNull(table.TableSample);
        Assert.Equal("SYSTEM", table.TableSample.SamplingMethod);
        Assert.False(table.TableSample.Percentage);
    }

    #endregion
}
