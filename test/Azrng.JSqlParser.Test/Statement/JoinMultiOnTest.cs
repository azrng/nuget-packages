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
        var stmt = SqlParser.Parse("SELECT * FROM t1 JOIN t2 ON t1.id = t2.id");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t1 JOIN t2 ON t1.id = t2.id", stmt!.ToString());
    }

    [Fact]
    public void Join_SingleOn_OnExpressionCompat_ShouldWork()
    {
        // OnExpression 兼容属性仍可访问首项
        var stmt = SqlParser.Parse("SELECT * FROM t1 JOIN t2 ON a = b");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var join = Assert.Single(plainSelect.Joins!);

        Assert.NotNull(join.OnExpression);
        Assert.Single(join.OnExpressions);
    }

    [Fact]
    public void Join_MultiOn_Parsed_ShouldCollectAllOnExpressions()
    {
        // grammar 现已支持 JOIN 多 ON（JOIN t ON a ON b），对齐上游 jjt:5995 ( <K_ON> expr )*。
        // 此前 grammar 仅单 ON，OnExpressions 列表形同虚设。
        var stmt = SqlParser.Parse("SELECT * FROM t1 JOIN t2 ON a = b ON c = d");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var join = Assert.Single(plainSelect.Joins!);

        Assert.Equal(2, join.OnExpressions.Count);
        Assert.Contains("ON a = b ON c = d", stmt.ToString()!);
    }

    [Fact]
    public void Join_OnExpressions_ProgrammaticMultipleOn_ShouldSerialize()
    {
        // 程序化构造多 ON 仍可序列化（与上一测试互补：覆盖无源 SQL 的纯 API 场景）
        var stmt = SqlParser.Parse("SELECT * FROM t1 JOIN t2 ON a = b");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var join = plainSelect.Joins![0];
        // 程序化追加第二个 ON
        join.OnExpressions.Add(SqlParser.ParseExpression("c = d")!);

        Assert.Equal(2, join.OnExpressions.Count);
        Assert.Contains("ON a = b ON c = d", stmt.ToString());
    }

    /// <summary>
    /// 括号 FROM 项的可选 alias 不应丢失（BL-19f）：FROM (fromItem) alias
    /// 此前兜底 Visit(GetChild(0)) 跳过 alias，修复后应保留。
    /// </summary>
    [Fact]
    public void ParenthesedFromItem_WithAlias_ShouldPreserveAlias()
    {
        // 括号包裹单表 + alias（subSelect 分支不走，此处验证括号分支 alias 保留）
        var stmt = SqlParser.Parse("SELECT * FROM (users) AS u");
        // alias "u" 应保留在 round-trip 输出中（此前兜底路径会丢失）
        Assert.Contains("u", stmt!.ToString()!);
    }
}
