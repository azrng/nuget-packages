using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// Expression 覆盖补充测试 — 填补审查中发现的未覆盖类型
/// </summary>
public class ExpressionCoverageTest
{
    #region BooleanValue

    [Fact]
    public void BooleanValue_True_ShouldBeTrue()
    {
        var expr = CCJSqlParserUtil.ParseExpression("TRUE");
        Assert.IsType<BooleanValue>(expr);
        Assert.True(((BooleanValue)expr!).Value);
    }

    [Fact]
    public void BooleanValue_False_ShouldBeFalse()
    {
        var expr = CCJSqlParserUtil.ParseExpression("FALSE");
        Assert.IsType<BooleanValue>(expr);
        Assert.False(((BooleanValue)expr!).Value);
    }

    [Fact]
    public void BooleanValue_InWhereClause_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM users WHERE active = TRUE")!;
        Assert.IsType<EqualsTo>(select.Where);
        var equals = (EqualsTo)select.Where!;
        Assert.IsType<BooleanValue>(equals.RightExpression);
        Assert.True(((BooleanValue)equals.RightExpression).Value);
    }

    #endregion

    #region HexValue

    [Fact]
    public void HexValue_ShouldBeHexValue()
    {
        var expr = CCJSqlParserUtil.ParseExpression("0xFF");
        Assert.IsType<HexValue>(expr);
        Assert.Equal("0xFF", ((HexValue)expr!).Value);
    }

    #endregion

    #region IsBooleanExpression

    [Fact]
    public void IsBooleanExpression_IsTrue_ShouldParse()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("active IS TRUE");
        Assert.IsType<IsBooleanExpression>(expr);
        var isBool = (IsBooleanExpression)expr!;
        Assert.False(isBool.Not);
        Assert.True(isBool.IsTrue);
    }

    [Fact]
    public void IsBooleanExpression_IsNotFalse_ShouldParse()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("active IS NOT FALSE");
        Assert.IsType<IsBooleanExpression>(expr);
        var isBool = (IsBooleanExpression)expr!;
        Assert.True(isBool.Not);
        Assert.False(isBool.IsTrue);
    }

    #endregion

    #region IsUnknownExpression

    [Fact]
    public void IsUnknownExpression_IsUnknown_ShouldParse()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("flag IS UNKNOWN");
        Assert.IsType<IsUnknownExpression>(expr);
        var isUnk = (IsUnknownExpression)expr!;
        Assert.False(isUnk.Not);
    }

    [Fact]
    public void IsUnknownExpression_IsNotUnknown_ShouldParse()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("flag IS NOT UNKNOWN");
        Assert.IsType<IsUnknownExpression>(expr);
        var isUnk = (IsUnknownExpression)expr!;
        Assert.True(isUnk.Not);
    }

    #endregion

    #region IsDistinctExpression

    [Fact]
    public void IsDistinctExpression_IsDistinctFrom_ShouldParse()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("a IS DISTINCT FROM b");
        Assert.IsType<IsDistinctExpression>(expr);
        var isDist = (IsDistinctExpression)expr!;
        Assert.False(isDist.Not);
        Assert.NotNull(isDist.LeftExpression);
        Assert.NotNull(isDist.RightExpression);
    }

    [Fact]
    public void IsDistinctExpression_IsNotDistinctFrom_ShouldParse()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("a IS NOT DISTINCT FROM b");
        Assert.IsType<IsDistinctExpression>(expr);
        Assert.True(((IsDistinctExpression)expr!).Not);
    }

    #endregion

    #region CaseExpression / WhenClause

    [Fact]
    public void CaseExpression_Simple_ShouldHaveWhenClauses()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT CASE WHEN id = 1 THEN 'a' ELSE 'b' END FROM users")!;
        var item = select.SelectItems![0];
        Assert.IsType<CaseExpression>(item.Expression);
        var caseExpr = (CaseExpression)item.Expression!;
        Assert.Single(caseExpr.WhenClauses);
        Assert.NotNull(caseExpr.ElseExpression);
    }

    [Fact]
    public void CaseExpression_SearchedCase_ShouldHaveMultipleWhenClauses()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT CASE WHEN id = 1 THEN 'a' WHEN id = 2 THEN 'b' ELSE 'c' END FROM users")!;
        var caseExpr = (CaseExpression)select.SelectItems![0].Expression!;
        Assert.Equal(2, caseExpr.WhenClauses.Count);
    }

    [Fact]
    public void WhenClause_ShouldHaveWhenAndThenExpressions()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT CASE WHEN id > 0 THEN name END FROM users")!;
        var caseExpr = (CaseExpression)select.SelectItems![0].Expression!;
        var when = caseExpr.WhenClauses[0];
        Assert.NotNull(when.WhenExpression);
        Assert.NotNull(when.ThenExpression);
    }

    #endregion

    #region CastExpression

    [Fact]
    public void CastExpression_CastKeyword_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT CAST(id AS INT) FROM users")!;
        var item = select.SelectItems![0];
        Assert.IsType<CastExpression>(item.Expression);
        var cast = (CastExpression)item.Expression!;
        Assert.Equal("INT", cast.DataType);
    }

    [Fact]
    public void CastExpression_DoubleColon_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id::varchar FROM users")!;
        var item = select.SelectItems![0];
        Assert.IsType<CastExpression>(item.Expression);
    }

    #endregion

    #region ExtractExpression

    [Fact]
    public void ExtractExpression_Year_ShouldHaveName()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT EXTRACT(YEAR FROM created_at) FROM users")!;
        var item = select.SelectItems![0];
        Assert.IsType<ExtractExpression>(item.Expression);
        Assert.Equal("YEAR", ((ExtractExpression)item.Expression!).Name);
    }

    [Fact]
    public void ExtractExpression_Month_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT EXTRACT(MONTH FROM created_at) FROM users")!;
        Assert.IsType<ExtractExpression>(select.SelectItems![0].Expression);
    }

    #endregion

    #region AnalyticExpression

    [Fact]
    public void AnalyticExpression_RowNumber_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT ROW_NUMBER() OVER(ORDER BY id) FROM users")!;
        Assert.IsType<AnalyticExpression>(select.SelectItems![0].Expression);
        var analytic = (AnalyticExpression)select.SelectItems![0].Expression!;
        Assert.Equal("ROW_NUMBER", analytic.Name);
        Assert.NotNull(analytic.OrderByElements);
    }

    [Fact]
    public void AnalyticExpression_WithPartitionBy_ShouldHavePartition()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT COUNT(*) OVER(PARTITION BY dept_id ORDER BY salary) FROM employees")!;
        var analytic = (AnalyticExpression)select.SelectItems![0].Expression!;
        Assert.NotNull(analytic.PartitionExpressionList);
        Assert.NotNull(analytic.OrderByElements);
        Assert.True(analytic.AllColumns);
    }

    #endregion

    #region Function

    [Fact]
    public void Function_CountStar_ShouldHaveName()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT COUNT(*) FROM users")!;
        Assert.IsType<Function>(select.SelectItems![0].Expression);
        var function = (Function)select.SelectItems![0].Expression!;
        Assert.Equal("COUNT", function.Name);
        Assert.True(function.AllColumns);
    }

    [Fact]
    public void Function_WithParameters_ShouldHaveParameters()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT COALESCE(name, 'default') FROM users")!;
        var func = (Function)select.SelectItems![0].Expression!;
        Assert.Equal("COALESCE", func.Name);
        Assert.NotNull(func.Parameters);
    }

    [Fact]
    public void Function_Sum_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT SUM(amount) FROM orders")!;
        Assert.IsType<Function>(select.SelectItems![0].Expression);
    }

    [Fact]
    public void Function_GroupConcat_WithSeparator_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT GROUP_CONCAT(name SEPARATOR ', ') FROM users")!;
        var func = Assert.IsType<Function>(select.SelectItems![0].Expression);
        Assert.Equal("GROUP_CONCAT", func.Name);
        Assert.NotNull(func.Separator);
        var output = func.ToString()!;
        Assert.Contains("SEPARATOR", output);
        Assert.Contains("', '", output);
    }

    [Fact]
    public void Function_GroupConcat_Distinct_ShouldHaveDistinctFlag()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT GROUP_CONCAT(DISTINCT name SEPARATOR ',') FROM users")!;
        var func = Assert.IsType<Function>(select.SelectItems![0].Expression);
        Assert.True(func.Distinct);
        Assert.Contains("DISTINCT", func.ToString()!);
    }

    [Fact]
    public void Function_GroupConcat_WithOrderBy_ShouldHaveOrderByElements()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT GROUP_CONCAT(name ORDER BY id DESC SEPARATOR '|') FROM users")!;
        var func = Assert.IsType<Function>(select.SelectItems![0].Expression);
        Assert.NotNull(func.OrderByElements);
        Assert.NotEmpty(func.OrderByElements);
        var output = func.ToString()!;
        Assert.Contains("ORDER BY id DESC", output);
        Assert.Contains("SEPARATOR '|'", output);
    }

    [Fact]
    public void Function_GroupConcat_NoClauses_ShouldParse()
    {
        // 不带 SEPARATOR/ORDER BY/DISTINCT 的最简 GROUP_CONCAT
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT GROUP_CONCAT(name) FROM users")!;
        var func = Assert.IsType<Function>(select.SelectItems![0].Expression);
        Assert.Equal("GROUP_CONCAT", func.Name);
        Assert.Null(func.Separator);
    }

    [Fact]
    public void Function_GroupConcat_MultipleExpressions_ShouldParse()
    {
        // GROUP_CONCAT 支持多表达式
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT GROUP_CONCAT(id, name SEPARATOR '|') FROM users")!;
        var func = Assert.IsType<Function>(select.SelectItems![0].Expression);
        Assert.NotNull(func.Parameters);
    }

    /// <summary>
    /// 通用函数关键字参数（对应上游 cd71aada / Function.KeywordArgument）。
    /// 应能解析 func(args) SEPARATOR ',' 等形式。
    /// </summary>
    [Fact]
    public void Function_KeywordArgument_ShouldRoundTrip()
    {
        // 在普通函数的 ) 之后附加关键字参数
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT my_func(arg) SEPARATOR ',' FROM t")!;
        var func = Assert.IsType<Function>(select.SelectItems![0].Expression);
        Assert.NotNull(func.KeywordArguments);
        Assert.Single(func.KeywordArguments!);
        Assert.Equal("SEPARATOR", func.KeywordArguments![0].Keyword);
        Assert.Contains("SEPARATOR", func.ToString()!);
    }

    [Fact]
    public void Function_MultipleKeywordArguments_ShouldRoundTrip()
    {
        // 多个连续关键字参数
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT my_func(arg) SEPARATOR ',' FROM t")!;
        var func = Assert.IsType<Function>(select.SelectItems![0].Expression);
        Assert.NotNull(func.KeywordArguments);
    }

    [Fact]
    public void Function_KeywordArgument_WithIgnore_ShouldRoundTrip()
    {
        // IGNORE 作为 nonReservedKeyword 的关键字参数
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT my_func(arg) IGNORE 1 FROM t")!;
        var func = Assert.IsType<Function>(select.SelectItems![0].Expression);
        Assert.NotNull(func.KeywordArguments);
        Assert.Equal("IGNORE", func.KeywordArguments![0].Keyword);
    }

    /// <summary>
    /// Oracle KEEP (DENSE_RANK FIRST|LAST ORDER BY ...) 应正确解析并往返。
    /// </summary>
    [Fact]
    public void Function_Keep_First_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT MAX(salary) KEEP (DENSE_RANK FIRST ORDER BY hire_date DESC) FROM employees")!;
        var func = Assert.IsType<Function>(select.SelectItems![0].Expression);
        Assert.NotNull(func.Keep);
        Assert.Equal("DENSE_RANK", func.Keep!.Name);
        Assert.True(func.Keep.First);
        Assert.NotNull(func.Keep.OrderByElements);
        var output = func.ToString()!;
        Assert.Contains("KEEP (DENSE_RANK FIRST ORDER BY", output);
    }

    [Fact]
    public void Function_Keep_Last_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT MIN(salary) KEEP (DENSE_RANK LAST ORDER BY hire_date) FROM employees")!;
        var func = Assert.IsType<Function>(select.SelectItems![0].Expression);
        Assert.NotNull(func.Keep);
        Assert.False(func.Keep!.First);
        Assert.Contains("KEEP (DENSE_RANK LAST", func.ToString()!);
    }

    [Fact]
    public void Function_NoKeep_ShouldHaveNullKeep()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT MAX(salary) FROM employees")!;
        var func = Assert.IsType<Function>(select.SelectItems![0].Expression);
        Assert.Null(func.Keep);
    }

    /// <summary>
    /// TRIM 函数各种形式应正确解析并往返。
    /// </summary>
    [Fact]
    public void Trim_Simple_ShouldRoundTrip()
    {
        // TRIM(str) 简单形式
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT TRIM('  hello  ') FROM t")!;
        var trim = Assert.IsType<TrimFunction>(select.SelectItems![0].Expression);
        Assert.Null(trim.TrimSpecification);
        Assert.NotNull(trim.FromExpression);
    }

    [Fact]
    public void Trim_LeadingFrom_ShouldRoundTrip()
    {
        // TRIM(LEADING ' ' FROM str) 标准形式
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT TRIM(LEADING ' ' FROM '  hello') FROM t")!;
        var trim = Assert.IsType<TrimFunction>(select.SelectItems![0].Expression);
        Assert.Equal(TrimSpecification.Leading, trim.TrimSpecification);
        Assert.NotNull(trim.Expression);
        Assert.NotNull(trim.FromExpression);
        Assert.True(trim.UsingFromKeyword);
        Assert.Contains("LEADING", trim.ToString()!);
        Assert.Contains("FROM", trim.ToString()!);
    }

    [Fact]
    public void Trim_TrailingFrom_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT TRIM(TRAILING ',' FROM 'hello,') FROM t")!;
        var trim = Assert.IsType<TrimFunction>(select.SelectItems![0].Expression);
        Assert.Equal(TrimSpecification.Trailing, trim.TrimSpecification);
    }

    [Fact]
    public void Trim_BothFrom_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT TRIM(BOTH ',' FROM ',,hello,,') FROM t")!;
        var trim = Assert.IsType<TrimFunction>(select.SelectItems![0].Expression);
        Assert.Equal(TrimSpecification.Both, trim.TrimSpecification);
    }

    [Fact]
    public void Trim_PostgresCommaForm_ShouldRoundTrip()
    {
        // PostgreSQL 风格：TRIM(chars, str)（用逗号而非 FROM）
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT TRIM(' ', '  hello') FROM t")!;
        var trim = Assert.IsType<TrimFunction>(select.SelectItems![0].Expression);
        Assert.NotNull(trim.Expression);
        Assert.NotNull(trim.FromExpression);
        Assert.False(trim.UsingFromKeyword);
        Assert.Contains(",", trim.ToString()!);
    }

    /// <summary>
    /// CollateExpression（expr COLLATE collation）应正确解析并往返。
    /// </summary>
    [Fact]
    public void CollateExpression_StringLiteral_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT name COLLATE 'en_US.utf8' FROM t")!;
        var collate = Assert.IsType<CollateExpression>(select.SelectItems![0].Expression);
        Assert.NotNull(collate.LeftExpression);
        Assert.Equal("'en_US.utf8'", collate.Collate);
        Assert.Contains("COLLATE 'en_US.utf8'", collate.ToString()!);
    }

    [Fact]
    public void CollateExpression_Identifier_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT name COLLATE utf8_unicode_ci FROM t")!;
        var collate = Assert.IsType<CollateExpression>(select.SelectItems![0].Expression);
        Assert.Equal("utf8_unicode_ci", collate.Collate);
    }

    /// <summary>
    /// TimezoneExpression（expr AT TIME ZONE zone）应正确解析并往返。
    /// </summary>
    [Fact]
    public void TimezoneExpression_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT ts AT TIME ZONE 'UTC' FROM t")!;
        var tz = Assert.IsType<TimezoneExpression>(select.SelectItems![0].Expression);
        Assert.NotNull(tz.LeftExpression);
        Assert.NotNull(tz.TimeZoneExpression);
        Assert.Contains("AT TIME ZONE", tz.ToString()!);
    }

    [Fact]
    public void TimezoneExpression_Chain_ShouldRoundTrip()
    {
        // 链式 AT TIME ZONE
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT ts AT TIME ZONE 'UTC' AT TIME ZONE 'PST' FROM t")!;
        Assert.NotNull(select.SelectItems);
        var output = select.ToString()!;
        Assert.Contains("AT TIME ZONE 'UTC'", output);
        Assert.Contains("AT TIME ZONE 'PST'", output);
    }

    /// <summary>
    /// 序列取值表达式 NEXTVAL FOR seq 和 NEXT VALUE FOR seq 应正确解析并往返。
    /// </summary>
    [Fact]
    public void NextValExpression_NextvalFor_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT NEXTVAL FOR seq_user FROM t")!;
        var nextVal = Assert.IsType<NextValExpression>(select.SelectItems![0].Expression);
        Assert.False(nextVal.UsingNextValueFor);
        Assert.Equal("seq_user", nextVal.Name);
        Assert.Equal("NEXTVAL FOR seq_user", nextVal.ToString());
    }

    [Fact]
    public void NextValExpression_NextValueFor_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT NEXT VALUE FOR seq_user FROM t")!;
        var nextVal = Assert.IsType<NextValExpression>(select.SelectItems![0].Expression);
        Assert.True(nextVal.UsingNextValueFor);
        Assert.Contains("NEXT VALUE FOR seq_user", nextVal.ToString());
    }

    [Fact]
    public void NextValExpression_QualifiedName_ShouldRoundTrip()
    {
        // schema.seq 多段限定名
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT NEXTVAL FOR my_schema.seq_user FROM t")!;
        var nextVal = Assert.IsType<NextValExpression>(select.SelectItems![0].Expression);
        Assert.Equal("my_schema.seq_user", nextVal.Name);
        Assert.Equal(2, nextVal.NameList.Count);
    }

    /// <summary>
    /// ANY/SOME/ALL 比较表达式应正确解析并往返。
    /// </summary>
    [Fact]
    public void AnyComparison_EqualAny_ShouldRoundTrip()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression(
            "x = ANY (SELECT id FROM t)");
        Assert.NotNull(expr);
        var output = expr!.ToString()!;
        Assert.Contains("ANY", output);
        Assert.Contains("SELECT id FROM t", output);
    }

    [Fact]
    public void AnyComparison_GreaterAll_ShouldRoundTrip()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression(
            "x > ALL (SELECT val FROM t)");
        Assert.NotNull(expr);
        Assert.Contains("ALL", expr!.ToString()!);
    }

    [Fact]
    public void AnyComparison_EqualSome_ShouldRoundTrip()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression(
            "x = SOME (SELECT id FROM t)");
        Assert.NotNull(expr);
        Assert.Contains("SOME", expr!.ToString()!);
    }

    /// <summary>
    /// 数组构造器和数组下标访问应正确解析并往返。
    /// </summary>
    [Fact]
    public void ArrayConstructor_WithKeyword_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT ARRAY[1, 2, 3] FROM t")!;
        var arr = Assert.IsType<ArrayConstructor>(select.SelectItems![0].Expression);
        Assert.True(arr.ArrayKeyword);
        Assert.NotNull(arr.Expressions);
        Assert.Equal(3, arr.Expressions!.Expressions.Count);
        Assert.Equal("ARRAY[1, 2, 3]", arr.ToString());
    }

    [Fact]
    public void ArrayExpression_Index_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT arr[1] FROM t")!;
        var idx = Assert.IsType<ArrayExpression>(select.SelectItems![0].Expression);
        Assert.NotNull(idx.ObjExpression);
        Assert.NotNull(idx.IndexExpression);
        Assert.Null(idx.StartIndexExpression);
        Assert.Null(idx.StopIndexExpression);
        Assert.Contains("[1]", idx.ToString()!);
    }

    [Fact]
    public void ArrayExpression_Range_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT arr[1:3] FROM t")!;
        var idx = Assert.IsType<ArrayExpression>(select.SelectItems![0].Expression);
        Assert.Null(idx.IndexExpression);
        Assert.NotNull(idx.StartIndexExpression);
        Assert.NotNull(idx.StopIndexExpression);
        Assert.Contains("[1:3]", idx.ToString()!);
    }

    [Fact]
    public void ArrayConstructor_Empty_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT ARRAY[] FROM t")!;
        var arr = Assert.IsType<ArrayConstructor>(select.SelectItems![0].Expression);
        Assert.True(arr.ArrayKeyword);
    }

    /// <summary>
    /// 行构造器 ROW(...) 应正确解析并往返。
    /// </summary>
    [Fact]
    public void RowConstructor_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT ROW(1, 2, 3) FROM t")!;
        var row = Assert.IsType<RowConstructor>(select.SelectItems![0].Expression);
        Assert.Equal("ROW", row.Name);
        Assert.NotNull(row.Expressions);
        Assert.Equal(3, row.Expressions!.Expressions.Count);
        Assert.Equal("ROW(1, 2, 3)", row.ToString());
    }

    [Fact]
    public void RowConstructor_SingleValue_ShouldRoundTrip()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT ROW(1) FROM t")!;
        var row = Assert.IsType<RowConstructor>(select.SelectItems![0].Expression);
        Assert.Single(row.Expressions!.Expressions);
    }

    [Fact]
    public void RowConstructor_InWhere_ShouldRoundTrip()
    {
        // WHERE (a, b) IN (SELECT x, y FROM t) 形式
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT * FROM t WHERE ROW(a, b) IN (SELECT x, y FROM t2)")!;
        Assert.NotNull(select.Where);
        Assert.Contains("ROW(a, b)", select.ToString()!);
    }

    #endregion

    #region ExcludesExpression / IncludesExpression

    [Fact]
    public void ExcludesExpression_ShouldParse()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("a EXCLUDES (1, 2)");
        Assert.IsType<ExcludesExpression>(expr);
        var excludes = (ExcludesExpression)expr!;
        Assert.NotNull(excludes.LeftExpression);
        Assert.NotNull(excludes.RightExpression);
    }

    [Fact]
    public void IncludesExpression_ShouldParse()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("a INCLUDES (1, 2)");
        Assert.IsType<IncludesExpression>(expr);
        var includes = (IncludesExpression)expr!;
        Assert.NotNull(includes.LeftExpression);
        Assert.NotNull(includes.RightExpression);
    }

    #endregion

    #region RegExpMatchOperator

    [Fact]
    public void RegExpMatchOperator_Regexp_ShouldParse()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("name REGEXP '^test'");
        Assert.IsType<RegExpMatchOperator>(expr);
        var regexp = (RegExpMatchOperator)expr!;
        Assert.Equal("REGEXP", regexp.Operator);
        Assert.NotNull(regexp.LeftExpression);
        Assert.NotNull(regexp.RightExpression);
    }

    [Fact]
    public void RegExpMatchOperator_Rlike_ShouldParse()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("name RLIKE '^test'");
        Assert.IsType<RegExpMatchOperator>(expr);
        Assert.Equal("RLIKE", ((RegExpMatchOperator)expr!).Operator);
    }

    [Fact]
    public void RegExpMatchOperator_NotRegexp_ShouldSetNot()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("name NOT REGEXP '^test'");
        Assert.IsType<RegExpMatchOperator>(expr);
        var regexp = (RegExpMatchOperator)expr!;
        Assert.True(regexp.Not);
        Assert.Equal("name NOT REGEXP '^test'", regexp.ToString());
    }

    #endregion

    #region IntervalExpression

    [Fact]
    public void IntervalExpression_WithDay_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT INTERVAL '7' DAY FROM users")!;
        var item = select.SelectItems![0];
        Assert.IsType<IntervalExpression>(item.Expression);
        var interval = (IntervalExpression)item.Expression!;
        Assert.Equal("DAY", interval.IntervalType);
        Assert.True(interval.IntervalKeyword);
        Assert.NotNull(interval.Expression);
    }

    [Fact]
    public void IntervalExpression_WithYear_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT INTERVAL '1' YEAR FROM users")!;
        var interval = (IntervalExpression)select.SelectItems![0].Expression!;
        Assert.Equal("YEAR", interval.IntervalType);
    }

    #endregion

    #region RowGetExpression / 字段访问

    /// <summary>
    /// PostgreSQL 复合类型 cast 后的多层字段访问应正确解析并往返。
    /// 对应上游 commit 5b5fe6c2 (issue #2404)。
    /// </summary>
    [Fact]
    public void NestedCompositeFieldAccess_AfterCast_ShouldRoundTrip()
    {
        var sql = "SELECT (product_data::product_info_similarity).info.category AS category FROM products";
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void NestedCompositeFieldAccess_GroupBy_ShouldRoundTrip()
    {
        var sql = "SELECT COUNT(*) FROM products GROUP BY (product_data::product_info_similarity).info.category";
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void SimpleFieldAccess_OnParenthesis_ShouldRoundTrip()
    {
        var sql = "SELECT (a).b FROM t";
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    #endregion

    #region KeyExpression

    /// <summary>
    /// MySQL 方言的 KEY 前缀表达式（如 KEY chain.entity）应正确解析并往返。
    /// 对应上游 commit bfcb8b75 (issue #2409)。
    /// </summary>
    [Fact]
    public void KeyExpression_AsFunctionParameter_ShouldRoundTrip()
    {
        var expr = CCJSqlParserUtil.ParseExpression("aes_decrypt(from_base64(entity), KEY chain.entity)");
        Assert.NotNull(expr);
        Assert.Equal("aes_decrypt(from_base64(entity), KEY chain.entity)", expr!.ToString());
    }

    [Fact]
    public void KeyExpression_Simple_ShouldBeKeyExpressionType()
    {
        var expr = CCJSqlParserUtil.ParseExpression("KEY chain.entity");
        var keyExpr = Assert.IsType<KeyExpression>(expr);
        Assert.Equal("chain.entity", keyExpr.Expression!.ToString());
        Assert.Equal("KEY chain.entity", expr!.ToString());
    }

    [Fact]
    public void KeyExpression_InSelect_ShouldRoundTrip()
    {
        var sql = "SELECT aes_decrypt(entity, KEY chain.entity) FROM t";
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    #endregion

    #region FullTextSearch (MATCH ... AGAINST)

    /// <summary>
    /// MySQL FULLTEXT 搜索的 AGAINST 应支持任意表达式（含 concat、参数等）。
    /// 对应上游 commit 5788ca06 (issue #2413)。
    /// </summary>
    [Fact]
    public void FullTextSearch_AgainstConcatExpression_ShouldRoundTrip()
    {
        var sql = "SELECT MATCH (name) AGAINST (concat('', ?, '') IN BOOLEAN MODE) AS full_text FROM commodity";
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void FullTextSearch_AgainstStringLiteral_ShouldRoundTrip()
    {
        var sql = "SELECT MATCH (title, body) AGAINST ('database') FROM articles";
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void FullTextSearch_InBooleanMode_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM articles WHERE MATCH (title) AGAINST ('+MySQL -YourSQL' IN BOOLEAN MODE)";
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void FullTextSearch_InNaturalLanguageMode_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM articles WHERE MATCH (title) AGAINST ('database' IN NATURAL LANGUAGE MODE)";
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    #endregion

    #region InExpression 优先级

    /// <summary>
    /// IN 表达式的优先级应高于 OR，确保 WHERE a IN (...) OR b = 2 解析为
    /// (a IN (...)) OR (b = 2) 而非 a IN (... OR b = 2)。
    /// 对应上游 issue #2244（JavaCC 版曾因 InExpression 右侧用 Expression() 导致贪婪匹配）。
    /// ANTLR 版文法通过显式括号分组天然规避此问题，此处补充回归测试固化正确行为。
    /// </summary>
    [Fact]
    public void InExpression_WithOrSuffix_ShouldParseOrAtTopLevel()
    {
        var sql = "SELECT * FROM T_DEMO WHERE a IN (1, 3, 2) OR b = 2";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        // WHERE 顶层应为 OrExpression，左操作数为 InExpression，右操作数为比较表达式
        var orExpr = Assert.IsType<OrExpression>(select.Where);
        Assert.IsType<InExpression>(orExpr.LeftExpression);
    }

    /// <summary>
    /// IN 表达式的优先级应高于 AND，确保 WHERE a IN (...) AND b = 2 解析为
    /// (a IN (...)) AND (b = 2)。
    /// </summary>
    [Fact]
    public void InExpression_WithAndSuffix_ShouldParseAndAtTopLevel()
    {
        var sql = "SELECT * FROM T_DEMO WHERE a IN (1, 3, 2) AND b = 2";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var andExpr = Assert.IsType<AndExpression>(select.Where);
        Assert.IsType<InExpression>(andExpr.LeftExpression);
    }

    #endregion
}
