using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// 括号化 INSERT（CTE 形式 WITH x AS (INSERT ...)）测试。
/// 对应上游 ParenthesedInsert 设计（继承自 Insert）。
/// </summary>
public class ParenthesedInsertTest
{
    [Fact]
    public void Cte_WithInsert_ShouldParseAsParenthesedInsert()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "WITH t AS (INSERT INTO users (id) VALUES (1)) SELECT * FROM t")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void ParenthesedInsert_ShouldBeAssignableToInsert()
    {
        // 重构后 ParenthesedInsert 继承 Insert，可作为 Insert 使用
        var stmt = CCJSqlParserUtil.Parse(
            "WITH t AS (INSERT INTO users (id) VALUES (1)) SELECT * FROM t")!;
        // stmt 实际是 PlainSelect（继承 Select），WithItemsList 在 Select 基类上
        var select = (Select)stmt;
        Assert.NotNull(select.WithItemsList);
        var withItem = select.WithItemsList![0];
        Assert.NotNull(withItem.ParenthesedInsert);
        var pi = withItem.ParenthesedInsert!;
        // 验证继承关系：可以赋值给 Insert 类型的变量
        Azrng.JSqlParser.Statement.Insert.Insert asInsert = pi;
        Assert.NotNull(asInsert);
    }

    [Fact]
    public void ParenthesedInsert_ToString_ShouldRenderWithParens()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "WITH t AS (INSERT INTO users (id) VALUES (1)) SELECT * FROM t")!;
        var output = stmt.ToString()!;
        Assert.Contains("(INSERT INTO users (id) VALUES (1))", output);
    }

    [Fact]
    public void Cte_WithInsertReturning_ShouldRoundTrip()
    {
        var sql = "WITH t AS (INSERT INTO users (id) VALUES (1) RETURNING id) SELECT * FROM t";
        var stmt = CCJSqlParserUtil.Parse(sql)!;
        Assert.NotNull(stmt);
        Assert.Contains("RETURNING", stmt.ToString()!);
    }
}
