using Antlr4.Runtime;
using Azrng.JSqlParser.Parser.ANTLR4;
using Xunit.Abstractions;

namespace Azrng.JSqlParser.Test.Parser.ANTLR4;

/// <summary>
/// ANTLR4 解析器基础测试
/// M045: 验证 ANTLR4 语法能正确解析 SQL
/// </summary>
public class Antlr4ParserTest
{
    private readonly ITestOutputHelper _output;

    public Antlr4ParserTest(ITestOutputHelper output)
    {
        _output = output;
    }

    private static (JSqlParserGrammar Parser, CollectingErrorListener ErrorListener) ParseSql(string sql)
    {
        var inputStream = new AntlrInputStream(sql);
        var lexer = new JSqlParserGrammarLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new JSqlParserGrammar(tokenStream);
        var errorListener = new CollectingErrorListener();
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorListener);
        return (parser, errorListener);
    }

    [Fact]
    public void SimpleSelect_ShouldParse()
    {
        var (parser, errors) = ParseSql("SELECT id, name FROM users WHERE status = 'active'");
        var tree = parser.statements();
        Assert.NotNull(tree);
        Assert.Empty(errors.Errors);
    }

    [Fact]
    public void SelectWithJoin_ShouldParse()
    {
        var (parser, errors) = ParseSql(
            "SELECT u.id, o.total FROM users u INNER JOIN orders o ON u.id = o.user_id");
        var tree = parser.statements();
        Assert.NotNull(tree);
        Assert.Empty(errors.Errors);
    }

    [Fact]
    public void SelectWithSubquery_ShouldParse()
    {
        var (parser, errors) = ParseSql(
            "SELECT id FROM users WHERE id IN (SELECT user_id FROM orders WHERE amount > 100)");
        var tree = parser.statements();
        Assert.NotNull(tree);
        Assert.Empty(errors.Errors);
    }

    [Fact]
    public void SelectWithCte_ShouldParse()
    {
        var (parser, errors) = ParseSql(
            "WITH active_users AS (SELECT id, name FROM users WHERE status = 'active') SELECT * FROM active_users");
        var tree = parser.statements();
        Assert.NotNull(tree);
        Assert.Empty(errors.Errors);
    }

    [Fact]
    public void UnionQuery_ShouldParse()
    {
        var (parser, errors) = ParseSql("SELECT id FROM users UNION SELECT id FROM admins");
        var tree = parser.statements();
        Assert.NotNull(tree);
        Assert.Empty(errors.Errors);
    }

    [Fact]
    public void InsertStatement_ShouldParse()
    {
        var (parser, errors) = ParseSql("INSERT INTO users (id, name) VALUES (1, 'test')");
        var tree = parser.statements();
        Assert.NotNull(tree);
        Assert.Empty(errors.Errors);
    }

    [Fact]
    public void UpdateStatement_ShouldParse()
    {
        var (parser, errors) = ParseSql("UPDATE users SET name = 'test' WHERE id = 1");
        var tree = parser.statements();
        Assert.NotNull(tree);
        Assert.Empty(errors.Errors);
    }

    [Fact]
    public void DeleteStatement_ShouldParse()
    {
        var (parser, errors) = ParseSql("DELETE FROM users WHERE id = 1");
        var tree = parser.statements();
        Assert.NotNull(tree);
        Assert.Empty(errors.Errors);
    }

    [Fact]
    public void CreateTable_ShouldParse()
    {
        var (parser, errors) = ParseSql(
            "CREATE TABLE users (id INT PRIMARY KEY, name VARCHAR(100) NOT NULL, email VARCHAR(200))");
        var tree = parser.statements();
        Assert.NotNull(tree);
        Assert.Empty(errors.Errors);
    }

    [Fact]
    public void AlterTable_ShouldParse()
    {
        var (parser, errors) = ParseSql("ALTER TABLE users ADD COLUMN age INT");
        var tree = parser.statements();
        Assert.NotNull(tree);
        Assert.Empty(errors.Errors);
    }

    [Fact]
    public void DropTable_ShouldParse()
    {
        var (parser, errors) = ParseSql("DROP TABLE users");
        var tree = parser.statements();
        Assert.NotNull(tree);
        Assert.Empty(errors.Errors);
    }

    [Fact]
    public void ComplexQuery_ShouldParse()
    {
        var (parser, errors) = ParseSql(
            "SELECT u.id, u.name, COUNT(o.id) as order_count, SUM(o.amount) as total " +
            "FROM users u LEFT JOIN orders o ON u.id = o.user_id " +
            "WHERE u.status = 'active' AND u.created_at > '2024-01-01' " +
            "GROUP BY u.id, u.name HAVING COUNT(o.id) > 5 ORDER BY total DESC LIMIT 100");
        var tree = parser.statements();
        Assert.NotNull(tree);
        if (errors.Errors.Count > 0)
        {
            foreach (var err in errors.Errors)
                _output.WriteLine($"Syntax error: {err}");
        }
        Assert.Empty(errors.Errors);
    }

    [Fact]
    public void CaseExpression_ShouldParse()
    {
        var (parser, errors) = ParseSql(
            "SELECT CASE WHEN status = 'active' THEN 'Active' ELSE 'Inactive' END as status_text FROM users");
        var tree = parser.statements();
        Assert.NotNull(tree);
        Assert.Empty(errors.Errors);
    }

    [Fact]
    public void CastExpression_ShouldParse()
    {
        var (parser, errors) = ParseSql("SELECT CAST(id AS VARCHAR) FROM users");
        var tree = parser.statements();
        Assert.NotNull(tree);
        Assert.Empty(errors.Errors);
    }

    [Fact]
    public void MultipleStatements_ShouldParse()
    {
        var (parser, errors) = ParseSql("SELECT 1; SELECT 2; SELECT 3;");
        var tree = parser.statements();
        Assert.NotNull(tree);
        Assert.Empty(errors.Errors);
    }

    [Fact]
    public void CaseInsensitive_ShouldParse()
    {
        var (parser, errors) = ParseSql("select id, Name from USERS where STATUS = 'active'");
        var tree = parser.statements();
        Assert.NotNull(tree);
        Assert.Empty(errors.Errors);
    }
}
