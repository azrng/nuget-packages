using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// BL-07 OVERLAPS / BL-08 MEMBER OF 回归测试。
///
/// 此前 VisitPredicateSuffix 未接线 MEMBER OF / OVERLAPS 分支，grammar 接受但 AST 静默丢弃谓词语义，
/// round-trip 会丢数据。本测试覆盖解析 → AST 构建 → 序列化的完整链路。
/// </summary>
public class OverlapsAndMemberOfTest
{
    #region BL-08 MEMBER OF

    [Fact]
    public void MemberOf_RoundTrip_ShouldPreserveSemantics()
    {
        // 修复前：MEMBER OF 被 VisitPredicateSuffix 静默丢弃，WHERE 仅剩左侧 1
        // 右侧括号是 Parenthesis 表达式的一部分，round-trip 会保留
        var stmt = SqlParser.Parse("SELECT * FROM t WHERE 1 MEMBER OF ('[1,2,3]')");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t WHERE 1 MEMBER OF ('[1,2,3]')", stmt!.ToString());
    }

    [Fact]
    public void MemberOf_Not_RoundTrip_ShouldPreserveNot()
    {
        // NOT MEMBER OF 必须保留 NOT 语义
        var stmt = SqlParser.Parse("SELECT * FROM t WHERE 1 NOT MEMBER OF ('[1,2,3]')");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t WHERE 1 NOT MEMBER OF ('[1,2,3]')", stmt!.ToString());
    }

    [Fact]
    public void MemberOf_ShouldBuildMemberOfExpressionNode()
    {
        // AST 类型断言：确保生成 MemberOfExpression 而非被丢弃后退化成左侧表达式
        var stmt = SqlParser.Parse("SELECT * FROM t WHERE 1 MEMBER OF ('[1,2,3]')");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var where = Assert.IsType<MemberOfExpression>(plainSelect.Where);

        Assert.False(where.Not);
        Assert.Equal("1", where.LeftExpression.ToString());
    }

    [Fact]
    public void MemberOf_Not_ShouldSetNotFlag()
    {
        var stmt = SqlParser.Parse("SELECT * FROM t WHERE x NOT MEMBER OF arr");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var where = Assert.IsType<MemberOfExpression>(plainSelect.Where);

        Assert.True(where.Not);
    }

    #endregion

    #region BL-07 OVERLAPS

    [Fact]
    public void Overlaps_RoundTrip_ShouldPreserveSemantics()
    {
        // 修复前：OVERLAPS 被 VisitPredicateSuffix 静默丢弃，WHERE 仅剩左侧 a
        var stmt = SqlParser.Parse("SELECT * FROM t WHERE a OVERLAPS b");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t WHERE a OVERLAPS b", stmt!.ToString());
    }

    [Fact]
    public void Overlaps_ShouldBuildOverlapsConditionNode()
    {
        // AST 类型断言：确保生成 OverlapsCondition 而非被丢弃
        var stmt = SqlParser.Parse("SELECT * FROM t WHERE a OVERLAPS b");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var where = Assert.IsType<OverlapsCondition>(plainSelect.Where);

        Assert.NotNull(where.LeftExpression);
        Assert.NotNull(where.RightExpression);
    }

    [Fact]
    public void Overlaps_InComplexWhere_ShouldPreserve()
    {
        // 嵌入复合 WHERE 子句，验证 OVERLAPS 不被 AND 链路吞掉
        var stmt = SqlParser.Parse("SELECT * FROM t WHERE active = 1 AND a OVERLAPS b");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t WHERE active = 1 AND a OVERLAPS b", stmt!.ToString());
    }

    #endregion
}
