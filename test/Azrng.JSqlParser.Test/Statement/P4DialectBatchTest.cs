using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// T096 P4 剩余方言批量测试：TableStatement / WITH ISOLATION / FOR CLAUSE / WITH FUNCTION / EXPORT / IMPORT。
/// </summary>
public class P4DialectBatchTest
{
    // ── BL-19d TableStatement（MySQL 8.2）──

    [Fact]
    public void TableStatement_Simple_ShouldRoundTrip()
    {
        var stmt = (TableStatement)CCJSqlParserUtil.Parse("TABLE columns")!;
        Assert.NotNull(stmt.Table);
        Assert.Equal("columns", stmt.Table!.Name);
        Assert.Equal("TABLE columns", stmt.ToString());
    }

    [Fact]
    public void TableStatement_WithOrderBy_ShouldRoundTrip()
    {
        var sql = "TABLE columns ORDER BY column_name";
        var stmt = (TableStatement)CCJSqlParserUtil.Parse(sql)!;
        Assert.NotNull(stmt.OrderByElements);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void TableStatement_WithLimit_ShouldRoundTrip()
    {
        var sql = "TABLE columns LIMIT 10";
        var stmt = (TableStatement)CCJSqlParserUtil.Parse(sql)!;
        Assert.NotNull(stmt.Limit);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void TableStatement_Full_ShouldRoundTrip()
    {
        var sql = "TABLE columns ORDER BY column_name LIMIT 10 OFFSET 5";
        var stmt = (TableStatement)CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal(sql, stmt.ToString());
    }

    // ── BL-19h-2 WITH ISOLATION（DB2）──

    [Fact]
    public void WithIsolation_UR_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT * FROM mytable WITH UR")!;
        Assert.Equal("UR", select.Isolation);
    }

    [Fact]
    public void WithIsolation_CS_ShouldPreserveCase()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT * FROM mytable WITH Cs")!;
        Assert.Equal("Cs", select.Isolation);
    }

    [Fact]
    public void WithIsolation_RoundTrip_ShouldPreserve()
    {
        var sql = "SELECT * FROM mytable WITH UR";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal(sql, select.ToString());
    }

    // ── BL-19h-3 FOR CLAUSE（FOR BROWSE / FOR XML RAW|AUTO / FOR JSON）──

    [Fact]
    public void ForClause_Browse_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM table1 FOR BROWSE";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal("BROWSE", select.ForClause);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void ForClause_XmlRaw_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM table1 FOR XML RAW('something'), ROOT('trkseg')";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        Assert.NotNull(select.ForClause);
        Assert.StartsWith("XML RAW", select.ForClause!);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void ForClause_XmlAuto_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM table1 FOR XML AUTO, ROOT('trkseg')";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void ForClause_JsonPath_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM table1 FOR JSON PATH, ROOT('trkseg')";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void ForClause_XmlPath_BackwardCompat_ShouldUseForXmlPath()
    {
        // 向后兼容：FOR XML PATH 仍填充 ForXmlPath 字段
        var sql = "SELECT * FROM users FOR XML PATH('user')";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal("'user'", select.ForXmlPath);
        Assert.Null(select.ForClause);
        Assert.Equal(sql, select.ToString());
    }

    // ── BL-19h-1 WITH FUNCTION ──

    [Fact]
    public void WithFunction_WithParameters_ShouldRoundTrip()
    {
        var sql = "WITH FUNCTION func1(param1 bigint, param2 double) RETURNS integer RETURN 1 + 1 " +
                  "SELECT 1";
        var select = (Select)CCJSqlParserUtil.Parse(sql)!;
        Assert.NotNull(select.WithItemsList);
        var item = select.WithItemsList![0];
        Assert.NotNull(item.WithFunctionDeclaration);
        Assert.Equal("func1", item.WithFunctionDeclaration!.FunctionName);
        Assert.Equal(2, item.WithFunctionDeclaration.Parameters.Count);
        Assert.Equal("integer", item.WithFunctionDeclaration.ReturnType);
        Assert.Contains("FUNCTION func1(param1 bigint, param2 double) RETURNS integer RETURN 1 + 1",
            select.ToString()!);
    }

    [Fact]
    public void WithFunction_NoParameters_ShouldRoundTrip()
    {
        var sql = "WITH FUNCTION func1() RETURNS integer RETURN 1 + 1 SELECT 1";
        var select = (Select)CCJSqlParserUtil.Parse(sql)!;
        var item = select.WithItemsList![0];
        Assert.NotNull(item.WithFunctionDeclaration);
        Assert.Empty(item.WithFunctionDeclaration!.Parameters);
        Assert.Equal("integer", item.WithFunctionDeclaration.ReturnType);
    }
}
