using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;
using Insert = Azrng.JSqlParser.Statement.Insert.Insert;
using Update = Azrng.JSqlParser.Statement.Update.Update;
using Delete = Azrng.JSqlParser.Statement.Delete.Delete;

namespace Azrng.JSqlParser.Test;

/// <summary>
/// 冒烟测试：验证 Azrng.JSqlParser 原生解析器可正常工作
/// </summary>
public class ParserSmokeTest
{
    [Fact]
    public void Parse_SimpleSelect_ShouldReturnPlainSelect()
    {
        var sql = "SELECT id, name FROM users WHERE id = 1";
        var statement = SqlParser.Parse(sql);

        Assert.NotNull(statement);
        Assert.IsType<PlainSelect>(statement);

        var select = (PlainSelect)statement;
        Assert.NotNull(select.SelectItems);
        Assert.NotNull(select.FromItem);
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Parse_NullInput_ShouldReturnNull()
    {
        var result = SqlParser.Parse((string?)null);
        Assert.Null(result);
    }

    [Fact]
    public void Parse_EmptyInput_ShouldReturnNull()
    {
        var result = SqlParser.Parse("");
        Assert.Null(result);
    }

    [Fact]
    public void Parse_SelectWithJoin_ShouldReturnStatement()
    {
        var sql = "SELECT a.id, b.name FROM users a INNER JOIN orders b ON a.id = b.user_id";
        var statement = SqlParser.Parse(sql);

        Assert.NotNull(statement);
        Assert.IsType<PlainSelect>(statement);

        var select = (PlainSelect)statement;
        Assert.NotNull(select.Joins);
        Assert.Single(select.Joins);
    }

    [Fact]
    public void Parse_SelectWithSubQuery_ShouldReturnStatement()
    {
        var sql = "SELECT * FROM (SELECT id FROM users) AS t";
        var statement = SqlParser.Parse(sql);

        Assert.NotNull(statement);
    }

    [Fact]
    public void Parse_InvalidSql_ShouldThrowException()
    {
        Assert.ThrowsAny<Exception>(() => SqlParser.Parse("INVALID SQL $$$"));
    }

    [Fact]
    public void Parse_InsertStatement_ShouldReturnInsert()
    {
        var sql = "INSERT INTO users (id, name) VALUES (1, 'test')";
        var statement = SqlParser.Parse(sql);

        Assert.NotNull(statement);
        Assert.IsType<Insert>(statement);
    }

    [Fact]
    public void Parse_UpdateStatement_ShouldReturnUpdate()
    {
        var sql = "UPDATE users SET name = 'test' WHERE id = 1";
        var statement = SqlParser.Parse(sql);

        Assert.NotNull(statement);
        Assert.IsType<Update>(statement);
    }

    [Fact]
    public void Parse_DeleteStatement_ShouldReturnDelete()
    {
        var sql = "DELETE FROM users WHERE id = 1";
        var statement = SqlParser.Parse(sql);

        Assert.NotNull(statement);
        Assert.IsType<Delete>(statement);
    }

    [Fact]
    public void Parse_SelectWithUnion_ShouldReturnSetOperationList()
    {
        var sql = "SELECT id FROM users UNION SELECT id FROM admins";
        var statement = SqlParser.Parse(sql);

        Assert.NotNull(statement);
        Assert.IsType<SetOperationList>(statement);
    }

    [Fact]
    public void Parse_SelectWithOrderBy_ShouldHaveOrderBy()
    {
        var sql = "SELECT id, name FROM users ORDER BY name ASC";
        var statement = SqlParser.Parse(sql);

        Assert.NotNull(statement);
        var select = (PlainSelect)statement;
        Assert.NotNull(select.OrderByElements);
        Assert.Single(select.OrderByElements!);
    }

    [Fact]
    public void Parse_SelectWithGroupBy_ShouldHaveGroupBy()
    {
        var sql = "SELECT department, COUNT(*) FROM users GROUP BY department";
        var statement = SqlParser.Parse(sql);

        Assert.NotNull(statement);
        var select = (PlainSelect)statement;
        Assert.NotNull(select.GroupBy);
    }

    [Fact]
    public void Parse_SelectWithLimit_ShouldHaveLimit()
    {
        var sql = "SELECT id FROM users LIMIT 10";
        var statement = SqlParser.Parse(sql);

        Assert.NotNull(statement);
        var select = (PlainSelect)statement;
        Assert.NotNull(select.Limit);
    }

    /// <summary>
    /// 旧名 CCJSqlParserUtil 已标 [Obsolete] 转发到 SqlParser，验证转发仍可用（向后兼容回归保护）。
    /// </summary>
    [Fact]
#pragma warning disable CS0618 // 旧名测试预期触发 obsolete 警告
    public void Legacy_CCJSqlParserUtil_ForwardsToSqlParser()
    {
        var statement = CCJSqlParserUtil.Parse("SELECT id FROM users");
        Assert.NotNull(statement);
        Assert.IsType<PlainSelect>(statement);

        var expr = CCJSqlParserUtil.ParseExpression("id = 1");
        Assert.NotNull(expr);

        var stmts = CCJSqlParserUtil.ParseStatements("SELECT 1; SELECT 2");
        Assert.NotNull(stmts);
        Assert.Equal(2, stmts!.StatementList.Count);

        Assert.Null(CCJSqlParserUtil.ParseNullable("INVALID SQL $$$"));
    }
#pragma warning restore CS0618
}
