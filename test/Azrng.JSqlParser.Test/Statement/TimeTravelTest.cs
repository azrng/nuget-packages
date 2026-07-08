using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Xunit;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-13：Snowflake 时间旅行（AT/BEFORE）接线测试。
/// grammar/visitor 将 AT (TIMESTAMP|OFFSET|STATEMENT =&gt; expr) / BEFORE (STATEMENT =&gt; expr) 解析到 Table.TimeTravel。
/// </summary>
public class TimeTravelTest
{
    public static TheoryData<string> RoundTripCases => new()
    {
        "SELECT * FROM t AT (TIMESTAMP => '2024-01-01')",
        "SELECT * FROM t AT (OFFSET => '-1h')",
        "SELECT * FROM t AT (STATEMENT => 'stmt-id')",
        "SELECT * FROM t BEFORE (STATEMENT => 'stmt-id')",
    };

    [Theory]
    [MemberData(nameof(RoundTripCases))]
    public void ShouldRoundTrip(string sql)
    {
        var stmt = CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void AtTimestamp_ParsesTimeTravelClause()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t AT (TIMESTAMP => '2024-01-01')")!;
        var table = ExtractFromItem(stmt);
        Assert.NotNull(table.TimeTravel);
        Assert.False(table.TimeTravel!.IsBefore);
        Assert.Equal("TIMESTAMP", table.TimeTravel.TravelType);
    }

    [Fact]
    public void BeforeStatement_ParsesIsBefore()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t BEFORE (STATEMENT => 'stmt-id')")!;
        var table = ExtractFromItem(stmt);
        Assert.NotNull(table.TimeTravel);
        Assert.True(table.TimeTravel!.IsBefore);
        Assert.Equal("STATEMENT", table.TimeTravel.TravelType);
    }

    [Fact]
    public void AtOffset_ParsesOffsetType()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t AT (OFFSET => '-1h')")!;
        var table = ExtractFromItem(stmt);
        Assert.NotNull(table.TimeTravel);
        Assert.Equal("OFFSET", table.TimeTravel!.TravelType);
    }

    private static Table ExtractFromItem(Azrng.JSqlParser.Statement.Statement stmt)
    {
        var plain = Assert.IsType<Azrng.JSqlParser.Statement.Select.PlainSelect>(stmt);
        return Assert.IsType<Table>(plain.FromItem);
    }
}
