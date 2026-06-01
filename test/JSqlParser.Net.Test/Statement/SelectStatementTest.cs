using JSqlParser.Net.Expression;
using JSqlParser.Net.Expression.Operators.Conditional;
using JSqlParser.Net.Expression.Operators.Relational;
using JSqlParser.Net.Parser;
using JSqlParser.Net.Schema;
using JSqlParser.Net.Statement.Select;

namespace JSqlParser.Net.Test.Statement;

/// <summary>
/// SELECT 语句详细测试
/// </summary>
public class SelectStatementTest
{
    #region 基础 SELECT

    [Fact]
    public void Select_SingleTable_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM users")!;
        Assert.NotNull(select.FromItem);
    }

    [Fact]
    public void Select_MultipleColumns_ShouldHaveItems()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id, name, email FROM users")!;
        Assert.Equal(3, select.SelectItems!.Count);
    }

    [Fact]
    public void Select_Star_ShouldHaveOneItem()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT * FROM users")!;
        Assert.Single(select.SelectItems!);
    }

    [Fact]
    public void Select_Alias_ShouldHaveAlias()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id AS user_id FROM users")!;
        var item = select.SelectItems![0];
        Assert.NotNull(item.Alias);
        Assert.True(item.Alias.UseAs);
        Assert.Equal("id AS user_id", item.ToString());
    }

    [Fact]
    public void Select_TableAlias_ShouldHaveAlias()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT u.id FROM users u")!;
        var from = (Table)select.FromItem!;
        Assert.NotNull(from.Alias);
    }

    #endregion

    #region WHERE 子句

    [Fact]
    public void Select_WhereEquals_ShouldHaveWhere()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM users WHERE id = 1")!;
        Assert.NotNull(select.Where);
        Assert.IsType<EqualsTo>(select.Where);
    }

    [Fact]
    public void Select_WhereAnd_ShouldHaveAndExpression()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE id = 1 AND name = 'test'")!;
        Assert.NotNull(select.Where);
        Assert.IsType<AndExpression>(select.Where);
    }

    [Fact]
    public void Select_WhereComplex_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE (id > 10 OR name LIKE '%test%') AND status = 'active'")!;
        Assert.NotNull(select.Where);
    }

    #endregion

    #region ORDER BY

    [Fact]
    public void Select_OrderByAsc_ShouldHaveOrderBy()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM users ORDER BY id ASC")!;
        Assert.NotNull(select.OrderByElements);
        Assert.Single(select.OrderByElements!);
    }

    [Fact]
    public void Select_OrderByDesc_ShouldHaveOrderBy()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM users ORDER BY id DESC")!;
        Assert.NotNull(select.OrderByElements);
    }

    [Fact]
    public void Select_OrderByMultiple_ShouldHaveMultipleElements()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT id FROM users ORDER BY name ASC, id DESC")!;
        Assert.Equal(2, select.OrderByElements!.Count);
    }

    #endregion

    #region GROUP BY & HAVING

    [Fact]
    public void Select_GroupBy_ShouldHaveGroupBy()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT name, COUNT(*) FROM users GROUP BY name")!;
        Assert.NotNull(select.GroupBy);
    }

    [Fact]
    public void Select_GroupByMultiple_ShouldHaveGroupBy()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT name, status, COUNT(*) FROM users GROUP BY name, status")!;
        Assert.NotNull(select.GroupBy);
    }

    [Fact]
    public void Select_Having_ShouldHaveHaving()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT name, COUNT(*) c FROM users GROUP BY name HAVING c > 1")!;
        Assert.NotNull(select.Having);
    }

    #endregion

    #region LIMIT & OFFSET

    [Fact]
    public void Select_Limit_ShouldHaveLimit()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM users LIMIT 10")!;
        Assert.NotNull(select.Limit);
    }

    [Fact]
    public void Select_LimitOffset_ShouldHaveBoth()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT id FROM users LIMIT 10 OFFSET 20")!;
        Assert.NotNull(select.Limit);
        Assert.NotNull(select.Offset);
    }

    #endregion

    #region DISTINCT

    [Fact]
    public void Select_Distinct_ShouldHaveDistinct()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT DISTINCT name FROM users")!;
        Assert.NotNull(select.Distinct);
    }

    #endregion

    #region JOIN

    [Fact]
    public void Select_InnerJoin_ShouldHaveJoin()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT u.id, o.total FROM users u INNER JOIN orders o ON u.id = o.user_id")!;
        Assert.NotNull(select.Joins);
        Assert.Single(select.Joins);
    }

    [Fact]
    public void Select_LeftJoin_ShouldHaveJoin()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT u.id, o.total FROM users u LEFT JOIN orders o ON u.id = o.user_id")!;
        Assert.NotNull(select.Joins);
    }

    [Fact]
    public void Select_RightJoin_ShouldHaveJoin()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT u.id, o.total FROM users u RIGHT JOIN orders o ON u.id = o.user_id")!;
        Assert.NotNull(select.Joins);
    }

    [Fact]
    public void Select_MultipleJoins_ShouldHaveMultipleJoins()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT u.id, o.total, p.name FROM users u " +
            "INNER JOIN orders o ON u.id = o.user_id " +
            "INNER JOIN products p ON o.product_id = p.id")!;
        Assert.NotNull(select.Joins);
        Assert.Equal(2, select.Joins.Count);
    }

    [Fact]
    public void Select_CommaSeparatedTables_ShouldKeepAllFromItems()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT * FROM users, orders, products")!;

        Assert.NotNull(select.Joins);
        Assert.Equal(2, select.Joins.Count);
        Assert.All(select.Joins, join => Assert.True(join.Simple));
        Assert.Equal("SELECT * FROM users, orders, products", select.ToString());
    }

    [Fact]
    public void Join_Simple_NullRightItem_ShouldThrow()
    {
        var join = new Join { Simple = true, RightItem = null! };

        Assert.Throws<InvalidOperationException>(() => join.ToString());
    }

    [Fact]
    public void Join_Simple_WithRightItem_ShouldOutputCommaTable()
    {
        var join = new Join { Simple = true, RightItem = new Table { Name = "orders" } };
        var result = join.ToString();
        Assert.Equal(", orders", result);
    }

    #endregion

    #region 子查询

    [Fact]
    public void Select_SubqueryInFrom_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT * FROM (SELECT id, name FROM users) AS sub")!;
        Assert.NotNull(select.FromItem);
    }

    [Fact]
    public void Select_SubqueryInWhere_ShouldParse()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT id FROM users WHERE id IN (SELECT user_id FROM orders)")!;
        Assert.NotNull(select.Where);
    }

    #endregion

    #region UNION / INTERSECT / EXCEPT

    [Fact]
    public void Select_Union_ShouldBeSetOperationList()
    {
        var select = (Select)CCJSqlParserUtil.Parse("SELECT id FROM users UNION SELECT id FROM admins")!;
        Assert.IsType<SetOperationList>(select);
    }

    [Fact]
    public void Select_UnionAll_ShouldBeSetOperationList()
    {
        var select = (SetOperationList)CCJSqlParserUtil.Parse("SELECT id FROM users UNION ALL SELECT id FROM admins")!;
        Assert.Equal(2, select.Selects.Count);
    }

    [Fact]
    public void Select_Intersect_ShouldBeSetOperationList()
    {
        var select = (Select)CCJSqlParserUtil.Parse(
            "SELECT id FROM users INTERSECT SELECT id FROM admins")!;
        Assert.IsType<SetOperationList>(select);
    }

    [Fact]
    public void Select_Except_ShouldBeSetOperationList()
    {
        var select = (Select)CCJSqlParserUtil.Parse(
            "SELECT id FROM users EXCEPT SELECT id FROM admins")!;
        Assert.IsType<SetOperationList>(select);
    }

    #endregion

    #region WITH (CTE)

    [Fact]
    public void Select_WithCte_ShouldHaveWithItemsList()
    {
        var select = (Select)CCJSqlParserUtil.Parse(
            "WITH active_users AS (SELECT id, name FROM users WHERE status = 'active') SELECT * FROM active_users")!;
        Assert.NotNull(select.WithItemsList);
        Assert.Single(select.WithItemsList!);
    }

    [Fact]
    public void Select_MultipleCtes_ShouldHaveMultipleWithItems()
    {
        var select = (Select)CCJSqlParserUtil.Parse(
            "WITH cte1 AS (SELECT id FROM users), cte2 AS (SELECT id FROM orders) SELECT * FROM cte1")!;
        Assert.NotNull(select.WithItemsList);
        Assert.Equal(2, select.WithItemsList!.Count);
    }

    #endregion

    #region AllTableColumns / AnalyticExpression 类型断言

    [Fact]
    public void Select_AllTableColumns_ShouldHaveAllTableColumns()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT t.* FROM users t")!;
        var item = select.SelectItems![0];
        Assert.IsType<AllTableColumns>(item.Expression);
        var allTableCols = (AllTableColumns)item.Expression!;
        Assert.NotNull(allTableCols.Table);
        Assert.Equal("t", allTableCols.Table!.Name);
    }

    [Fact]
    public void Select_AnalyticFunction_ShouldBeAnalyticExpression()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT ROW_NUMBER() OVER(ORDER BY id) FROM users")!;
        Assert.IsType<AnalyticExpression>(select.SelectItems![0].Expression);
    }

    [Fact]
    public void Select_FunctionWithinGroup_ShouldKeepOrderBy()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY salary) FROM users")!;
        var function = Assert.IsType<Function>(select.SelectItems![0].Expression);
        Assert.NotNull(function.WithinGroupOrderByElements);
        Assert.Single(function.WithinGroupOrderByElements!);
    }

    [Fact]
    public void Select_FunctionFilter_ShouldKeepWhereExpression()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT COUNT(*) FILTER (WHERE active = TRUE) FROM users")!;
        var function = Assert.IsType<Function>(select.SelectItems![0].Expression);
        Assert.True(function.AllColumns);
        Assert.NotNull(function.FilterExpression);
        Assert.IsType<EqualsTo>(function.FilterExpression);
    }

    [Fact]
    public void Select_AnalyticFunctionFilter_ShouldKeepWhereExpression()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT COUNT(*) FILTER (WHERE active = TRUE) OVER(PARTITION BY dept_id) FROM users")!;
        var analytic = Assert.IsType<AnalyticExpression>(select.SelectItems![0].Expression);
        Assert.NotNull(analytic.FilterExpression);
        Assert.NotNull(analytic.PartitionExpressionList);
    }

    #endregion
}
