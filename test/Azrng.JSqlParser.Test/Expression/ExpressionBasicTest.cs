using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
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
        var expr = (LongValue)SqlParser.ParseExpression("42")!;
        Assert.Equal(42, expr.Value);
    }

    [Fact]
    public void LongValue_Negative_ShouldBeSignedExpression()
    {
        // -1 被解析为 SignedExpression('-', LongValue(1))
        var expr = SqlParser.ParseExpression("-1");
        Assert.IsType<SignedExpression>(expr);
        var signed = (SignedExpression)expr;
        Assert.Equal('-', signed.Sign);
        Assert.IsType<LongValue>(signed.Expression);
    }

    [Fact]
    public void DoubleValue_ShouldHoldDoubleValue()
    {
        var expr = (DoubleValue)SqlParser.ParseExpression("3.14")!;
        Assert.Equal(3.14, expr.Value, 2);
    }

    [Fact]
    public void StringValue_ShouldHoldStringValue()
    {
        var expr = (StringValue)SqlParser.ParseExpression("'hello'")!;
        Assert.Equal("hello", expr.Value);
    }

    [Fact]
    public void StringValue_Empty_ShouldHoldEmptyString()
    {
        var expr = (StringValue)SqlParser.ParseExpression("''")!;
        Assert.Equal("", expr.Value);
    }

    [Fact]
    public void NullValue_ShouldBeNull()
    {
        var expr = SqlParser.ParseExpression("NULL");
        Assert.NotNull(expr);
        Assert.IsType<NullValue>(expr);
    }

    #endregion

    #region JDBC 参数

    [Fact]
    public void JdbcParameter_QuestionMark_ShouldBeParameter()
    {
        var expr = SqlParser.ParseExpression("?");
        Assert.NotNull(expr);
        Assert.IsType<JdbcParameter>(expr);
    }

    [Fact]
    public void JdbcNamedParameter_WithName_ShouldHoldName()
    {
        var expr = (JdbcNamedParameter)SqlParser.ParseExpression(":name")!;
        Assert.Equal("name", expr.Name);
    }

    [Fact]
    public void JdbcNamedParameter_WithAtName_ShouldHoldNameAndPrefix()
    {
        var expr = (JdbcNamedParameter)SqlParser.ParseExpression("@name")!;
        Assert.Equal("name", expr.Name);
        Assert.Equal("@", expr.Prefix);
        Assert.Equal("@name", expr.ToString());
    }

    [Fact]
    public void JdbcNamedParameter_WithAtNameInComparison_ShouldHoldName()
    {
        var expr = (EqualsTo)SqlParser.ParseCondExpression("u.name = @name")!;
        var parameter = Assert.IsType<JdbcNamedParameter>(expr.RightExpression);
        Assert.Equal("name", parameter.Name);
        Assert.Equal("@", parameter.Prefix);
    }

    #endregion

    #region NotExpression / Parenthesis / SignedExpression

    [Fact]
    public void NotExpression_ShouldWrapInnerExpression()
    {
        var expr = SqlParser.ParseCondExpression("NOT (id = 1)");
        Assert.IsType<NotExpression>(expr);
        var notExpr = (NotExpression)expr;
        Assert.NotNull(notExpr.Expression);
    }

    [Fact]
    public void Parenthesis_ShouldWrapInnerExpression()
    {
        var expr = SqlParser.ParseCondExpression("(id = 1)");
        Assert.IsType<Parenthesis>(expr);
        var paren = (Parenthesis)expr;
        Assert.NotNull(paren.Expression);
    }

    [Fact]
    public void SignedExpression_Positive_ShouldBeLongValue()
    {
        // +1 is normalized to LongValue(1) by the parser
        var expr = SqlParser.ParseExpression("+1")!;
        Assert.IsType<LongValue>(expr);
    }

    [Fact]
    public void SignedExpression_Negative_ShouldHoldSign()
    {
        var expr = (SignedExpression)SqlParser.ParseExpression("-1")!;
        Assert.Equal('-', expr.Sign);
    }

    #endregion

    #region 条件运算符

    [Fact]
    public void AndExpression_ShouldHaveLeftAndRight()
    {
        var expr = SqlParser.ParseCondExpression("a = 1 AND b = 2");
        Assert.IsType<AndExpression>(expr);
        var and = (AndExpression)expr;
        Assert.NotNull(and.LeftExpression);
        Assert.NotNull(and.RightExpression);
    }

    [Fact]
    public void OrExpression_ShouldHaveLeftAndRight()
    {
        var expr = SqlParser.ParseCondExpression("a = 1 OR b = 2");
        Assert.IsType<OrExpression>(expr);
        var or = (OrExpression)expr;
        Assert.NotNull(or.LeftExpression);
        Assert.NotNull(or.RightExpression);
    }

    [Fact]
    public void AndOrCombined_ShouldNestCorrectly()
    {
        // AND 优先级高于 OR
        var expr = SqlParser.ParseCondExpression("a = 1 AND b = 2 OR c = 3");
        Assert.IsType<OrExpression>(expr);
    }

    [Fact]
    public void ParenthesizedAndOr_ShouldRespectParentheses()
    {
        var expr = SqlParser.ParseCondExpression("a = 1 AND (b = 2 OR c = 3)");
        Assert.IsType<AndExpression>(expr);
        var and = (AndExpression)expr;
        Assert.IsType<Parenthesis>(and.RightExpression);
    }

    #endregion

    #region 关系运算符

    [Fact]
    public void EqualsTo_ShouldHaveLeftAndRight()
    {
        var expr = SqlParser.ParseCondExpression("id = 1");
        Assert.IsType<EqualsTo>(expr);
        var eq = (EqualsTo)expr;
        Assert.NotNull(eq.LeftExpression);
        Assert.NotNull(eq.RightExpression);
    }

    [Fact]
    public void NotEqualsTo_ShouldHaveLeftAndRight()
    {
        var expr = SqlParser.ParseCondExpression("id <> 1");
        Assert.IsType<NotEqualsTo>(expr);
    }

    [Fact]
    public void GreaterThan_ShouldHaveLeftAndRight()
    {
        var expr = SqlParser.ParseCondExpression("age > 18");
        Assert.IsType<GreaterThan>(expr);
    }

    [Fact]
    public void GreaterThanEquals_ShouldHaveLeftAndRight()
    {
        var expr = SqlParser.ParseCondExpression("age >= 18");
        Assert.IsType<GreaterThanEquals>(expr);
    }

    [Fact]
    public void MinorThan_ShouldHaveLeftAndRight()
    {
        var expr = SqlParser.ParseCondExpression("age < 60");
        Assert.IsType<MinorThan>(expr);
    }

    [Fact]
    public void MinorThanEquals_ShouldHaveLeftAndRight()
    {
        var expr = SqlParser.ParseCondExpression("age <= 60");
        Assert.IsType<MinorThanEquals>(expr);
    }

    [Fact]
    public void LikeExpression_ShouldHaveLeftAndRight()
    {
        var expr = SqlParser.ParseCondExpression("name LIKE '%test%'");
        Assert.IsType<LikeExpression>(expr);
        var like = (LikeExpression)expr;
        Assert.NotNull(like.LeftExpression);
        Assert.NotNull(like.RightExpression);
    }

    [Fact]
    public void NotLikeExpression_ShouldBeNot()
    {
        var expr = SqlParser.ParseCondExpression("name NOT LIKE '%test%'");
        Assert.IsType<LikeExpression>(expr);
        var like = (LikeExpression)expr;
        Assert.True(like.Not);
    }

    [Fact]
    public void InExpression_ShouldHaveLeftAndRightItemsList()
    {
        var expr = SqlParser.ParseCondExpression("id IN (1, 2, 3)");
        Assert.IsType<InExpression>(expr);
        var inExpr = (InExpression)expr;
        Assert.NotNull(inExpr.LeftExpression);
        Assert.NotNull(inExpr.RightExpression);
    }

    [Fact]
    public void NotInExpression_ShouldBeNot()
    {
        var expr = SqlParser.ParseCondExpression("id NOT IN (1, 2, 3)");
        Assert.IsType<InExpression>(expr);
        var inExpr = (InExpression)expr;
        Assert.True(inExpr.Not);
    }

    [Fact]
    public void IsNullExpression_ShouldHaveLeftExpression()
    {
        var expr = SqlParser.ParseCondExpression("deleted_at IS NULL");
        Assert.IsType<IsNullExpression>(expr);
        var isNull = (IsNullExpression)expr;
        Assert.NotNull(isNull.LeftExpression);
        Assert.False(isNull.Not);
    }

    [Fact]
    public void IsNotNullExpression_ShouldBeNot()
    {
        var expr = SqlParser.ParseCondExpression("deleted_at IS NOT NULL");
        Assert.IsType<IsNullExpression>(expr);
        var isNull = (IsNullExpression)expr;
        Assert.True(isNull.Not);
    }

    [Fact]
    public void Between_ShouldHaveLeftBeginAndEnd()
    {
        var expr = SqlParser.ParseCondExpression("age BETWEEN 18 AND 60");
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
        var expr = SqlParser.ParseCondExpression("a BETWEEN SYMMETRIC 1 AND 10");
        var between = Assert.IsType<Between>(expr);
        Assert.True(between.UsingSymmetric);
        Assert.Equal("a BETWEEN SYMMETRIC 1 AND 10", expr!.ToString());
    }

    [Fact]
    public void Between_Asymmetric_ShouldRoundTrip()
    {
        var expr = SqlParser.ParseCondExpression("a BETWEEN ASYMMETRIC 1 AND 10");
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
        var expr = SqlParser.ParseCondExpression(
            "ts BETWEEN CAST(CAST((NOW() + INTERVAL '-30 day') AS date) AS timestamp) AND NOW()");
        var between = Assert.IsType<Between>(expr);
        Assert.NotNull(between.BetweenExpressionStart);
        Assert.NotNull(between.BetweenExpressionEnd);
    }

    [Fact]
    public void Between_WithParenthesisOnBothSides_ShouldParse()
    {
        var expr = SqlParser.ParseCondExpression(
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
        var stmt = SqlParser.Parse(
            "WITH cte AS NOT MATERIALIZED (SELECT id FROM src) SELECT * FROM cte");
        Assert.NotNull(stmt);
    }

    [Fact]
    public void Cte_Materialized_ShouldParse()
    {
        var stmt = SqlParser.Parse(
            "WITH cte AS MATERIALIZED (SELECT id FROM src) SELECT * FROM cte");
        Assert.NotNull(stmt);
    }

    /// <summary>
    /// TRY_CAST 已支持（对应上游 9dfa0d68 中提到的 TRY_CONVERT/SAFE_CONVERT 等价物）。
    /// </summary>
    [Fact]
    public void Cast_TryCast_ShouldParse()
    {
        var expr = SqlParser.ParseCondExpression("TRY_CAST(id AS INTEGER)");
        Assert.NotNull(expr);
    }

    /// <summary>
    /// PostgreSQL dollar-quoted string body 已支持（对应上游 95ebda5a）。
    /// </summary>
    [Fact]
    public void DollarQuotedString_InExpression_ShouldRoundTrip()
    {
        var select = (PlainSelect)SqlParser.Parse("SELECT $$hello world$$ FROM t")!;
        var sv = Assert.IsType<StringValue>(select.SelectItems![0].Expression);
        Assert.Equal("hello world", sv.Value);
        Assert.Equal("$$", sv.DollarPrefix);
        Assert.Equal("SELECT $$hello world$$ FROM t", select.ToString());
    }

    [Fact]
    public void DollarQuotedString_Tagged_ShouldRoundTrip()
    {
        var select = (PlainSelect)SqlParser.Parse("SELECT $tag$body$tag$ FROM t")!;
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
        var select = (PlainSelect)SqlParser.Parse($"SELECT {prefix}'{content}' FROM t")!;
        var sv = Assert.IsType<StringValue>(select.SelectItems![0].Expression);
        Assert.Equal(prefix, sv.Prefix);
        Assert.Equal(content, sv.Value);
        Assert.Equal($"{prefix}'{content}'", sv.ToString());
    }

    [Fact]
    public void StringValue_NoPrefix_ShouldRoundTrip()
    {
        var select = (PlainSelect)SqlParser.Parse("SELECT 'hello' FROM t")!;
        var sv = Assert.IsType<StringValue>(select.SelectItems![0].Expression);
        Assert.Null(sv.Prefix);
        Assert.Equal("hello", sv.Value);
        Assert.Equal("'hello'", sv.ToString());
    }

    [Fact]
    public void StringValue_Utf8Prefix_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse("SELECT _utf8'unicode' FROM t")!;
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
        var select = (PlainSelect)SqlParser.Parse("SELECT q'[abc]' FROM dual")!;
        var sv = Assert.IsType<StringValue>(select.SelectItems![0].Expression);
        Assert.Equal("abc", sv.Value);
    }

    [Fact]
    public void OracleQString_Paren_ShouldRoundTrip()
    {
        var select = (PlainSelect)SqlParser.Parse("SELECT q'(hello)' FROM dual")!;
        var sv = Assert.IsType<StringValue>(select.SelectItems![0].Expression);
        Assert.Equal("hello", sv.Value);
    }

    [Fact]
    public void OracleQString_CanContainSingleQuote()
    {
        // Oracle q-string 优势：内容可含单引号而无需转义
        var select = (PlainSelect)SqlParser.Parse("SELECT q'[it''s]' FROM dual")!;
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
        var expr = SqlParser.ParseCondExpression("x -> x + 1");
        Assert.NotNull(expr);
        Assert.Contains("->", expr!.ToString()!);
    }

    [Fact]
    public void Lambda_MultiParam_ShouldRoundTrip()
    {
        var expr = SqlParser.ParseCondExpression("(x, y) -> x + y");
        Assert.NotNull(expr);
    }

    /// <summary>
    /// 日期单位字段作列名/参数/函数应能解析（对应上游 4fdfa785 DateUnitExpression）。
    /// </summary>
    [Fact]
    public void DateUnit_AsColumnName_ShouldParse()
    {
        var stmt = SqlParser.Parse("SELECT YEAR FROM t")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void DateUnit_AsFunctionArg_ShouldParse()
    {
        var stmt = SqlParser.Parse("SELECT TIMESTAMPDIFF(YEAR, a, b) FROM t")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void DateUnit_TimestampAdd_ShouldRoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT TIMESTAMPADD(HOUR, 1, ts) FROM t")!;
        Assert.NotNull(stmt);
        // 往返
        Assert.Contains("TIMESTAMPADD(HOUR, 1, ts)", stmt.ToString()!);
    }

    [Fact]
    public void DateUnit_MonthDaySecond_ShouldAllParse()
    {
        foreach (var unit in new[] { "MONTH", "DAY", "HOUR", "MINUTE", "SECOND" })
        {
            var stmt = SqlParser.Parse($"SELECT TIMESTAMPDIFF({unit}, a, b) FROM t")!;
            Assert.NotNull(stmt);
        }
    }

    [Fact]
    public void DateUnitExpression_Type_ShouldRoundTrip()
    {
        var expr = new DateUnitExpression(DateUnit.Year);
        Assert.Equal("YEAR", expr.ToString());
        var expr2 = new DateUnitExpression("month");
        Assert.Equal(DateUnit.Month, expr2.Unit);
    }

    /// <summary>
    /// Oracle 命名函数参数（name => value）应正确解析并往返。
    /// 对应上游 commit 834afe18 / OracleNamedFunctionParameter。
    /// </summary>
    [Fact]
    public void OracleNamedFunctionParameter_ShouldRoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT my_func(arg1 => 'x', arg2 => 42) FROM t")!;
        Assert.NotNull(stmt);
        var output = stmt.ToString()!;
        Assert.Contains("arg1 => 'x'", output);
        Assert.Contains("arg2 => 42", output);
    }

    [Fact]
    public void OracleNamedFunctionParameter_Single_ShouldParse()
    {
        var stmt = SqlParser.Parse("SELECT my_func(name => col) FROM t")!;
        Assert.NotNull(stmt);
        Assert.Contains("=>", stmt.ToString()!);
    }

    /// <summary>
    /// PostgreSQL 命名函数参数（name := value）应正确解析并往返。
    /// </summary>
    [Fact]
    public void PostgresNamedFunctionParameter_ShouldRoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT my_func(arg1 := 'x') FROM t")!;
        Assert.NotNull(stmt);
        Assert.Contains(":=", stmt.ToString()!);
    }

    /// <summary>
    /// PostgreSQL 多命名参数应正确解析并往返（上游 commit 7c52e7fe 核心场景）。
    /// </summary>
    [Fact]
    public void PostgresNamedFunctionParameter_MultipleArgs_ShouldRoundTrip()
    {
        var sql = "SELECT concat_lower_or_upper(a := 'Hello', b := 'World')";
        var stmt = SqlParser.Parse(sql)!;
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void NamedFunctionParameter_MixedShouldParse()
    {
        // 混合命名参数和位置参数（命名参数通常在后面）
        var stmt = SqlParser.Parse("SELECT my_func(1, 2, opt => 3) FROM t")!;
        Assert.NotNull(stmt);
    }

    /// <summary>
    /// Oracle 多行 Hint（/*+ ... */）应正确解析并往返。
    /// </summary>
    [Fact]
    public void OracleHint_MultiLine_ShouldRoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT /*+ INDEX(t idx) */ id FROM t")!;
        var select = (PlainSelect)stmt;
        Assert.NotNull(select.OracleHint);
        Assert.Contains("INDEX(t idx)", select.OracleHint!.Value!);
        Assert.False(select.OracleHint!.SingleLine);
        // 往返
        Assert.Contains("/*+ INDEX(t idx) */", stmt.ToString()!);
    }

    [Fact]
    public void OracleHint_None_ShouldBeNull()
    {
        var stmt = SqlParser.Parse("SELECT id FROM t")!;
        var select = (PlainSelect)stmt;
        Assert.Null(select.OracleHint);
    }

    [Fact]
    public void OracleHint_RegularComment_ShouldNotBeHint()
    {
        // 普通块注释不应被识别为 Oracle Hint
        var stmt = SqlParser.Parse("SELECT /* normal comment */ id FROM t")!;
        var select = (PlainSelect)stmt;
        Assert.Null(select.OracleHint);
    }

    [Fact]
    public void OracleHint_Type_ShouldRoundTrip()
    {
        var hint = new OracleHint("/*+ FIRST_ROWS(100) */");
        Assert.Equal("FIRST_ROWS(100)", hint.Value);
        Assert.False(hint.SingleLine);
        Assert.Equal("/*+ FIRST_ROWS(100) */", hint.ToString());
    }

    [Fact]
    public void Between_NotSymmetric_ShouldRoundTrip()
    {
        var expr = SqlParser.ParseCondExpression("a NOT BETWEEN SYMMETRIC 1 AND 10");
        var between = Assert.IsType<Between>(expr);
        Assert.True(between.Not);
        Assert.True(between.UsingSymmetric);
        Assert.Equal("a NOT BETWEEN SYMMETRIC 1 AND 10", expr!.ToString());
    }

    [Fact]
    public void ExistsExpression_ShouldHaveRightExpression()
    {
        var expr = SqlParser.ParseCondExpression("EXISTS (SELECT 1 FROM t)");
        Assert.IsType<ExistsExpression>(expr);
        var exists = (ExistsExpression)expr;
        Assert.NotNull(exists.RightExpression);
    }

    [Fact]
    public void NotExistsExpression_ShouldBeNotExpressionWrappingExists()
    {
        // NOT EXISTS 被解析为 NotExpression(ExistsExpression)
        var expr = SqlParser.ParseCondExpression("NOT EXISTS (SELECT 1 FROM t)");
        Assert.IsType<NotExpression>(expr);
        var notExpr = (NotExpression)expr;
        Assert.IsType<ExistsExpression>(notExpr.Expression);
    }

    #endregion

    #region BinaryExpression 通用属性

    [Fact]
    public void BinaryExpression_OperatorSymbol_ShouldBeEquals()
    {
        var expr = (Azrng.JSqlParser.Expression.Operators.Arithmetic.BinaryExpression)
            SqlParser.ParseCondExpression("id = 1")!;
        Assert.Equal("=", expr.OperatorSymbol);
    }

    [Fact]
    public void BinaryExpression_OperatorSymbol_ShouldBeGreaterThan()
    {
        var expr = (Azrng.JSqlParser.Expression.Operators.Arithmetic.BinaryExpression)
            SqlParser.ParseCondExpression("id > 1")!;
        Assert.Equal(">", expr.OperatorSymbol);
    }

    #endregion

    #region Alias

    [Fact]
    public void Alias_OnSelectItem_ShouldHaveAlias()
    {
        var select = (PlainSelect)
            SqlParser.Parse("SELECT id AS user_id FROM users")!;
        var item = select.SelectItems![0];
        Assert.NotNull(item.Alias);
        Assert.Equal("user_id", item.Alias!.Name);
    }

    #endregion

    #region required 字段初始化器构造（批 8）

    /// <summary>
    /// Between 的三个必填字段（批 8 改为 required）用对象初始化器构造，序列化正确。
    /// </summary>
    [Fact]
    public void Between_RequiredFields_ObjectInitializer_ShouldSerialize()
    {
        var between = new Between
        {
            LeftExpression = new Column { ColumnName = "age" },
            BetweenExpressionStart = new LongValue(18),
            BetweenExpressionEnd = new LongValue(65)
        };
        Assert.Equal("age BETWEEN 18 AND 65", between.ToString());
    }

    /// <summary>
    /// Parenthesis 的必填 Expression 字段（批 8 改为 required）用对象初始化器构造，序列化正确。
    /// </summary>
    [Fact]
    public void Parenthesis_RequiredField_ObjectInitializer_ShouldSerialize()
    {
        var paren = new Parenthesis
        {
            Expression = new LongValue(42)
        };
        Assert.Equal("(42)", paren.ToString());
    }

    #endregion
}
