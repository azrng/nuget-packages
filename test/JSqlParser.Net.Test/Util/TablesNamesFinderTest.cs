using JSqlParser.Net.Parser;
using JSqlParser.Net.Util;

namespace JSqlParser.Net.Test.Util;

/// <summary>
/// TablesNamesFinder 测试 — 从各种 SQL 中提取表名
/// </summary>
public class TablesNamesFinderTest
{
    private HashSet<string> GetTables(JSqlParser.Net.Statement.Statement stmt)
    {
        var finder = new TablesNamesFinder();
        return finder.GetTables(stmt);
    }

    [Fact]
    public void FindTables_SimpleSelect_ShouldReturnTable()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM users")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void FindTables_SelectWithJoin_ShouldReturnBothTables()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT u.id, o.total FROM users u INNER JOIN orders o ON u.id = o.user_id")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
    }

    [Fact]
    public void FindTables_Insert_ShouldReturnTable()
    {
        var stmt = CCJSqlParserUtil.Parse("INSERT INTO users (id, name) VALUES (1, 'test')")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void FindTables_Update_ShouldReturnTable()
    {
        var stmt = CCJSqlParserUtil.Parse("UPDATE users SET name = 'test' WHERE id = 1")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void FindTables_Delete_ShouldReturnTable()
    {
        var stmt = CCJSqlParserUtil.Parse("DELETE FROM users WHERE id = 1")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void FindTables_Subquery_ShouldReturnAllTables()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE id IN (SELECT user_id FROM orders)")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
    }

    [Fact]
    public void FindTables_Union_ShouldReturnAllTables()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM users UNION SELECT id FROM admins")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
        Assert.Contains("admins", tables);
    }

    [Fact]
    public void FindTables_MultipleTables_ShouldReturnAll()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT * FROM users, orders, products")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
        Assert.Contains("products", tables);
    }

    [Fact]
    public void FindTables_NotExistsSubquery_ShouldReturnAllTables()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE NOT EXISTS (SELECT 1 FROM orders)")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
    }

    [Fact]
    public void FindTables_FromSubquery_ShouldReturnInnerTable()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT * FROM (SELECT id FROM users) u")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void FindTables_WithSchema_ShouldReturnTableName()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM mydb.users")!;
        var tables = GetTables(stmt);
        Assert.True(tables.Count > 0);
    }

    [Fact]
    public void FindTables_CreateTable_ShouldReturnTable()
    {
        var stmt = CCJSqlParserUtil.Parse("CREATE TABLE users (id INT, name VARCHAR(100))")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void FindTables_InExpressionList_ShouldReturnTable()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE status IN (1, 2, 3)")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void FindTables_InSubquery_ShouldReturnBothTables()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE status IN (SELECT status FROM orders WHERE amount > 100)")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
    }
}
