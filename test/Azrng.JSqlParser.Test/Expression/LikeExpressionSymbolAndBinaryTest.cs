using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// LikeExpression 的 SQL 关键字输出与 MySQL BINARY 标记测试（批次6）。
///
/// 此前 OperatorSymbol 用 .ToUpperInvariant() 把 PascalCase 枚举名转大写，
/// 丢失下划线（RegexpLike → REGEXPLIKE 应为 REGEXP_LIKE，MatchAny → MATCHANY 应为 MATCH_ANY）。
/// 也无 UseBinary 字段（MySQL x LIKE BINARY 'A' 大小写敏感匹配无法表达）。
/// </summary>
public class LikeExpressionSymbolAndBinaryTest
{
    private static readonly IExpression LeftCol = new Column { ColumnName = "name" };

    #region OperatorSymbol 保留下划线

    [Theory]
    [InlineData(LikeExpression.KeyWord.Like, "LIKE")]
    [InlineData(LikeExpression.KeyWord.Ilike, "ILIKE")]
    [InlineData(LikeExpression.KeyWord.Rlike, "RLIKE")]
    [InlineData(LikeExpression.KeyWord.Regexp, "REGEXP")]
    [InlineData(LikeExpression.KeyWord.RegexpLike, "REGEXP_LIKE")]   // 保留下划线
    [InlineData(LikeExpression.KeyWord.SimilarTo, "SIMILAR TO")]      // 保留空格
    [InlineData(LikeExpression.KeyWord.MatchAny, "MATCH_ANY")]        // 保留下划线
    [InlineData(LikeExpression.KeyWord.MatchAll, "MATCH_ALL")]
    [InlineData(LikeExpression.KeyWord.MatchPhrase, "MATCH_PHRASE")]
    [InlineData(LikeExpression.KeyWord.MatchPhrasePrefix, "MATCH_PHRASE_PREFIX")]
    [InlineData(LikeExpression.KeyWord.MatchRegexp, "MATCH_REGEXP")]
    public void OperatorSymbol_PreservesUpstreamSqlKeyword(LikeExpression.KeyWord kw, string expected)
    {
        var like = new LikeExpression
        {
            LeftExpression = LeftCol,
            RightExpression = new Column { ColumnName = "pat" },
            LikeKeyWord = kw
        };
        Assert.Equal(expected, like.OperatorSymbol);
    }

    [Fact]
    public void ToString_RegexpLike_PreservesUnderscore()
    {
        var like = new LikeExpression
        {
            LeftExpression = LeftCol,
            RightExpression = new Column { ColumnName = "pat" },
            LikeKeyWord = LikeExpression.KeyWord.RegexpLike
        };
        // 此前 ToUpperInvariant() 输出 REGEXPLIKE（无下划线），round-trip 会重新解析失败
        Assert.Equal("name REGEXP_LIKE pat", like.ToString());
    }

    [Fact]
    public void ToString_MatchAny_PreservesUnderscore()
    {
        var like = new LikeExpression
        {
            LeftExpression = LeftCol,
            RightExpression = new Column { ColumnName = "pat" },
            LikeKeyWord = LikeExpression.KeyWord.MatchAny
        };
        Assert.Equal("name MATCH_ANY pat", like.ToString());
    }

    [Fact]
    public void ToString_NotLike_ShouldRenderNotPrefix()
    {
        var like = new LikeExpression
        {
            LeftExpression = LeftCol,
            RightExpression = new Column { ColumnName = "pat" },
            Not = true
        };
        Assert.Equal("name NOT LIKE pat", like.ToString());
    }

    #endregion

    #region MySQL LIKE BINARY

    [Fact]
    public void UseBinary_True_ShouldRenderBinaryBeforeRightExpression()
    {
        var like = new LikeExpression
        {
            LeftExpression = LeftCol,
            RightExpression = new Column { ColumnName = "pat" },
            UseBinary = true
        };
        // BINARY 在 LIKE 之后、右表达式之前（对齐上游 LikeExpression.java:53-54）
        Assert.Equal("name LIKE BINARY pat", like.ToString());
    }

    [Fact]
    public void UseBinary_False_ShouldNotRenderBinary()
    {
        var like = new LikeExpression
        {
            LeftExpression = LeftCol,
            RightExpression = new Column { ColumnName = "pat" }
        };
        Assert.DoesNotContain("BINARY", like.ToString());
    }

    [Fact]
    public void UseBinary_WithNotAndEscape_ShouldComposeCorrectly()
    {
        var like = new LikeExpression
        {
            LeftExpression = LeftCol,
            RightExpression = new Column { ColumnName = "pat" },
            Not = true,
            UseBinary = true,
            Escape = new Column { ColumnName = "\\" }
        };
        // NOT LIKE BINARY pat ESCAPE ...
        Assert.Equal("name NOT LIKE BINARY pat ESCAPE \\", like.ToString());
    }

    [Fact]
    public void Parse_MysqlLikeBinary_ShouldRoundTrip()
    {
        // grammar 在 LIKE 关键字后加 BINARY? 选项（对齐上游 jjt:7058 [ LOOKAHEAD(2) <K_BINARY> ]）
        var expr = (LikeExpression)SqlParser.ParseCondExpression("name LIKE BINARY 'A%'")!;
        Assert.True(expr.UseBinary);
        Assert.Contains("LIKE BINARY", expr.ToString()!);
    }

    [Fact]
    public void Parse_StandardLike_NoBinary()
    {
        var expr = (LikeExpression)SqlParser.ParseCondExpression("name LIKE 'A%'")!;
        Assert.False(expr.UseBinary);
    }

    #endregion
}
