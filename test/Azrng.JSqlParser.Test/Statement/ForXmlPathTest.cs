using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// SQL Server FOR XML PATH 测试。
/// </summary>
public class ForXmlPathTest
{
    [Fact]
    public void ForXmlPath_WithName_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM users FOR XML PATH('user')";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal("'user'", select.ForXmlPath);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void ForXmlPath_WithoutName_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM users FOR XML PATH";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal("", select.ForXmlPath);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void ForXmlPath_WithColumns_ShouldRoundTrip()
    {
        var sql = "SELECT id, name FROM users ORDER BY id FOR XML PATH('row')";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal("'row'", select.ForXmlPath);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void ForXmlPath_InSubquery_ShouldParse()
    {
        // 上游 commit 9de70747 修复的场景：子查询中的 FOR XML PATH
        var sql = "SELECT * FROM (SELECT id FROM t FOR XML PATH('item')) sub";
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
    }

    [Fact]
    public void ForXmlPath_NotSpecified_ShouldBeNull()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT * FROM users")!;
        Assert.Null(select.ForXmlPath);
    }
}
