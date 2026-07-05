using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// Expression 基础类型测试
/// </summary>
public class ExpressionBasicTest
{
    #region 值类型

    [Fact]
    public void LongValue_ShouldHoldIntegerValue()
    {
        var expr = (LongValue)CCJSqlParserUtil.ParseExpression("42")!;
        Assert.Equal(42, expr.Value);
    }

    [Fact]
    public void LongValue_Negative_ShouldBeSignedExpression()
    {
        // -1 被解析为 SignedExpression('-', LongValue(1))
        var expr = CCJSqlParserUtil.ParseExpression("-1");
        Assert.IsType<SignedExpression>(expr);
        var signed = (SignedExpression)expr;
        Assert.Equal('-', signed.Sign);
        Assert.IsType<LongValue>(signed.Expression);
    }

    [Fact]
    public void DoubleValue_ShouldHoldDoubleValue()
    {
        var expr = (DoubleValue)CCJSqlParserUtil.ParseExpression("3.14")!;
        Assert.Equal(3.14, expr.Value, 2);
    }

    [Fact]
    public void StringValue_ShouldHoldStringValue()
    {
        var expr = (StringValue)CCJSqlParserUtil.ParseExpression("'hello'")!;
        Assert.Equal("hello", expr.Value);
    }

    [Fact]
    public void StringValue_Empty_ShouldHoldEmptyString()
    {
        var expr = (StringValue)CCJSqlParserUtil.ParseExpression("''")!;
        Assert.Equal("", expr.Value);
    }

    [Fact]
    public void NullValue_ShouldBeNull()
    {
        var expr = CCJSqlParserUtil.ParseExpression("NULL");
        Assert.NotNull(expr);
        Assert.IsType<NullValue>(expr);
    }

    #endregion

    #region JDBC 参数

    [Fact]
    public void JdbcParameter_QuestionMark_ShouldBeParameter()
    {
        var expr = CCJSqlParserUtil.ParseExpression("?");
        Assert.NotNull(expr);
        Assert.IsType<JdbcParameter>(expr);
    }

    [Fact]
    public void JdbcNamedParameter_WithName_ShouldHoldName()
    {
        var expr = (JdbcNamedParameter)CCJSqlParserUtil.ParseExpression(":name")!;
        Assert.Equal("name", expr.Name);
    }

    #endregion

    #region NotExpression / Parenthesis / SignedExpression

    [Fact]
    public void NotExpression_ShouldWrapInnerExpression()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("NOT (id = 1)");
        Assert.IsType<NotExpression>(expr);
        var notExpr = (NotExpression)expr;
        Assert.NotNull(notExpr.Expression);
    }

    [Fact]
    public void Parenthesis_ShouldWrapInnerExpression()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("(id = 1)");
        Assert.IsType<Parenthesis>(expr);
        var paren = (Parenthesis)expr;
        Assert.NotNull(paren.Expression);
    }

    [Fact]
    public void SignedExpression_Positive_ShouldBeLongValue()
    {
        // +1 is normalized to LongValue(1) by the parser
        var expr = CCJSqlParserUtil.ParseExpression("+1")!;
        Assert.IsType<LongValue>(expr);
    }

    [Fact]
    public void SignedExpression_Negative_ShouldHoldSign()
    {
        var expr = (SignedExpression)CCJSqlParserUtil.ParseExpression("-1")!;
        Assert.Equal('-', expr.Sign);
    }

    #endregion

    #region 条件运算符

    [Fact]
    public void AndExpression_ShouldHaveLeftAndRight()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("a = 1 AND b = 2");
        Assert.IsType<AndExpression>(expr);
        var and = (AndExpression)expr;
        Assert.NotNull(and.LeftExpression);
        Assert.NotNull(and.RightExpression);
    }

    [Fact]
    public void OrExpression_ShouldHaveLeftAndRight()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("a = 1 OR b = 2");
        Assert.IsType<OrExpression>(expr);
        var or = (OrExpression)expr;
        Assert.NotNull(or.LeftExpression);
        Assert.NotNull(or.RightExpression);
    }

    [Fact]
    public void AndOrCombined_ShouldNestCorrectly()
    {
        // AND 优先级高于 OR
        var expr = CCJSqlParserUtil.ParseCondExpression("a = 1 AND b = 2 OR c = 3");
        Assert.IsType<OrExpression>(expr);
    }

    [Fact]
    public void ParenthesizedAndOr_ShouldRespectParentheses()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("a = 1 AND (b = 2 OR c = 3)");
        Assert.IsType<AndExpression>(expr);
        var and = (AndExpression)expr;
        Assert.IsType<Parenthesis>(and.RightExpression);
    }

    #endregion

    #region 关系运算符

    [Fact]
    public void EqualsTo_ShouldHaveLeftAndRight()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("id = 1");
        Assert.IsType<EqualsTo>(expr);
        var eq = (EqualsTo)expr;
        Assert.NotNull(eq.LeftExpression);
        Assert.NotNull(eq.RightExpression);
    }

    [Fact]
    public void NotEqualsTo_ShouldHaveLeftAndRight()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("id <> 1");
        Assert.IsType<NotEqualsTo>(expr);
    }

    [Fact]
    public void GreaterThan_ShouldHaveLeftAndRight()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("age > 18");
        Assert.IsType<GreaterThan>(expr);
    }

    [Fact]
    public void GreaterThanEquals_ShouldHaveLeftAndRight()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("age >= 18");
        Assert.IsType<GreaterThanEquals>(expr);
    }

    [Fact]
    public void MinorThan_ShouldHaveLeftAndRight()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("age < 60");
        Assert.IsType<MinorThan>(expr);
    }

    [Fact]
    public void MinorThanEquals_ShouldHaveLeftAndRight()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("age <= 60");
        Assert.IsType<MinorThanEquals>(expr);
    }

    [Fact]
    public void LikeExpression_ShouldHaveLeftAndRight()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("name LIKE '%test%'");
        Assert.IsType<LikeExpression>(expr);
        var like = (LikeExpression)expr;
        Assert.NotNull(like.LeftExpression);
        Assert.NotNull(like.RightExpression);
    }

    [Fact]
    public void NotLikeExpression_ShouldBeNot()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("name NOT LIKE '%test%'");
        Assert.IsType<LikeExpression>(expr);
        var like = (LikeExpression)expr;
        Assert.True(like.Not);
    }

    [Fact]
    public void InExpression_ShouldHaveLeftAndRightItemsList()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("id IN (1, 2, 3)");
        Assert.IsType<InExpression>(expr);
        var inExpr = (InExpression)expr;
        Assert.NotNull(inExpr.LeftExpression);
        Assert.NotNull(inExpr.RightExpression);
    }

    [Fact]
    public void NotInExpression_ShouldBeNot()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("id NOT IN (1, 2, 3)");
        Assert.IsType<InExpression>(expr);
        var inExpr = (InExpression)expr;
        Assert.True(inExpr.Not);
    }

    [Fact]
    public void IsNullExpression_ShouldHaveLeftExpression()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("deleted_at IS NULL");
        Assert.IsType<IsNullExpression>(expr);
        var isNull = (IsNullExpression)expr;
        Assert.NotNull(isNull.LeftExpression);
        Assert.False(isNull.Not);
    }

    [Fact]
    public void IsNotNullExpression_ShouldBeNot()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("deleted_at IS NOT NULL");
        Assert.IsType<IsNullExpression>(expr);
        var isNull = (IsNullExpression)expr;
        Assert.True(isNull.Not);
    }

    [Fact]
    public void Between_ShouldHaveLeftBeginAndEnd()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("age BETWEEN 18 AND 60");
        Assert.IsType<Between>(expr);
        var between = (Between)expr;
        Assert.NotNull(between.LeftExpression);
        Assert.NotNull(between.BetweenExpressionStart);
        Assert.NotNull(between.BetweenExpressionEnd);
    }

    /// <summary>
    /// SQL:2016 BETWEEN SYMMETRIC/ASYMMETRIC 应正确解析并往返。
    /// 对应上游 commit 001ad1c2 (issue #2250)。
    /// </summary>
    [Fact]
    public void Between_Symmetric_ShouldRoundTrip()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("a BETWEEN SYMMETRIC 1 AND 10");
        var between = Assert.IsType<Between>(expr);
        Assert.True(between.UsingSymmetric);
        Assert.Equal("a BETWEEN SYMMETRIC 1 AND 10", expr!.ToString());
    }

    [Fact]
    public void Between_Asymmetric_ShouldRoundTrip()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("a BETWEEN ASYMMETRIC 1 AND 10");
        var between = Assert.IsType<Between>(expr);
        Assert.True(between.UsingAsymmetric);
        Assert.Equal("a BETWEEN ASYMMETRIC 1 AND 10", expr!.ToString());
    }

    /// <summary>
    /// BETWEEN 内带括号的复杂表达式应能正确解析（对应上游 commit f10b52ed / issue #2288）。
    /// ANTLR 用 ALL(*) 解析，天然规避 JavaCC LOOKAHEAD 限制。
    /// </summary>
    [Fact]
    public void Between_WithParenthesis_ShouldParse()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression(
            "ts BETWEEN CAST(CAST((NOW() + INTERVAL '-30 day') AS date) AS timestamp) AND NOW()");
        var between = Assert.IsType<Between>(expr);
        Assert.NotNull(between.BetweenExpressionStart);
        Assert.NotNull(between.BetweenExpressionEnd);
    }

    [Fact]
    public void Between_WithParenthesisOnBothSides_ShouldParse()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression(
            "a BETWEEN (1 + 2) AND (3 * 4)");
        var between = Assert.IsType<Between>(expr);
        Assert.NotNull(between.BetweenExpressionStart);
        Assert.NotNull(between.BetweenExpressionEnd);
    }

    /// <summary>
    /// WITH AS NOT MATERIALIZED 应能解析（对应上游 commit 2f6afbc3）。
    /// </summary>
    [Fact]
    public void Cte_NotMaterialized_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "WITH cte AS NOT MATERIALIZED (SELECT id FROM src) SELECT * FROM cte");
        Assert.NotNull(stmt);
    }

    [Fact]
    public void Cte_Materialized_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "WITH cte AS MATERIALIZED (SELECT id FROM src) SELECT * FROM cte");
        Assert.NotNull(stmt);
    }

    /// <summary>
    /// TRY_CAST 已支持（对应上游 9dfa0d68 中提到的 TRY_CONVERT/SAFE_CONVERT 等价物）。
    /// </summary>
    [Fact]
    public void Cast_TryCast_ShouldParse()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("TRY_CAST(id AS INTEGER)");
        Assert.NotNull(expr);
    }

    /// <summary>
    /// PostgreSQL dollar-quoted string body 已支持（对应上游 95ebda5a）。
    /// </summary>
    [Fact]
    public void DollarQuotedString_InExpression_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT $$hello world$$ FROM t")!;
        var sv = Assert.IsType<StringValue>(select.SelectItems![0].Expression);
        Assert.Equal("hello world", sv.Value);
        Assert.Equal("$$", sv.DollarPrefix);
        Assert.Equal("SELECT $$hello world$$ FROM t", select.ToString());
    }

    [Fact]
    public void DollarQuotedString_Tagged_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT $tag$body$tag$ FROM t")!;
        var sv = Assert.IsType<StringValue>(select.SelectItems![0].Expression);
        Assert.Equal("$tag$", sv.DollarPrefix);
    }

    /// <summary>
    /// 带前缀的字符串字面量（N'..'、E'..'、U'..'、R'..'、B'..'、RB'..'）应正确解析并往返。
    /// </summary>
    [Theory]
    [InlineData("N", "abc")]
    [InlineData("E", "x")]
    [InlineData("U", "y")]
    [InlineData("R", "raw")]
    [InlineData("B", "bit")]
    [InlineData("RB", "rb")]
    public void StringValue_WithPrefix_ShouldRoundTrip(string prefix, string content)
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse($"SELECT {prefix}'{content}' FROM t")!;
        var sv = Assert.IsType<StringValue>(select.SelectItems![0].Expression);
        Assert.Equal(prefix, sv.Prefix);
        Assert.Equal(content, sv.Value);
        Assert.Equal($"{prefix}'{content}'", sv.ToString());
    }

    [Fact]
    public void StringValue_NoPrefix_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT 'hello' FROM t")!;
        var sv = Assert.IsType<StringValue>(select.SelectItems![0].Expression);
        Assert.Null(sv.Prefix);
        Assert.Equal("hello", sv.Value);
        Assert.Equal("'hello'", sv.ToString());
    }

    [Fact]
    public void StringValue_Utf8Prefix_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT _utf8'unicode' FROM t")!;
        var sv = Assert.IsType<StringValue>(select.SelectItems![0].Expression);
        Assert.Equal("_utf8", sv.Prefix);
        Assert.Equal("unicode", sv.Value);
    }

    /// <summary>
    /// Oracle q'...{...}...' 自定义分隔引号应能正确解析。
    /// </summary>
    [Fact]
    public void OracleQString_Bracket_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT q'[abc]' FROM dual")!;
        var sv = Assert.IsType<StringValue>(select.SelectItems![0].Expression);
        Assert.Equal("abc", sv.Value);
    }

    [Fact]
    public void OracleQString_Paren_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT q'(hello)' FROM dual")!;
        var sv = Assert.IsType<StringValue>(select.SelectItems![0].Expression);
        Assert.Equal("hello", sv.Value);
    }

    [Fact]
    public void OracleQString_CanContainSingleQuote()
    {
        // Oracle q-string 优势：内容可含单引号而无需转义
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT q'[it''s]' FROM dual")!;
        var sv = Assert.IsType<StringValue>(select.SelectItems![0].Expression);
        Assert.Equal("it''s", sv.Value);
    }

    /// <summary>
    /// Lambda 表达式：单参数形式（对应上游 12489af6 关注的 lambda 解析）。
    /// Azrng 文法 identifier LAMBDA_ARROW expression 较保守，不会过度解析。
    /// </summary>
    [Fact]
    public void Lambda_SingleParam_ShouldRoundTrip()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("x -> x + 1");
        Assert.NotNull(expr);
        Assert.Contains("->", expr!.ToString()!);
    }

    [Fact]
    public void Lambda_MultiParam_ShouldRoundTrip()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("(x, y) -> x + y");
        Assert.NotNull(expr);
    }

    [Fact]
    public void Between_NotSymmetric_ShouldRoundTrip()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("a NOT BETWEEN SYMMETRIC 1 AND 10");
        var between = Assert.IsType<Between>(expr);
        Assert.True(between.Not);
        Assert.True(between.UsingSymmetric);
        Assert.Equal("a NOT BETWEEN SYMMETRIC 1 AND 10", expr!.ToString());
    }

    [Fact]
    public void ExistsExpression_ShouldHaveRightExpression()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("EXISTS (SELECT 1 FROM t)");
        Assert.IsType<ExistsExpression>(expr);
        var exists = (ExistsExpression)expr;
        Assert.NotNull(exists.RightExpression);
    }

    [Fact]
    public void NotExistsExpression_ShouldBeNotExpressionWrappingExists()
    {
        // NOT EXISTS 被解析为 NotExpression(ExistsExpression)
        var expr = CCJSqlParserUtil.ParseCondExpression("NOT EXISTS (SELECT 1 FROM t)");
        Assert.IsType<NotExpression>(expr);
        var notExpr = (NotExpression)expr;
        Assert.IsType<ExistsExpression>(notExpr.Expression);
    }

    #endregion

    #region BinaryExpression 通用属性

    [Fact]
    public void BinaryExpression_StringExpression_ShouldBeEquals()
    {
        var expr = (Azrng.JSqlParser.Expression.Operators.Arithmetic.BinaryExpression)
            CCJSqlParserUtil.ParseCondExpression("id = 1")!;
        Assert.Equal("=", expr.GetStringExpression());
    }

    [Fact]
    public void BinaryExpression_StringExpression_ShouldBeGreaterThan()
    {
        var expr = (Azrng.JSqlParser.Expression.Operators.Arithmetic.BinaryExpression)
            CCJSqlParserUtil.ParseCondExpression("id > 1")!;
        Assert.Equal(">", expr.GetStringExpression());
    }

    #endregion

    #region Alias

    [Fact]
    public void Alias_OnSelectItem_ShouldHaveAlias()
    {
        var select = (PlainSelect)
            CCJSqlParserUtil.Parse("SELECT id AS user_id FROM users")!;
        var item = select.SelectItems![0];
        Assert.NotNull(item.Alias);
        Assert.Equal("user_id", item.Alias!.Name);
    }

    #endregion
}
