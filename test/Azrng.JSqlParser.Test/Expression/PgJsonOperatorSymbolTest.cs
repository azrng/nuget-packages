using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// PostgreSQL 数组/JSON 运算符符号对齐测试。
/// 对齐上游 JSqlParser 5.4：
/// - Contains/ContainedBy = &amp;&gt;/&lt;&amp;（PG 数组范围运算符，ComparisonOperator 子类）
/// - JsonOperator = 参数化（-&gt;/-&gt;&gt;/#&gt;/#&gt;&gt;/@&gt;/&lt;@/?/?|/?&amp;/||/-/-#）
///
/// 此前 Azrng 把 @&gt;/&lt;@ 错配给 Contains/ContainedBy（应为 &amp;&gt;/&lt;&amp;），
/// 且 JsonOperator 硬编码 -&gt; 无法承载其他 JSON 运算符，与 Matches ~→@@ 同型迁移走样。
/// </summary>
public class PgJsonOperatorSymbolTest
{
    #region Contains / ContainedBy 符号

    private static readonly IExpression LeftCol = new Column { ColumnName = "a" };
    private static readonly IExpression RightCol = new Column { ColumnName = "b" };

    [Fact]
    public void Contains_DefaultSymbol_ShouldBeAmpersandGt()
    {
        // 上游 Contains.java:24 super("&>")
        var contains = new Contains { LeftExpression = LeftCol, RightExpression = RightCol };
        Assert.Equal("&>", contains.OperatorSymbol);
    }

    [Fact]
    public void ContainedBy_DefaultSymbol_ShouldBeLtAmpersand()
    {
        // 上游 ContainedBy.java:24 super("<&")
        var containedBy = new ContainedBy { LeftExpression = LeftCol, RightExpression = RightCol };
        Assert.Equal("<&", containedBy.OperatorSymbol);
    }

    [Fact]
    public void Contains_And_ContainedBy_ShouldBeComparisonOperator()
    {
        // 对齐上游：二者都继承 ComparisonOperator（带 Operator 字段），不是裸 BinaryExpression
        var contains = new Contains { LeftExpression = LeftCol, RightExpression = RightCol };
        var containedBy = new ContainedBy { LeftExpression = LeftCol, RightExpression = RightCol };
        Assert.IsAssignableFrom<ComparisonOperator>(contains);
        Assert.IsAssignableFrom<ComparisonOperator>(containedBy);
    }

    [Fact]
    public void Contains_CustomSymbol_ShouldOverrideDefault()
    {
        // 双构造器支持自定义符号（对齐 CosineSimilarity/GeometryDistance 范式）
        var contains = new Contains("@>") { LeftExpression = LeftCol, RightExpression = RightCol };
        Assert.Equal("@>", contains.OperatorSymbol);
    }

    [Fact]
    public void Contains_ToString_ShouldUseAmpersandGtSymbol()
    {
        var contains = new Contains
        {
            LeftExpression = new Column { ColumnName = "a" },
            RightExpression = new Column { ColumnName = "b" }
        };
        Assert.Equal("a &> b", contains.ToString());
    }

    [Fact]
    public void ContainedBy_ToString_ShouldUseLtAmpersandSymbol()
    {
        var containedBy = new ContainedBy
        {
            LeftExpression = new Column { ColumnName = "a" },
            RightExpression = new Column { ColumnName = "b" }
        };
        Assert.Equal("a <& b", containedBy.ToString());
    }

    #endregion

    #region JsonOperator 参数化

    [Fact]
    public void JsonOperator_DefaultSymbol_ShouldBeArrow()
    {
        // 无参构造默认 "->"（向后兼容 Azrng 既有行为）
        var op = new JsonOperator { LeftExpression = LeftCol, RightExpression = RightCol };
        Assert.Equal("->", op.OperatorSymbol);
    }

    [Theory]
    [InlineData("->")]
    [InlineData("->>")]
    [InlineData("#>")]
    [InlineData("#>>")]
    [InlineData("@>")]
    [InlineData("<@")]
    [InlineData("?")]
    [InlineData("?|")]
    [InlineData("?&")]
    [InlineData("||")]
    [InlineData("-")]
    [InlineData("-#")]
    public void JsonOperator_CustomSymbol_ShouldRoundTrip(string symbol)
    {
        // 上游 JsonOperator(String op) 可承载 12+ JSON 运算符
        // 上游 .jjt:6838-6845 明确列出 @>、<@、?、?|、?&、||、-、-# 等
        var op = new JsonOperator(symbol) { LeftExpression = LeftCol, RightExpression = RightCol };
        Assert.Equal(symbol, op.OperatorSymbol);
        Assert.Equal(symbol, op.Operator);
    }

    [Fact]
    public void JsonOperator_ToString_ShouldUseInjectedSymbol()
    {
        var op = new JsonOperator("@>")
        {
            LeftExpression = new Column { ColumnName = "a" },
            RightExpression = new Column { ColumnName = "b" }
        };
        Assert.Equal("a @> b", op.ToString());
    }

    [Fact]
    public void JsonOperator_SetOperator_AfterConstruction_ShouldReflectInSymbol()
    {
        // Operator 是 { get; set; }，运行时可改
        var op = new JsonOperator("->") { LeftExpression = LeftCol, RightExpression = RightCol };
        op.Operator = "->>";
        Assert.Equal("->>", op.OperatorSymbol);
    }

    #endregion
}
