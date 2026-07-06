using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// 范围表达式 start : end 测试（移植自上游 RangeExpression，主要用于数组构造）。
/// </summary>
public class RangeExpressionTest
{
    [Fact]
    public void RangeExpression_InArrayConstructor_ShouldRoundTrip()
    {
        var sql = "SELECT ARRAY[1:10] FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var array = Assert.IsType<ArrayConstructor>(select.SelectItems![0].Expression);
        var range = Assert.IsType<RangeExpression>(array.Expressions!.Expressions[0]);
        Assert.Equal("1", range.StartExpression?.ToString());
        Assert.Equal("10", range.EndExpression?.ToString());

        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void RangeExpression_MixedWithScalarInArray_ShouldRoundTrip()
    {
        var sql = "SELECT ARRAY[1:10, 20] FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var array = Assert.IsType<ArrayConstructor>(select.SelectItems![0].Expression);
        Assert.Equal(2, array.Expressions!.Expressions.Count);
        Assert.IsType<RangeExpression>(array.Expressions.Expressions[0]);
        Assert.IsType<LongValue>(array.Expressions.Expressions[1]);

        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void RangeExpression_NestedArray_ShouldRoundTrip()
    {
        var sql = "SELECT ARRAY[1:10, ARRAY[2, 3]] FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var array = Assert.IsType<ArrayConstructor>(select.SelectItems![0].Expression);
        Assert.Equal(2, array.Expressions!.Expressions.Count);
        Assert.IsType<RangeExpression>(array.Expressions.Expressions[0]);
        Assert.IsType<ArrayConstructor>(array.Expressions.Expressions[1]);

        Assert.Equal(sql, select.ToString());
    }
}
