using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Test.DeParser;

/// <summary>
/// DeParser 测试 — AST → SQL 字符串的反序列化
/// 使用 Statement.ToString() 进行验证
/// </summary>
public class DeParserTest
{
    [Fact]
    public void Deparse_SimpleSelect_ShouldReturnSql()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id, name FROM users")!;
        var sql = stmt.ToString();
        Assert.Contains("SELECT", sql);
        Assert.Contains("users", sql);
    }

    [Fact]
    public void Deparse_SelectWithWhere_ShouldReturnSql()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM users WHERE id = 1")!;
        var sql = stmt.ToString();
        Assert.Contains("WHERE", sql);
    }

    [Fact]
    public void Deparse_SelectWithJoin_ShouldReturnSql()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT u.id, o.total FROM users u INNER JOIN orders o ON u.id = o.user_id")!;
        var sql = stmt.ToString();
        Assert.Contains("JOIN", sql);
    }

    [Fact]
    public void Deparse_Insert_ShouldReturnSql()
    {
        var stmt = CCJSqlParserUtil.Parse("INSERT INTO users (id, name) VALUES (1, 'test')")!;
        var sql = stmt.ToString();
        Assert.Contains("INSERT", sql);
        Assert.Contains("users", sql);
    }

    [Fact]
    public void Deparse_Update_ShouldReturnSql()
    {
        var stmt = CCJSqlParserUtil.Parse("UPDATE users SET name = 'test' WHERE id = 1")!;
        var sql = stmt.ToString();
        Assert.Contains("UPDATE", sql);
        Assert.Contains("SET", sql);
    }

    [Fact]
    public void Deparse_Delete_ShouldReturnSql()
    {
        var stmt = CCJSqlParserUtil.Parse("DELETE FROM users WHERE id = 1")!;
        var sql = stmt.ToString();
        Assert.Contains("DELETE", sql);
        Assert.Contains("users", sql);
    }

    [Fact]
    public void Deparse_CreateTable_ShouldReturnSql()
    {
        var stmt = CCJSqlParserUtil.Parse("CREATE TABLE users (id INT, name VARCHAR(100))")!;
        var sql = stmt.ToString();
        Assert.Contains("CREATE TABLE", sql);
    }

    [Fact]
    public void Deparse_DropTable_ShouldReturnSql()
    {
        var stmt = CCJSqlParserUtil.Parse("DROP TABLE users")!;
        var sql = stmt.ToString();
        Assert.Contains("DROP TABLE", sql);
    }

    [Fact]
    public void Deparse_Union_ShouldReturnSql()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM users UNION SELECT id FROM admins")!;
        var sql = stmt.ToString();
        Assert.Contains("UNION", sql);
    }

    [Fact]
    public void Deparse_SelectWithOrderBy_ShouldReturnSql()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM users ORDER BY id DESC")!;
        var sql = stmt.ToString();
        Assert.Contains("ORDER BY", sql);
    }

    [Fact]
    public void Deparse_SelectWithGroupBy_ShouldReturnSql()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT name, COUNT(*) FROM users GROUP BY name")!;
        var sql = stmt.ToString();
        Assert.Contains("GROUP BY", sql);
    }

    [Fact]
    public void Deparse_SelectWithLimit_ShouldReturnSql()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM users LIMIT 10")!;
        var sql = stmt.ToString();
        Assert.Contains("LIMIT", sql);
    }
}
