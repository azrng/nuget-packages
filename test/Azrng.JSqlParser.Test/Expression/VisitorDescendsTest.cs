using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// visitor 子节点下钻完整性测试（批次3）。
///
/// 此前 ExpressionVisitorAdapter 与 ExpressionDescendantsWalker 对若干 BinaryExpression 子类
/// 的 Visit 是空实现（return default），不下钻 LeftExpression/RightExpression，
/// 导致 TablesNamesFinder/Descendants/自定义 visitor 漏掉这些节点下的子表达式（表名、列引用、子查询）。
/// 与 Matches/RegExpMatchOperator 之前的 bug 同源（迁移遗漏）。
/// </summary>
public class VisitorDescendsTest
{
    private sealed class ColumnRecorder : ExpressionVisitorAdapter<object?>
    {
        public List<string> Columns { get; } = new();
        public override object? Visit<S>(Column column, S context)
        {
            Columns.Add(column.ColumnName);
            return base.Visit(column, context);
        }
    }

    /// <summary>
    /// 对一个手工构造的表达式跑 ColumnRecorder，返回收集到的列名（去重保序）。
    /// </summary>
    private static List<string> CollectColumns(IExpression expr)
    {
        var recorder = new ColumnRecorder();
        expr.Accept(recorder, (object?)null);
        return recorder.Columns;
    }

    private static readonly IExpression ColX = new Column { ColumnName = "x" };
    private static readonly IExpression ColY = new Column { ColumnName = "y" };

    #region ExpressionVisitorAdapter 下钻完整性

    [Fact]
    public void Adapter_IsBooleanExpression_ShouldDescendLeft()
    {
        // IS TRUE / IS FALSE 此前 Visit 不下钻，LeftExpression 中的列引用被漏
        var expr = new IsBooleanExpression { LeftExpression = ColX, IsTrue = true };
        Assert.Equal(new[] { "x" }, CollectColumns(expr));
    }

    [Fact]
    public void Adapter_IsDistinctExpression_ShouldDescendBoth()
    {
        var expr = new IsDistinctExpression { LeftExpression = ColX, RightExpression = ColY };
        Assert.Equal(new[] { "x", "y" }, CollectColumns(expr));
    }

    [Fact]
    public void Adapter_JsonOperator_ShouldDescendBoth()
    {
        var expr = new JsonOperator("->") { LeftExpression = ColX, RightExpression = ColY };
        Assert.Equal(new[] { "x", "y" }, CollectColumns(expr));
    }

    [Fact]
    public void Adapter_Contains_ShouldDescendBoth()
    {
        var expr = new Contains { LeftExpression = ColX, RightExpression = ColY };
        Assert.Equal(new[] { "x", "y" }, CollectColumns(expr));
    }

    [Fact]
    public void Adapter_ContainedBy_ShouldDescendBoth()
    {
        var expr = new ContainedBy { LeftExpression = ColX, RightExpression = ColY };
        Assert.Equal(new[] { "x", "y" }, CollectColumns(expr));
    }

    [Fact]
    public void Adapter_Matches_ShouldDescendBoth()
    {
        var expr = new Matches { LeftExpression = ColX, RightExpression = ColY };
        Assert.Equal(new[] { "x", "y" }, CollectColumns(expr));
    }

    [Fact]
    public void Adapter_RegExpMatchOperator_ShouldDescendBoth()
    {
        var expr = new RegExpMatchOperator(RegExpMatchOperatorType.MatchCaseSensitive)
        {
            LeftExpression = ColX,
            RightExpression = ColY
        };
        Assert.Equal(new[] { "x", "y" }, CollectColumns(expr));
    }

    #endregion

    #region ExpressionDescendantsWalker 下钻完整性（通过 Descendants 扩展）

    [Fact]
    public void Descendants_IsBooleanExpression_ShouldCollectLeafColumns()
    {
        var expr = new IsBooleanExpression { LeftExpression = ColX, IsTrue = true };
        // Descendants<Column> 内部走 ExpressionDescendantsWalker
        var cols = expr.Descendants<Column>().Select(c => c.ColumnName).ToList();
        Assert.Equal(new[] { "x" }, cols);
    }

    [Fact]
    public void Descendants_IsDistinctExpression_ShouldCollectBothColumns()
    {
        var expr = new IsDistinctExpression { LeftExpression = ColX, RightExpression = ColY };
        var cols = expr.Descendants<Column>().Select(c => c.ColumnName).ToList();
        Assert.Equal(new[] { "x", "y" }, cols);
    }

    [Fact]
    public void Descendants_JsonOperator_ShouldCollectBothColumns()
    {
        var expr = new JsonOperator("->") { LeftExpression = ColX, RightExpression = ColY };
        var cols = expr.Descendants<Column>().Select(c => c.ColumnName).ToList();
        Assert.Equal(new[] { "x", "y" }, cols);
    }

    [Fact]
    public void Descendants_Contains_ShouldCollectBothColumns()
    {
        var expr = new Contains { LeftExpression = ColX, RightExpression = ColY };
        var cols = expr.Descendants<Column>().Select(c => c.ColumnName).ToList();
        Assert.Equal(new[] { "x", "y" }, cols);
    }

    [Fact]
    public void Descendants_ContainedBy_ShouldCollectBothColumns()
    {
        var expr = new ContainedBy { LeftExpression = ColX, RightExpression = ColY };
        var cols = expr.Descendants<Column>().Select(c => c.ColumnName).ToList();
        Assert.Equal(new[] { "x", "y" }, cols);
    }

    [Fact]
    public void Descendants_Matches_ShouldCollectBothColumns()
    {
        // Matches 此前在 ExpressionDescendantsWalker 也漏下钻（与 Adapter 修复不同步）
        var expr = new Matches { LeftExpression = ColX, RightExpression = ColY };
        var cols = expr.Descendants<Column>().Select(c => c.ColumnName).ToList();
        Assert.Equal(new[] { "x", "y" }, cols);
    }

    [Fact]
    public void Descendants_RegExpMatchOperator_ShouldCollectBothColumns()
    {
        var expr = new RegExpMatchOperator(RegExpMatchOperatorType.MatchCaseSensitive)
        {
            LeftExpression = ColX,
            RightExpression = ColY
        };
        var cols = expr.Descendants<Column>().Select(c => c.ColumnName).ToList();
        Assert.Equal(new[] { "x", "y" }, cols);
    }

    #endregion
}
