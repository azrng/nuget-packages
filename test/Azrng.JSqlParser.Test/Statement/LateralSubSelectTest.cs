using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-10 LATERAL 子查询回归测试。
///
/// 此前 grammar g4:151 已接受 LATERAL subSelect，但 VisitTableOrSubquery 走 subSelect 分支，
/// AST 退化为普通 ParenthesedSelect，LATERAL 标志被静默丢弃（round-trip 丢前缀）。
/// 本测试覆盖解析 → AST 类型断言 → 序列化完整链路。
/// </summary>
public class LateralSubSelectTest
{
    [Fact]
    public void Lateral_RoundTrip_ShouldPreservePrefix()
    {
        // 修复前：LATERAL 前缀被丢弃，输出 "FROM (SELECT 1) t"
        var stmt = SqlParser.Parse("SELECT * FROM LATERAL (SELECT 1) t");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM LATERAL (SELECT 1) t", stmt!.ToString());
    }

    [Fact]
    public void Lateral_ShouldBuildLateralSubSelectNode()
    {
        // AST 类型断言：IFromItem 应为 LateralSubSelect 而非 ParenthesedSelect
        var stmt = SqlParser.Parse("SELECT * FROM LATERAL (SELECT 1) t");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);

        Assert.IsType<LateralSubSelect>(plainSelect.IFromItem);
    }

    [Fact]
    public void Lateral_NoAlias_ShouldRoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT * FROM LATERAL (SELECT 1)");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM LATERAL (SELECT 1)", stmt!.ToString());
    }

    [Fact]
    public void Lateral_WithJoin_ShouldPreservePrefix()
    {
        // LATERAL 作为 JOIN 目标
        var stmt = SqlParser.Parse("SELECT * FROM t1 JOIN LATERAL (SELECT 1) t2 ON 1 = 1");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t1 JOIN LATERAL (SELECT 1) t2 ON 1 = 1", stmt!.ToString());
    }

    [Fact]
    public void Lateral_PrefixDefaultsToLateral()
    {
        var stmt = SqlParser.Parse("SELECT * FROM LATERAL (SELECT 1) t");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var lateral = Assert.IsType<LateralSubSelect>(plainSelect.IFromItem);

        Assert.Equal("LATERAL", lateral.Prefix);
    }

    [Fact]
    public void NonLateralSubSelect_ShouldRemainParenthesedSelect()
    {
        // 普通 FROM (subquery) 不应被误判为 LATERAL
        var stmt = SqlParser.Parse("SELECT * FROM (SELECT 1) t");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);

        Assert.IsType<ParenthesedSelect>(plainSelect.IFromItem);
        Assert.IsNotType<LateralSubSelect>(plainSelect.IFromItem);
    }
}
