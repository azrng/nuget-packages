using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// CASE 表达式测试。
///
/// 重点覆盖 searched 形式（CASE WHEN cond THEN val）的 round-trip 序列化，
/// 这是客户迁移反馈的 Bug 项：原实现把 searched 形式错误序列化成
/// "CASE 'small' WHEN a &gt; 1 THEN 'big' ELSE 'small' END"（语义错误的 SQL），
/// 已在 AstBuilderVisitor.VisitCaseExpr 修复。
/// </summary>
public class CaseExpressionTest
{
    /// <summary>
    /// 解析 SQL 并返回首个 SelectItem 的表达式（断言为 CaseExpression）。
    /// </summary>
    private static CaseExpression ParseFirstCase(string sql)
    {
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var item = select.SelectItems![0];
        return (CaseExpression)item.Expression!;
    }

    #region searched 形式（CASE WHEN cond THEN val）—— 回归重点

    [Fact]
    public void Searched_SingleWhenElse_ShouldRoundTrip()
    {
        const string sql = "SELECT CASE WHEN a > 1 THEN 'big' ELSE 'small' END AS r FROM t";

        var caseExpr = ParseFirstCase(sql);

        Assert.Null(caseExpr.SwitchExpression);
        Assert.Single(caseExpr.WhenClauses);
        Assert.NotNull(caseExpr.WhenClauses[0].WhenExpression);
        Assert.Equal("a > 1", caseExpr.WhenClauses[0].WhenExpression!.ToString());
        Assert.Equal("'big'", caseExpr.WhenClauses[0].ThenExpression!.ToString());
        Assert.NotNull(caseExpr.ElseExpression);
        Assert.Equal("'small'", caseExpr.ElseExpression!.ToString());

        // 关键回归断言：CASE 表达式本体 round-trip 必须等价，
        // 不得错位成 CASE 'small' WHEN a > 1 THEN 'big' ELSE 'small' END（语义错误）
        Assert.Equal(
            "CASE WHEN a > 1 THEN 'big' ELSE 'small' END",
            caseExpr.ToString());
    }

    [Fact]
    public void Searched_MultipleWhen_ShouldRoundTrip()
    {
        const string sql =
            "SELECT CASE WHEN a > 1 THEN 'big' WHEN a > 0 THEN 'mid' ELSE 'small' END AS r FROM t";

        var caseExpr = ParseFirstCase(sql);

        Assert.Null(caseExpr.SwitchExpression);
        Assert.Equal(2, caseExpr.WhenClauses.Count);
        Assert.NotNull(caseExpr.ElseExpression);

        Assert.Equal(
            "SELECT CASE WHEN a > 1 THEN 'big' WHEN a > 0 THEN 'mid' ELSE 'small' END AS r FROM t",
            SqlParser.Parse(sql)!.ToString());
    }

    [Fact]
    public void Searched_NoElse_ShouldRoundTrip()
    {
        const string sql = "SELECT CASE WHEN a > 1 THEN 'big' END AS r FROM t";

        var caseExpr = ParseFirstCase(sql);

        Assert.Null(caseExpr.SwitchExpression);
        Assert.Single(caseExpr.WhenClauses);
        Assert.Null(caseExpr.ElseExpression);
        Assert.Equal(
            "SELECT CASE WHEN a > 1 THEN 'big' END AS r FROM t",
            SqlParser.Parse(sql)!.ToString());
    }

    [Fact]
    public void Searched_FullStatementRoundTrip_ShouldBeEquivalent()
    {
        // 整条语句 round-trip（含 FROM/WHERE）也必须保持 CASE 结构正确
        const string sql =
            "SELECT CASE WHEN status = 1 THEN 'active' ELSE 'inactive' END FROM users WHERE id > 0";

        var parsed = SqlParser.Parse(sql)!;

        Assert.Equal(
            "SELECT CASE WHEN status = 1 THEN 'active' ELSE 'inactive' END FROM users WHERE id > 0",
            parsed.ToString());
    }

    #endregion

    #region switch 形式（CASE expr WHEN val THEN result）—— 保证不回归

    [Fact]
    public void Switch_SingleWhenElse_ShouldRoundTrip()
    {
        const string sql = "SELECT CASE a WHEN 1 THEN 'one' ELSE 'other' END AS r FROM t";

        var caseExpr = ParseFirstCase(sql);

        // switch 形式必须有 SwitchExpression
        Assert.NotNull(caseExpr.SwitchExpression);
        Assert.Equal("a", caseExpr.SwitchExpression!.ToString());
        Assert.Single(caseExpr.WhenClauses);
        Assert.Equal("1", caseExpr.WhenClauses[0].WhenExpression!.ToString());
        Assert.Equal("'one'", caseExpr.WhenClauses[0].ThenExpression!.ToString());
        Assert.NotNull(caseExpr.ElseExpression);

        Assert.Equal(
            "CASE a WHEN 1 THEN 'one' ELSE 'other' END",
            caseExpr.ToString());
    }

    [Fact]
    public void Switch_MultipleWhen_ShouldRoundTrip()
    {
        const string sql =
            "SELECT CASE a WHEN 1 THEN 'one' WHEN 2 THEN 'two' ELSE 'other' END AS r FROM t";

        var caseExpr = ParseFirstCase(sql);

        Assert.NotNull(caseExpr.SwitchExpression);
        Assert.Equal(2, caseExpr.WhenClauses.Count);
        Assert.Equal(
            "SELECT CASE a WHEN 1 THEN 'one' WHEN 2 THEN 'two' ELSE 'other' END AS r FROM t",
            SqlParser.Parse(sql)!.ToString());
    }

    #endregion

    #region 嵌套与复杂形式

    [Fact]
    public void Nested_CaseInThen_ShouldRoundTrip()
    {
        // THEN 子句中嵌套 CASE
        const string sql =
            "SELECT CASE WHEN a > 1 THEN CASE WHEN b = 2 THEN 'x' ELSE 'y' END ELSE 'z' END AS r FROM t";

        var caseExpr = ParseFirstCase(sql);

        Assert.Null(caseExpr.SwitchExpression);
        Assert.Single(caseExpr.WhenClauses);
        var nested = caseExpr.WhenClauses[0].ThenExpression;
        Assert.IsType<CaseExpression>(nested);

        Assert.Equal(
            "SELECT CASE WHEN a > 1 THEN CASE WHEN b = 2 THEN 'x' ELSE 'y' END ELSE 'z' END AS r FROM t",
            SqlParser.Parse(sql)!.ToString());
    }

    [Fact]
    public void Searched_InWhereClause_ShouldRoundTrip()
    {
        // CASE 出现在 WHERE 子句中（而非 SELECT 列）
        const string sql =
            "SELECT a FROM t WHERE CASE WHEN a > 1 THEN 1 ELSE 0 END = 1";

        var parsed = SqlParser.Parse(sql)!;
        var where = ((PlainSelect)parsed).Where!;
        Assert.Contains("CASE WHEN a > 1 THEN 1 ELSE 0 END", parsed.ToString()!);
    }

    #endregion

    #region 表达式解析入口（ParseExpression）

    [Fact]
    public void ParseExpression_SearchedCase_ShouldWork()
    {
        var expr = (CaseExpression)SqlParser.ParseExpression(
            "CASE WHEN x THEN 1 ELSE 2 END")!;

        Assert.Null(expr.SwitchExpression);
        Assert.Single(expr.WhenClauses);
        Assert.NotNull(expr.ElseExpression);
        Assert.Equal("CASE WHEN x THEN 1 ELSE 2 END", expr.ToString());
    }

    [Fact]
    public void ParseExpression_SwitchCase_ShouldWork()
    {
        var expr = (CaseExpression)SqlParser.ParseExpression(
            "CASE x WHEN 1 THEN 'a' ELSE 'b' END")!;

        Assert.NotNull(expr.SwitchExpression);
        Assert.Equal("x", expr.SwitchExpression!.ToString());
        Assert.Equal("CASE x WHEN 1 THEN 'a' ELSE 'b' END", expr.ToString());
    }

    #endregion
}
