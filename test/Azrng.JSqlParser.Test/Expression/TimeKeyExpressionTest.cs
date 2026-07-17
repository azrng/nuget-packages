using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// 时间关键字表达式测试（移植自上游 TimeKeyExpression）。
/// </summary>
public class TimeKeyExpressionTest
{
    [Theory]
    [InlineData("CURRENT_DATE")]
    [InlineData("CURRENT_TIME")]
    [InlineData("CURRENT_TIMESTAMP")]
    [InlineData("CURRENT_TIMEZONE")]
    [InlineData("LOCALTIME")]
    [InlineData("LOCALTIMESTAMP")]
    public void TimeKeyExpression_StandardKeys_ShouldRoundTrip(string keyword)
    {
        var sql = $"SELECT {keyword} FROM t";
        var select = (PlainSelect)SqlParser.Parse(sql)!;

        var timeKey = Assert.IsType<TimeKeyExpression>(select.SelectItems![0].Expression);
        Assert.Equal(keyword, timeKey.StringValue);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void TimeKeyExpression_LowerCase_ShouldPreserveText()
    {
        var sql = "SELECT current_timestamp FROM t";
        var select = (PlainSelect)SqlParser.Parse(sql)!;

        var timeKey = Assert.IsType<TimeKeyExpression>(select.SelectItems![0].Expression);
        Assert.Equal("current_timestamp", timeKey.StringValue);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void TimeKeyExpression_AsColumnName_ShouldStillWorkAsColumn()
    {
        // CURRENT_DATE 是非保留关键字，应能作为列名使用
        var sql = "SELECT t.current_date FROM t";
        var select = (PlainSelect)SqlParser.Parse(sql)!;

        // t.current_date 是列引用，不应被识别为 TimeKeyExpression
        Assert.IsNotType<TimeKeyExpression>(select.SelectItems![0].Expression);
        Assert.Equal(sql, select.ToString());
    }
}
