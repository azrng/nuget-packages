using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-13 #7 Join 多 ON 表达式测试。
/// 对齐上游 onExpressions 列表，支持 JOIN t ON a ON b。
/// </summary>
public class JoinMultiOnTest
{
    [Fact]
    public void Join_SingleOn_RoundTrip()
    {
        // 单 ON 向后兼容
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t1 JOIN t2 ON t1.id = t2.id");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t1 JOIN t2 ON t1.id = t2.id", stmt!.ToString());
    }

    [Fact]
    public void Join_SingleOn_OnExpressionCompat_ShouldWork()
    {
        // OnExpression 兼容属性仍可访问首项
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t1 JOIN t2 ON a = b");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var join = Assert.Single(plainSelect.Joins!);

        Assert.NotNull(join.OnExpression);
        Assert.Single(join.OnExpressions);
    }

    [Fact]
    public void Join_OnExpressions_ProgrammaticMultipleOn_ShouldSerialize()
    {
        // grammar 仅支持单 ON，但 OnExpressions 列表 API 支持程序化构造多 ON
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t1 JOIN t2 ON a = b");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var join = plainSelect.Joins![0];
        // 程序化追加第二个 ON
        join.OnExpressions.Add(CCJSqlParserUtil.ParseExpression("c = d")!);

        Assert.Equal(2, join.OnExpressions.Count);
        Assert.Contains("ON a = b ON c = d", stmt.ToString());
    }
}
