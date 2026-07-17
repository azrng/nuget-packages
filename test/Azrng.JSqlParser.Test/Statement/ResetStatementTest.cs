using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-12 RESET 语句回归测试（BL-12 分批迁移，本文件覆盖 RESET）。
///
/// 对齐上游 ResetStatement，形式 <c>RESET name</c>。
/// 注意：TIME ZONE 形式因 TIME 是关键字而非 identifier，暂不支持。
/// </summary>
public class ResetStatementTest
{
    [Theory]
    [InlineData("RESET TimeZone", "RESET TimeZone")]
    [InlineData("RESET TimeZone1", "RESET TimeZone1")]
    public void Reset_SingleIdentifier_RoundTrip(string input, string expected)
    {
        var stmt = SqlParser.Parse(input);

        Assert.NotNull(stmt);
        Assert.Equal(expected, stmt!.ToString());
    }

    [Fact]
    public void Reset_All_RoundTrip()
    {
        var stmt = SqlParser.Parse("RESET ALL");

        Assert.NotNull(stmt);
        Assert.Equal("RESET ALL", stmt!.ToString());
    }

    [Fact]
    public void Reset_ShouldBuildResetStatementNode()
    {
        var stmt = SqlParser.Parse("RESET TimeZone");
        var reset = Assert.IsType<ResetStatement>(stmt);

        Assert.Equal("TimeZone", reset.Name);
    }
}
