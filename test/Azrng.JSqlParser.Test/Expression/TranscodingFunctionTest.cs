using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// CONVERT / TRY_CONVERT / SAFE_CONVERT 双风格转码函数测试（移植自上游 TranscodingFunction）。
/// </summary>
public class TranscodingFunctionTest
{
    [Fact]
    public void TranscodingFunction_TranscodeStyle_ShouldRoundTrip()
    {
        // 上游 TranscodingFunction.appendTo 用 "( ... )" 带空格格式输出
        var sql = "SELECT CONVERT( name USING utf8 ) FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<TranscodingFunction>(select.SelectItems![0].Expression);
        Assert.True(func.IsTranscodeStyle);
        Assert.Equal("CONVERT", func.Keyword);
        Assert.Equal("utf8", func.TranscodingName);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void TranscodingFunction_TypeConversionStyle_ShouldRoundTrip()
    {
        var sql = "SELECT CONVERT( VARCHAR, age ) FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<TranscodingFunction>(select.SelectItems![0].Expression);
        Assert.False(func.IsTranscodeStyle);
        Assert.Equal("VARCHAR", func.ColDataType);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void TranscodingFunction_SqlServerStyle_WithStyleNumber_ShouldRoundTrip()
    {
        var sql = "SELECT CONVERT( VARCHAR, getdate(), 120 ) FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<TranscodingFunction>(select.SelectItems![0].Expression);
        Assert.False(func.IsTranscodeStyle);
        Assert.Equal("VARCHAR", func.ColDataType);
        Assert.Equal("120", func.TranscodingName);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void TranscodingFunction_TryConvert_ShouldRoundTrip()
    {
        var sql = "SELECT TRY_CONVERT( INT, col ) FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<TranscodingFunction>(select.SelectItems![0].Expression);
        Assert.Equal("TRY_CONVERT", func.Keyword);
        Assert.False(func.IsTranscodeStyle);
        Assert.Equal("INT", func.ColDataType);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void TranscodingFunction_SafeConvert_ShouldRoundTrip()
    {
        var sql = "SELECT SAFE_CONVERT( DECIMAL, amount ) FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<TranscodingFunction>(select.SelectItems![0].Expression);
        Assert.Equal("SAFE_CONVERT", func.Keyword);
        Assert.Equal(sql, select.ToString());
    }
}
