using JSqlParser.Net.Parser;
using JSqlParser.Net.Statement.Select;
using JSqlParser.Net.Util;

namespace JSqlParser.Net.Test.Integration;

/// <summary>
/// 集成测试 — 验证 JSqlParser.Net 原生解析器行为正确
/// 覆盖 SqlParser.Core 中使用的各种 SQL 模式
/// </summary>
public class IntegrationTest
{
    #region SqlParser.Core 核心场景

    [Fact]
    public void SqlParserCore_SimpleSelect_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id, name FROM users WHERE status = 'active'")!;
        var select = (PlainSelect)stmt;
        Assert.NotNull(select.FromItem);
        Assert.NotNull(select.Where);
        Assert.Equal(2, select.SelectItems!.Count);
    }

    [Fact]
    public void SqlParserCore_SelectWithJoin_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT u.id, o.total FROM users u INNER JOIN orders o ON u.id = o.user_id")!;
        var select = (PlainSelect)stmt;
        Assert.NotNull(select.Joins);
        Assert.Single(select.Joins);
    }

    [Fact]
    public void SqlParserCore_UnionQuery_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM users UNION SELECT id FROM admins")!;
        Assert.IsType<SetOperationList>(stmt);
        var setOpList = (SetOperationList)stmt;
        Assert.Equal(2, setOpList.Selects.Count);
    }

    [Fact]
    public void SqlParserCore_CteQuery_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "WITH active_users AS (SELECT id, name FROM users WHERE status = 'active') SELECT * FROM active_users")!;
        var select = (Select)stmt;
        Assert.NotNull(select.WithItemsList);
        Assert.Single(select.WithItemsList!);
    }

    [Fact]
    public void SqlParserCore_SubqueryInWhere_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE id IN (SELECT user_id FROM orders WHERE amount > 100)")!;
        var select = (PlainSelect)stmt;
        Assert.NotNull(select.Where);
    }

    #endregion

    #region 表达式类型覆盖

    [Fact]
    public void Expression_AndOr_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE status = 'active' AND age > 18 OR role = 'admin'")!;
        var select = (PlainSelect)stmt;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Expression_In_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE status IN ('active', 'pending', 'inactive')")!;
        var select = (PlainSelect)stmt;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Expression_Like_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE name LIKE '%test%'")!;
        var select = (PlainSelect)stmt;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Expression_Between_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE created_at BETWEEN '2024-01-01' AND '2024-12-31'")!;
        var select = (PlainSelect)stmt;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Expression_IsNull_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE email IS NULL")!;
        var select = (PlainSelect)stmt;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Expression_Exists_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE EXISTS (SELECT 1 FROM orders WHERE orders.user_id = users.id)")!;
        var select = (PlainSelect)stmt;
        Assert.NotNull(select.Where);
    }

    #endregion

    #region DML 语句

    [Fact]
    public void Dml_Insert_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("INSERT INTO users (id, name) VALUES (1, 'test')");
        Assert.NotNull(stmt);
    }

    [Fact]
    public void Dml_Update_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("UPDATE users SET name = 'test' WHERE id = 1");
        Assert.NotNull(stmt);
    }

    [Fact]
    public void Dml_Delete_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("DELETE FROM users WHERE id = 1");
        Assert.NotNull(stmt);
    }

    #endregion

    #region DDL 语句

    [Fact]
    public void Ddl_CreateTable_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "CREATE TABLE users (id INT PRIMARY KEY, name VARCHAR(100) NOT NULL, email VARCHAR(200))");
        Assert.NotNull(stmt);
    }

    [Fact]
    public void Ddl_CreateView_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "CREATE VIEW active_users AS SELECT id, name FROM users WHERE status = 'active'");
        Assert.NotNull(stmt);
    }

    [Fact]
    public void Ddl_AlterTable_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("ALTER TABLE users ADD COLUMN age INT");
        Assert.NotNull(stmt);
    }

    [Fact]
    public void Ddl_DropTable_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("DROP TABLE users");
        Assert.NotNull(stmt);
    }

    #endregion

    #region 复杂查询

    [Fact]
    public void Complex_MultipleJoins_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT u.id, o.total, p.name FROM users u " +
            "INNER JOIN orders o ON u.id = o.user_id " +
            "INNER JOIN products p ON o.product_id = p.id " +
            "WHERE u.status = 'active' " +
            "ORDER BY o.created_at DESC " +
            "LIMIT 10")!;
        var select = (PlainSelect)stmt;
        Assert.NotNull(select.Joins);
        Assert.Equal(2, select.Joins.Count);
        Assert.NotNull(select.Where);
        Assert.NotNull(select.OrderByElements);
        Assert.NotNull(select.Limit);
    }

    [Fact]
    public void Complex_GroupByHaving_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT user_id, COUNT(*) as order_count, SUM(amount) as total " +
            "FROM orders " +
            "WHERE status = 'completed' " +
            "GROUP BY user_id " +
            "HAVING COUNT(*) > 5 " +
            "ORDER BY total DESC")!;
        var select = (PlainSelect)stmt;
        Assert.NotNull(select.GroupBy);
        Assert.NotNull(select.Having);
        Assert.NotNull(select.OrderByElements);
    }

    [Fact]
    public void Complex_SubqueryInFrom_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT sub.id, sub.name FROM (SELECT id, name FROM users WHERE status = 'active') AS sub")!;
        var select = (PlainSelect)stmt;
        Assert.NotNull(select.FromItem);
    }

    [Fact]
    public void Complex_MultipleCtes_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "WITH " +
            "active_users AS (SELECT id, name FROM users WHERE status = 'active'), " +
            "recent_orders AS (SELECT user_id, amount FROM orders WHERE created_at > '2024-01-01') " +
            "SELECT u.id, u.name, o.amount " +
            "FROM active_users u " +
            "INNER JOIN recent_orders o ON u.id = o.user_id")!;
        var select = (Select)stmt;
        Assert.NotNull(select.WithItemsList);
        Assert.Equal(2, select.WithItemsList!.Count);
    }

    #endregion

    #region 表名提取

    [Fact]
    public void TableNames_SimpleSelect_ShouldReturnTable()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM users")!;
        var finder = new TablesNamesFinder();
        var tables = finder.GetTables(stmt);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void TableNames_Join_ShouldReturnBothTables()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT u.id, o.total FROM users u INNER JOIN orders o ON u.id = o.user_id")!;
        var finder = new TablesNamesFinder();
        var tables = finder.GetTables(stmt);
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
    }

    [Fact]
    public void TableNames_Subquery_ShouldReturnAllTables()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE id IN (SELECT user_id FROM orders)")!;
        var finder = new TablesNamesFinder();
        var tables = finder.GetTables(stmt);
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
    }

    #endregion

    #region AST → SQL 反序列化

    [Fact]
    public void Deparse_Select_ShouldReturnSql()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id, name FROM users WHERE status = 'active'")!;
        var sql = stmt.ToString();
        Assert.Contains("SELECT", sql);
        Assert.Contains("users", sql);
        Assert.Contains("WHERE", sql);
    }

    [Fact]
    public void Deparse_Insert_ShouldReturnSql()
    {
        var stmt = CCJSqlParserUtil.Parse("INSERT INTO users (id, name) VALUES (1, 'test')")!;
        var sql = stmt.ToString();
        Assert.Contains("INSERT", sql);
        Assert.Contains("users", sql);
    }

    #endregion
}
