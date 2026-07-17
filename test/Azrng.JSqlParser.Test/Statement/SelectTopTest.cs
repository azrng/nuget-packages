using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-09 SELECT TOP 量词回归测试。
///
/// 此前 grammar 已接受 topClause（g4:81,110），但 VisitPlainSelect 不读 context.topClause()、
/// PlainSelect 无 Top 字段，导致 TOP n 被静默丢弃。README 第 164 行因此曾误声明已支持。
/// 本测试覆盖 TOP n / TOP (n) / TOP n PERCENT / TOP n WITH TIES 及 AST 断言。
/// </summary>
public class SelectTopTest
{
    #region round-trip

    [Theory]
    [InlineData("SELECT TOP 5 * FROM t", "SELECT TOP 5 * FROM t")]
    [InlineData("SELECT TOP 1 id, name FROM users", "SELECT TOP 1 id, name FROM users")]
    public void Top_LongValue_RoundTrip(string input, string expected)
    {
        var stmt = SqlParser.Parse(input);

        Assert.NotNull(stmt);
        Assert.Equal(expected, stmt!.ToString());
    }

    [Fact]
    public void Top_ParenthesizedExpression_RoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT TOP (10) * FROM t");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT TOP (10) * FROM t", stmt!.ToString());
    }

    [Fact]
    public void Top_Percent_RoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT TOP 10 PERCENT * FROM t");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT TOP 10 PERCENT * FROM t", stmt!.ToString());
    }

    [Fact]
    public void Top_WithTies_RoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT TOP 10 WITH TIES * FROM t");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT TOP 10 WITH TIES * FROM t", stmt!.ToString());
    }

    [Fact]
    public void Top_PercentWithTies_RoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT TOP (10) PERCENT WITH TIES * FROM t");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT TOP (10) PERCENT WITH TIES * FROM t", stmt!.ToString());
    }

    #endregion

    #region AST 断言

    [Fact]
    public void Top_LongValue_ShouldBuildTopNode()
    {
        var stmt = SqlParser.Parse("SELECT TOP 5 * FROM t");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);

        Assert.NotNull(plainSelect.Top);
        Assert.False(plainSelect.Top.HasParenthesis);
        Assert.False(plainSelect.Top.IsPercentage);
        Assert.False(plainSelect.Top.IsWithTies);
        Assert.IsType<LongValue>(plainSelect.Top.Expression);
        Assert.Equal(5L, ((LongValue)plainSelect.Top.Expression).Value);
    }

    [Fact]
    public void Top_ParenthesizedPercentWithTies_ShouldSetAllFlags()
    {
        var stmt = SqlParser.Parse("SELECT TOP (10) PERCENT WITH TIES * FROM t");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);

        Assert.NotNull(plainSelect.Top);
        Assert.True(plainSelect.Top.HasParenthesis);
        Assert.True(plainSelect.Top.IsPercentage);
        Assert.True(plainSelect.Top.IsWithTies);
    }

    [Fact]
    public void Top_Absent_ShouldLeaveTopNull()
    {
        // 无 TOP 时 Top 字段应为 null，不影响原有 SELECT 行为
        var stmt = SqlParser.Parse("SELECT * FROM t");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);

        Assert.Null(plainSelect.Top);
    }

    #endregion
}
