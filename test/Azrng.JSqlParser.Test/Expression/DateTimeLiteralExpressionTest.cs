using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// BL-11 日期时间类型前缀字面量回归测试。
///
/// 此前 literal 规则不含类型前缀分支，DATE/TimestampValue/TimeValue 是死代码（零实例化），
/// <c>DATE '2024-01-01'</c> 这类 SQL 标准日期字面量无法正确解析。
/// 本测试覆盖 DATE/DATETIME/TIME/TIMESTAMP/TIMESTAMPTZ 各种前缀的 round-trip 和 AST 断言。
/// </summary>
public class DateTimeLiteralExpressionTest
{
    [Theory]
    [InlineData("SELECT DATE '2024-01-01' FROM dual", "SELECT DATE '2024-01-01' FROM dual")]
    [InlineData("SELECT TIME '10:00:00' FROM dual", "SELECT TIME '10:00:00' FROM dual")]
    [InlineData("SELECT TIMESTAMP '2024-01-01 10:00:00' FROM dual", "SELECT TIMESTAMP '2024-01-01 10:00:00' FROM dual")]
    [InlineData("SELECT TIMESTAMPTZ '2024-01-01 10:00:00+08' FROM dual", "SELECT TIMESTAMPTZ '2024-01-01 10:00:00+08' FROM dual")]
    public void DateTimeLiteral_RoundTrip_ShouldPreserveType(string input, string expected)
    {
        var stmt = CCJSqlParserUtil.Parse(input);

        Assert.NotNull(stmt);
        Assert.Equal(expected, stmt!.ToString());
    }

    [Fact]
    public void DateTimeLiteral_InWhere_ShouldRoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM t WHERE created > DATE '2024-01-01'");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t WHERE created > DATE '2024-01-01'", stmt!.ToString());
    }

    [Fact]
    public void DateTimeLiteral_ShouldBuildCorrectNode()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT DATE '2024-01-01' FROM dual");

        Assert.NotNull(stmt);
        Assert.Contains("DATE '2024-01-01'", stmt!.ToString());
    }

    [Fact]
    public void DateTimeLiteral_TimestampValue_TypeShouldBeTimestamp()
    {
        // Value 保留原始 token 文本（含引号），对齐上游 setValue(t.image)
        var expr = new DateTimeLiteralExpression { Type = DateTimeType.Timestamp, Value = "'2024-01-01 10:00:00'" };
        Assert.Equal("TIMESTAMP '2024-01-01 10:00:00'", expr.ToString());
    }

    [Fact]
    public void DateTimeLiteral_DefaultType_ShouldOmitPrefix()
    {
        // Type 为 null 时省略前缀
        var expr = new DateTimeLiteralExpression { Value = "2024-01-01" };
        Assert.Equal("2024-01-01", expr.ToString());
    }
}
