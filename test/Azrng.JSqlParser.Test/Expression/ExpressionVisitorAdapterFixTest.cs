using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using PlainSelect = Azrng.JSqlParser.Statement.Select.PlainSelect;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// ExpressionVisitorAdapter 修复（M1 context 透传 + M2 子树遍历完整性）的回归测试。
/// </summary>
public class ExpressionVisitorAdapterFixTest
{
    /// <summary>
    /// 记录所有访问到的 Column 名，用于验证子树遍历是否完整。
    /// </summary>
    private class ColumnRecorder : ExpressionVisitorAdapter<object?>
    {
        public List<string> Columns { get; } = new();

        public override object? Visit<S>(Column column, S context)
        {
            Columns.Add(column.ColumnName);
            return base.Visit(column, context);
        }
    }

    /// <summary>
    /// 记录子树访问时收到的 context，用于验证 context 透传一致性（M1）。
    /// </summary>
    private class ContextRecorder : ExpressionVisitorAdapter<string?>
    {
        public List<string> SeenContexts { get; } = new();

        public override string? Visit<S>(Column column, S context)
        {
            if (context is string s) SeenContexts.Add(s);
            return null;
        }
    }

    // ===== M2: Function 子树遍历完整性 =====

    /// <summary>FILTER 子句内的列应被遍历到（M2 修复前被漏掉）。
    /// COUNT(*) FILTER(WHERE x) 解析为 AnalyticExpression，FilterExpression 含 filter_col。</summary>
    [Fact]
    public void M2_Function_FilterClause_Traversed()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT COUNT(*) FILTER (WHERE filter_col > 0) FROM t")!;
        // FILTER 包装为 AnalyticExpression，遍历它应覆盖 FilterExpression
        var analytic = select.SelectItems![0].Expression;
        var visitor = new ColumnRecorder();

        analytic.Accept(visitor, (object?)null);

        Assert.Contains("filter_col", visitor.Columns);
    }

    /// <summary>AnalyticExpression 内的列应被遍历到（M2 修复前完全不遍历）。</summary>
    [Fact]
    public void M2_AnalyticExpression_PartitionAndOrderBy_Traversed()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT SUM(measure) OVER (PARTITION BY part_col ORDER BY ord_col) FROM t")!;
        var analytic = (AnalyticExpression)select.SelectItems![0].Expression;
        var visitor = new ColumnRecorder();

        analytic.Accept(visitor, (object?)null);

        Assert.Contains("part_col", visitor.Columns);
        Assert.Contains("ord_col", visitor.Columns);
        Assert.Contains("measure", visitor.Columns);
    }

    /// <summary>二元运算符左右操作数的列都应被遍历。</summary>
    [Fact]
    public void M1_BinaryExpression_BothOperands_Traversed()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("left_col = right_col")!;
        var visitor = new ColumnRecorder();

        expr.Accept(visitor, (object?)null);

        Assert.Contains("left_col", visitor.Columns);
        Assert.Contains("right_col", visitor.Columns);
    }

    /// <summary>Parenthesis 内的列应被遍历（M1 修复前丢 context）。</summary>
    [Fact]
    public void M1_Parenthesis_InnerExpression_Traversed()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("(inner_col > 0)")!;
        var visitor = new ColumnRecorder();

        expr.Accept(visitor, (object?)null);

        Assert.Contains("inner_col", visitor.Columns);
    }

    /// <summary>NotExpression 内的列应被遍历。</summary>
    [Fact]
    public void M1_NotExpression_InnerExpression_Traversed()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("NOT neg_col = 1")!;
        var visitor = new ColumnRecorder();

        expr.Accept(visitor, (object?)null);

        Assert.Contains("neg_col", visitor.Columns);
    }

    /// <summary>深层嵌套 AND/OR 表达式所有列都应被遍历（验证递归 context 透传）。</summary>
    [Fact]
    public void M1_DeepNested_AndOr_Traversed()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("a = 1 AND (b = 2 OR c = 3) AND d = 4")!;
        var visitor = new ColumnRecorder();

        expr.Accept(visitor, (object?)null);

        foreach (var expected in new[] { "a", "b", "c", "d" })
            Assert.Contains(expected, visitor.Columns);
    }

    /// <summary>CaseExpression 的 WHEN/THEN/ELSE 子表达式都应被遍历（M2 补全）。</summary>
    [Fact]
    public void M2_CaseExpression_AllBranches_Traversed()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression(
            "CASE WHEN cond_col > 0 THEN then_col ELSE else_col END")!;
        var visitor = new ColumnRecorder();

        expr.Accept(visitor, (object?)null);

        Assert.Contains("cond_col", visitor.Columns);
        Assert.Contains("then_col", visitor.Columns);
        Assert.Contains("else_col", visitor.Columns);
    }

    // ===== M1: context 透传一致性 =====

    /// <summary>子树遍历时 context 应保持一致透传（M1 修复前 Accept(this) 丢 context）。</summary>
    [Fact]
    public void M1_Context_PreservedThroughSubtree()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("a = 1 AND b = 2")!;
        var visitor = new ContextRecorder();

        expr.Accept(visitor, "MY_CTX");

        // 所有 Column 访问都应收到 "MY_CTX"，而非 default(null)
        Assert.NotEmpty(visitor.SeenContexts);
        Assert.All(visitor.SeenContexts, c => Assert.Equal("MY_CTX", c));
    }

    /// <summary>深层嵌套表达式的 context 也应一致透传。</summary>
    [Fact]
    public void M1_Context_PreservedInDeepNesting()
    {
        var expr = CCJSqlParserUtil.ParseCondExpression("(a = 1 AND (b = 2 OR c = 3))")!;
        var visitor = new ContextRecorder();

        expr.Accept(visitor, "DEEP_CTX");

        Assert.NotEmpty(visitor.SeenContexts);
        Assert.All(visitor.SeenContexts, c => Assert.Equal("DEEP_CTX", c));
    }
}
