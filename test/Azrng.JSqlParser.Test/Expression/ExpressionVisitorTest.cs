using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Cnf;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// IExpressionVisitor 模式测试
/// </summary>
public class ExpressionVisitorTest
{
    private class ColumnCollector : ExpressionVisitorAdapter<object?>
    {
        public int ColumnCount { get; private set; }

        public override object? Visit<S>(Column column, S context)
        {
            ColumnCount++;
            return base.Visit(column, context);
        }
    }

    [Fact]
    public void ExpressionVisitorAdapter_ShouldBeInstantiable()
    {
        var visitor = new ExpressionVisitorAdapter<object?>();
        Assert.NotNull(visitor);
    }

    [Fact]
    public void ExpressionVisitorAdapter_WhereColumns_ShouldCollect()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE name = 'test' AND age > 18")!;
        var visitor = new ColumnCollector();
        select.Where!.Accept(visitor);
        Assert.True(visitor.ColumnCount > 0);
    }

    [Fact]
    public void Expression_Accept_ShouldNotThrow()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE id = 1 AND name LIKE '%test%'")!;
        var visitor = new ColumnCollector();
        select.Where!.Accept(visitor);
        Assert.NotNull(visitor);
    }

    [Fact]
    public void ExpressionVisitorAdapter_NotExpression_ShouldVisitInnerColumn()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE NOT name = 'test'")!;
        var visitor = new ColumnCollector();

        select.Where!.Accept(visitor);

        Assert.Equal(1, visitor.ColumnCount);
    }

    [Fact]
    public void ExpressionVisitorAdapter_FunctionParameters_ShouldVisitColumns()
    {
        var expr = CCJSqlParserUtil.ParseExpression("COALESCE(name, fallback_name)")!;
        var visitor = new ColumnCollector();

        expr.Accept(visitor, (object?)null);

        Assert.Equal(2, visitor.ColumnCount);
    }

    [Fact]
    public void ExpressionVisitorAdapter_MultiAndExpression_ShouldVisitChildren()
    {
        var expr = new MultiAndExpression(
            CCJSqlParserUtil.ParseExpression("a = 1")!,
            CCJSqlParserUtil.ParseExpression("b = 2")!);
        var visitor = new ColumnCollector();

        expr.Accept(visitor, (object?)null);

        Assert.Equal(2, visitor.ColumnCount);
    }

    [Fact]
    public void ExpressionVisitorAdapter_MultiAndExpression_ThreeChildren_ShouldVisitAll()
    {
        var expr = new MultiAndExpression(
            CCJSqlParserUtil.ParseExpression("a = 1")!,
            CCJSqlParserUtil.ParseExpression("b = 2")!,
            CCJSqlParserUtil.ParseExpression("c = 3")!);
        var visitor = new ColumnCollector();

        expr.Accept(visitor, (object?)null);

        Assert.Equal(3, visitor.ColumnCount);
    }

    [Fact]
    public void ExpressionVisitorAdapter_MultiAndExpression_Empty_ShouldNotThrow()
    {
        var expr = new MultiAndExpression();
        var visitor = new ColumnCollector();

        var result = expr.Accept(visitor, (object?)null);

        Assert.Null(result);
        Assert.Equal(0, visitor.ColumnCount);
    }

    [Fact]
    public void ExpressionVisitorAdapter_ExpressionList_ShouldVisitAllChildren()
    {
        var expr = new ExpressionList
        {
            Expressions =
            [
                CCJSqlParserUtil.ParseExpression("a")!,
                CCJSqlParserUtil.ParseExpression("b")!,
                CCJSqlParserUtil.ParseExpression("c")!
            ]
        };
        var visitor = new ColumnCollector();

        expr.Accept(visitor, (object?)null);

        Assert.Equal(3, visitor.ColumnCount);
    }

    [Fact]
    public void ExpressionVisitorAdapter_ExpressionList_Empty_ShouldNotThrow()
    {
        var expr = new ExpressionList();
        var visitor = new ColumnCollector();

        var result = expr.Accept(visitor, (object?)null);

        Assert.Null(result);
        Assert.Equal(0, visitor.ColumnCount);
    }

    [Fact]
    public void ExpressionVisitorAdapter_ExpressionList_ViaInClause_ShouldVisitColumns()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE status IN (1, 2, 3)")!;
        var visitor = new ColumnCollector();

        select.Where!.Accept(visitor);

        Assert.Equal(1, visitor.ColumnCount); // status column
    }
}
