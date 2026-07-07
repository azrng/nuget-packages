using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// 客户迁移反馈的回归测试集。
///
/// 客户在从上游 JSqlParser 迁移到 Azrng.JSqlParser 时反馈了若干“不支持 / 格式化有问题”的项，
/// 本测试逐项核查实际行为并用 round-trip / AST 断言固化现状，避免后续改动引入回归。
///
/// 客户反馈编号与结论：
///   #1 != / || 格式化（全角/空格）—— 客户已明确为格式化问题，本测试不覆盖。
///   #3 CASE WHEN —— 真 Bug，已在 AstBuilderVisitor.VisitCaseExpr 修复，回归见 CaseExpressionTest。
///   #4 -- 行注释 —— 实测不抛错，与 /* */ 一致地被 lexer 丢弃（上游一致行为）。
///   #5 NULL AS 字段名 —— 实测 round-trip 正常。
///   #6 '0' || 字段 拼接 —— 实测 round-trip 正常（Concat 节点）。
///   #7 UNION ALL —— 实测 round-trip 正常（SetOperationList）。
///   #8 a.qty::varchar(20) 类型转换 —— 实测 round-trip 正常（CastExpression UseCastKeyword=false）。
/// </summary>
public class CustomerReportedRegressionTest
{
    #region #4 注释（-- 与 /* */ 均被 lexer 丢弃，不抛错）

    [Theory]
    [InlineData("SELECT a FROM t -- this is a comment\nWHERE a = 1", "SELECT a FROM t WHERE a = 1")]
    [InlineData("SELECT a FROM t -- comment\r\nWHERE a = 1", "SELECT a FROM t WHERE a = 1")]
    [InlineData("SELECT a FROM t -- comment at end", "SELECT a FROM t")]
    [InlineData("-- whole line comment\nSELECT a FROM t", "SELECT a FROM t")]
    [InlineData("SELECT a -- col comment\nFROM t WHERE a = 1", "SELECT a FROM t WHERE a = 1")]
    public void LineComment_ShouldParseAndBeDropped(string input, string expected)
    {
        // 客户反馈 #4：备注只支持 /* */ 不支持 --。
        // 现状：-- 行注释可正常解析（不抛错），与 /* */ 一致地被 lexer（-> skip）丢弃，
        // 与上游 JSqlParser 行为一致；本测试固化该现状，不作为 Bug 处理。
        var stmt = CCJSqlParserUtil.Parse(input);

        Assert.NotNull(stmt);
        Assert.Equal(expected, stmt!.ToString());
    }

    [Fact]
    public void LineComment_Chinese_ShouldParseWithoutThrowing()
    {
        // 中文注释也不应导致解析失败
        var sql = "SELECT a FROM t -- 中文注释\nWHERE a = 1";

        var stmt = CCJSqlParserUtil.Parse(sql);

        Assert.NotNull(stmt);
        Assert.Equal("SELECT a FROM t WHERE a = 1", stmt!.ToString());
    }

    [Fact]
    public void BlockComment_ShouldParseAndBeDropped()
    {
        // /* */ 块注释同样被丢弃，行为与 -- 一致
        var sql = "SELECT a FROM t /* block comment */ WHERE a = 1";

        var stmt = CCJSqlParserUtil.Parse(sql);

        Assert.NotNull(stmt);
        Assert.Equal("SELECT a FROM t WHERE a = 1", stmt!.ToString());
    }

    [Fact]
    public void InlineBlockComment_ShouldParseAndBeDropped()
    {
        var sql = "SELECT /* hint */ a FROM t";

        var stmt = CCJSqlParserUtil.Parse(sql);

        Assert.NotNull(stmt);
        Assert.Equal("SELECT a FROM t", stmt!.ToString());
    }

    #endregion

    #region #5 NULL AS 字段名

    [Fact]
    public void NullAsAlias_ShouldRoundTrip()
    {
        // 客户反馈 #5：不能写 NULL AS 字段名。实测正常。
        var sql = "SELECT NULL AS x FROM t";

        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        var item = select.SelectItems![0];

        Assert.IsType<NullValue>(item.Expression);
        Assert.NotNull(item.Alias);
        Assert.Equal("x", item.Alias!.Name);
        Assert.Equal("SELECT NULL AS x FROM t", select.ToString());
    }

    [Fact]
    public void NullAsAlias_MultipleColumns_ShouldRoundTrip()
    {
        var sql = "SELECT NULL AS x, NULL AS y, id FROM t";

        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        Assert.Equal(3, select.SelectItems!.Count);
        Assert.Equal("SELECT NULL AS x, NULL AS y, id FROM t", select.ToString());
    }

    #endregion

    #region #6 字符串 || 字段 拼接

    [Fact]
    public void StringConcat_LiteralAndColumn_ShouldRoundTrip()
    {
        // 客户反馈 #6：不支持 '0' || 某字段。实测正常，解析为 Concat 节点。
        var sql = "SELECT '0' || a AS x FROM t";

        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        var item = select.SelectItems![0];

        var concat = Assert.IsType<Concat>(item.Expression);
        Assert.IsType<StringValue>(concat.LeftExpression);
        Assert.Equal("0", ((StringValue)concat.LeftExpression).Value);
        Assert.Equal("a", concat.RightExpression.ToString());
        Assert.Equal("SELECT '0' || a AS x FROM t", select.ToString());
    }

    [Fact]
    public void Concat_ColumnAndLiteral_ShouldRoundTrip()
    {
        var sql = "SELECT a || 'suffix' FROM t";

        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        Assert.Equal("SELECT a || 'suffix' FROM t", select.ToString());
    }

    [Fact]
    public void Concat_MultipleOperands_ShouldRoundTrip()
    {
        var sql = "SELECT a || b || c FROM t";

        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        var item = select.SelectItems![0];

        Assert.IsType<Concat>(item.Expression);
        Assert.Equal("SELECT a || b || c FROM t", select.ToString());
    }

    #endregion

    #region #7 UNION ALL

    [Fact]
    public void UnionAll_TwoSelects_ShouldRoundTrip()
    {
        // 客户反馈 #7：不支持 UNION ALL。实测正常，解析为 SetOperationList。
        var sql = "SELECT a FROM t1 UNION ALL SELECT a FROM t2";

        var stmt = CCJSqlParserUtil.Parse(sql)!;

        var setOpList = Assert.IsType<SetOperationList>(stmt);
        Assert.Equal(2, setOpList.Selects.Count);
        Assert.Single(setOpList.Operations);
        Assert.Equal(SetOperation.OperationType.UNION, setOpList.Operations[0].Type);
        Assert.True(setOpList.Operations[0].All);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void Union_Distinct_ShouldRoundTrip()
    {
        var sql = "SELECT a FROM t1 UNION SELECT a FROM t2";

        var stmt = CCJSqlParserUtil.Parse(sql)!;

        var setOpList = Assert.IsType<SetOperationList>(stmt);
        Assert.Equal(SetOperation.OperationType.UNION, setOpList.Operations[0].Type);
        Assert.False(setOpList.Operations[0].All);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void UnionAll_ThreeSelects_ShouldRoundTrip()
    {
        var sql = "SELECT a FROM t1 UNION ALL SELECT a FROM t2 UNION ALL SELECT a FROM t3";

        var stmt = CCJSqlParserUtil.Parse(sql)!;

        var setOpList = Assert.IsType<SetOperationList>(stmt);
        Assert.Equal(3, setOpList.Selects.Count);
        Assert.Equal(2, setOpList.Operations.Count);
        Assert.Equal(sql, stmt.ToString());
    }

    #endregion

    #region #8 :: 类型转换（PostgreSQL 风格 CAST）

    [Fact]
    public void DoubleColonCast_WithDataTypeLength_ShouldRoundTrip()
    {
        // 客户反馈 #8：不支持 a.qty::varchar(20) as yl。实测正常，解析为 CastExpression（UseCastKeyword=false）。
        var sql = "SELECT a.qty::varchar(20) AS yl FROM t a";

        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        var item = select.SelectItems![0];

        var cast = Assert.IsType<CastExpression>(item.Expression);
        Assert.False(cast.UseCastKeyword);
        Assert.Equal("a.qty", cast.Expression.ToString());
        Assert.Equal("varchar(20)", cast.DataType);
        Assert.NotNull(item.Alias);
        Assert.Equal("yl", item.Alias!.Name);
        Assert.Equal("SELECT a.qty::varchar(20) AS yl FROM t a", select.ToString());
    }

    [Fact]
    public void DoubleColonCast_SimpleType_ShouldRoundTrip()
    {
        var sql = "SELECT id::bigint FROM t";

        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        Assert.Equal("SELECT id::bigint FROM t", select.ToString());
    }

    [Fact]
    public void DoubleColonCast_Text_ShouldRoundTrip()
    {
        var sql = "SELECT name::text FROM t";

        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        Assert.Equal("SELECT name::text FROM t", select.ToString());
    }

    [Fact]
    public void CastKeyword_ShouldRoundTripAsCast()
    {
        // 对照：CAST(...) 关键字形式应输出 CAST(x AS type)
        var sql = "SELECT CAST(id AS bigint) FROM t";

        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        var item = select.SelectItems![0];

        var cast = Assert.IsType<CastExpression>(item.Expression);
        Assert.True(cast.UseCastKeyword);
        Assert.Equal("SELECT CAST(id AS bigint) FROM t", select.ToString());
    }

    #endregion

    #region 综合场景（客户实际 SQL 组合）

    [Fact]
    public void Complex_RealWorldSelect_ShouldRoundTrip()
    {
        // 模拟客户实际 SQL：CASE + || + :: + NULL AS 组合
        var sql =
            "SELECT " +
            "CASE WHEN a.qty > 0 THEN '1' ELSE '0' END AS flag, " +
            "'0' || a.code AS prefixed, " +
            "a.qty::varchar(20) AS yl, " +
            "NULL AS remark " +
            "FROM orders a WHERE a.status != 1";

        var stmt = CCJSqlParserUtil.Parse(sql);

        Assert.NotNull(stmt);
        // round-trip 应保持结构完整（注意 != 会被规范化为 <>）
        var output = stmt!.ToString()!;
        Assert.Contains("CASE WHEN a.qty > 0 THEN '1' ELSE '0' END AS flag", output);
        Assert.Contains("'0' || a.code AS prefixed", output);
        Assert.Contains("a.qty::varchar(20) AS yl", output);
        Assert.Contains("NULL AS remark", output);
    }

    #endregion
}
