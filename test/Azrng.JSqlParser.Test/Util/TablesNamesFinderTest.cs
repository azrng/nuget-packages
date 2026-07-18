using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Util;

namespace Azrng.JSqlParser.Test.Util;

/// <summary>
/// TablesNamesFinder 测试 — 从各种 SQL 中提取表名
/// </summary>
public class TablesNamesFinderTest
{
    private HashSet<string> GetTables(Azrng.JSqlParser.Statement.IStatement stmt)
    {
        var finder = new TablesNamesFinder();
        return finder.GetTables(stmt);
    }

    [Fact]
    public void FindTables_SimpleSelect_ShouldReturnTable()
    {
        var stmt = SqlParser.Parse("SELECT id FROM users")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void FindTables_SelectWithJoin_ShouldReturnBothTables()
    {
        var stmt = SqlParser.Parse(
            "SELECT u.id, o.total FROM users u INNER JOIN orders o ON u.id = o.user_id")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
    }

    [Fact]
    public void FindTables_Insert_ShouldReturnTable()
    {
        var stmt = SqlParser.Parse("INSERT INTO users (id, name) VALUES (1, 'test')")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
    }

    /// <summary>
    /// 回归测试（上游 commit 49958b6b）：上游 Java 版 StatementVisitorAdapter.visit(Insert)
    /// 曾对 insert.getTable() 调用两次 fromItemVisitor 导致重复访问。Azrng 版 Adapter 为空
    /// 实现、TablesNamesFinder 用 HashSet 天然去重，不存在此 bug。此用例固化该结论。
    /// </summary>
    [Fact]
    public void FindTables_Insert_ShouldVisitTableOnlyOnce()
    {
        var stmt = SqlParser.Parse(
            "INSERT INTO users (id, name) VALUES (1, 'test')")!;
        var tables = GetTables(stmt);
        Assert.Single(tables);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void FindTables_Update_ShouldReturnTable()
    {
        var stmt = SqlParser.Parse("UPDATE users SET name = 'test' WHERE id = 1")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void FindTables_Delete_ShouldReturnTable()
    {
        var stmt = SqlParser.Parse("DELETE FROM users WHERE id = 1")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void FindTables_DeleteUsing_ShouldReturnAllTables()
    {
        var stmt = SqlParser.Parse(
            "DELETE FROM users USING orders WHERE users.id = orders.uid")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
    }

    [Fact]
    public void FindTables_Subquery_ShouldReturnAllTables()
    {
        var stmt = SqlParser.Parse(
            "SELECT id FROM users WHERE id IN (SELECT user_id FROM orders)")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
    }

    [Fact]
    public void FindTables_Union_ShouldReturnAllTables()
    {
        var stmt = SqlParser.Parse(
            "SELECT id FROM users UNION SELECT id FROM admins")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
        Assert.Contains("admins", tables);
    }

    [Fact]
    public void FindTables_MultipleTables_ShouldReturnAll()
    {
        var stmt = SqlParser.Parse(
            "SELECT * FROM users, orders, products")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
        Assert.Contains("products", tables);
    }

    [Fact]
    public void FindTables_NotExistsSubquery_ShouldReturnAllTables()
    {
        var stmt = SqlParser.Parse(
            "SELECT id FROM users WHERE NOT EXISTS (SELECT 1 FROM orders)")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
    }

    [Fact]
    public void FindTables_FromSubquery_ShouldReturnInnerTable()
    {
        var stmt = SqlParser.Parse(
            "SELECT * FROM (SELECT id FROM users) u")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void FindTables_WithSchema_ShouldReturnTableName()
    {
        var stmt = SqlParser.Parse("SELECT id FROM mydb.users")!;
        var tables = GetTables(stmt);
        Assert.True(tables.Count > 0);
    }

    [Fact]
    public void FindTables_CreateTable_ShouldReturnTable()
    {
        var stmt = SqlParser.Parse("CREATE TABLE users (id INT, name VARCHAR(100))")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void FindTables_InExpressionList_ShouldReturnTable()
    {
        var stmt = SqlParser.Parse(
            "SELECT id FROM users WHERE status IN (1, 2, 3)")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void FindTables_InSubquery_ShouldReturnBothTables()
    {
        var stmt = SqlParser.Parse(
            "SELECT id FROM users WHERE status IN (SELECT status FROM orders WHERE amount > 100)")!;
        var tables = GetTables(stmt);
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
    }
}
