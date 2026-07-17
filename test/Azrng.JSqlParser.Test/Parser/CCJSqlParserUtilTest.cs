using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;
using Insert = Azrng.JSqlParser.Statement.Insert.Insert;
using Update = Azrng.JSqlParser.Statement.Update.Update;
using Delete = Azrng.JSqlParser.Statement.Delete.Delete;
using CreateTable = Azrng.JSqlParser.Statement.CreateTable.CreateTable;
using Drop = Azrng.JSqlParser.Statement.Drop.Drop;
using Truncate = Azrng.JSqlParser.Statement.Truncate.Truncate;

namespace Azrng.JSqlParser.Test.Parser;

/// <summary>
/// SqlParser 入口测试
/// </summary>
public class CCJSqlParserUtilTest
{
    #region parse(string) - 基础解析

    [Fact]
    public void Parse_SelectAll_ShouldReturnPlainSelect()
    {
        var stmt = SqlParser.Parse("SELECT * FROM users");
        Assert.NotNull(stmt);
        Assert.IsType<PlainSelect>(stmt);
    }

    [Fact]
    public void Parse_SelectWithSchema_ShouldReturnPlainSelect()
    {
        var stmt = SqlParser.Parse("SELECT id FROM mydb.users");
        Assert.NotNull(stmt);
        Assert.IsType<PlainSelect>(stmt);
    }

    [Fact]
    public void Parse_SelectWithAlias_ShouldReturnPlainSelect()
    {
        var stmt = SqlParser.Parse("SELECT u.id, u.name FROM users u");
        Assert.NotNull(stmt);
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.IFromItem);
    }

    [Fact]
    public void Parse_SelectWithMultipleColumns_ShouldHaveCorrectItemCount()
    {
        var stmt = SqlParser.Parse("SELECT a, b, c, d FROM t");
        var select = (PlainSelect)stmt!;
        Assert.Equal(4, select.SelectItems!.Count);
    }

    [Fact]
    public void Parse_SelectWithWhereEquals_ShouldHaveWhere()
    {
        var stmt = SqlParser.Parse("SELECT id FROM users WHERE id = 1");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Parse_SelectWithWhereAnd_ShouldHaveWhere()
    {
        var stmt = SqlParser.Parse("SELECT id FROM users WHERE id = 1 AND name = 'test'");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Parse_SelectWithWhereOr_ShouldHaveWhere()
    {
        var stmt = SqlParser.Parse("SELECT id FROM users WHERE id = 1 OR id = 2");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Parse_SelectWithIn_ShouldHaveWhere()
    {
        var stmt = SqlParser.Parse("SELECT id FROM users WHERE id IN (1, 2, 3)");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Parse_SelectWithBetween_ShouldHaveWhere()
    {
        var stmt = SqlParser.Parse("SELECT id FROM users WHERE age BETWEEN 18 AND 60");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Parse_SelectWithLike_ShouldHaveWhere()
    {
        var stmt = SqlParser.Parse("SELECT id FROM users WHERE name LIKE '%test%'");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Parse_SelectWithIsNull_ShouldHaveWhere()
    {
        var stmt = SqlParser.Parse("SELECT id FROM users WHERE deleted_at IS NULL");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Parse_SelectWithIsNotNull_ShouldHaveWhere()
    {
        var stmt = SqlParser.Parse("SELECT id FROM users WHERE deleted_at IS NOT NULL");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Where);
    }

    #endregion

    #region parse(string) - JOIN

    [Fact]
    public void Parse_InnerJoin_ShouldHaveJoins()
    {
        var stmt = SqlParser.Parse(
            "SELECT a.id FROM users a INNER JOIN orders b ON a.id = b.user_id");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Joins);
        Assert.Single(select.Joins);
    }

    [Fact]
    public void Parse_LeftJoin_ShouldHaveJoins()
    {
        var stmt = SqlParser.Parse(
            "SELECT a.id FROM users a LEFT JOIN orders b ON a.id = b.user_id");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Joins);
    }

    [Fact]
    public void Parse_RightJoin_ShouldHaveJoins()
    {
        var stmt = SqlParser.Parse(
            "SELECT a.id FROM users a RIGHT JOIN orders b ON a.id = b.user_id");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Joins);
    }

    [Fact]
    public void Parse_FullJoin_ShouldHaveJoins()
    {
        var stmt = SqlParser.Parse(
            "SELECT a.id FROM users a FULL JOIN orders b ON a.id = b.user_id");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Joins);
    }

    [Fact]
    public void Parse_CrossJoin_ShouldHaveJoins()
    {
        var stmt = SqlParser.Parse(
            "SELECT a.id FROM users a CROSS JOIN orders b");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Joins);
    }

    [Fact]
    public void Parse_MultipleJoins_ShouldHaveMultipleJoins()
    {
        var stmt = SqlParser.Parse(
            "SELECT a.id FROM a INNER JOIN b ON a.id = b.aid INNER JOIN c ON b.id = c.bid");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Joins);
        Assert.Equal(2, select.Joins.Count);
    }

    #endregion

    #region parse(string) - 子查询

    [Fact]
    public void Parse_SubQueryInFrom_ShouldReturnPlainSelect()
    {
        var stmt = SqlParser.Parse("SELECT * FROM (SELECT id FROM users) AS t");
        Assert.NotNull(stmt);
        Assert.IsType<PlainSelect>(stmt);
    }

    [Fact]
    public void Parse_SubQueryInWhere_ShouldHaveWhere()
    {
        var stmt = SqlParser.Parse(
            "SELECT id FROM users WHERE id IN (SELECT user_id FROM orders)");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Parse_ExistsSubQuery_ShouldHaveWhere()
    {
        var stmt = SqlParser.Parse(
            "SELECT id FROM users WHERE EXISTS (SELECT 1 FROM orders WHERE orders.user_id = users.id)");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Where);
    }

    #endregion

    #region parse(string) - UNION / INTERSECT / EXCEPT

    [Fact]
    public void Parse_Union_ShouldReturnSetOperationList()
    {
        var stmt = SqlParser.Parse("SELECT id FROM a UNION SELECT id FROM b");
        Assert.IsType<SetOperationList>(stmt);
    }

    [Fact]
    public void Parse_UnionAll_ShouldReturnSetOperationList()
    {
        var stmt = SqlParser.Parse("SELECT id FROM a UNION ALL SELECT id FROM b");
        Assert.IsType<SetOperationList>(stmt);
    }

    [Fact]
    public void Parse_Intersect_ShouldReturnSetOperationList()
    {
        var stmt = SqlParser.Parse("SELECT id FROM a INTERSECT SELECT id FROM b");
        Assert.IsType<SetOperationList>(stmt);
    }

    [Fact]
    public void Parse_Except_ShouldReturnSetOperationList()
    {
        var stmt = SqlParser.Parse("SELECT id FROM a EXCEPT SELECT id FROM b");
        Assert.IsType<SetOperationList>(stmt);
    }

    [Fact]
    public void Parse_MultipleUnion_ShouldReturnSetOperationList()
    {
        var stmt = SqlParser.Parse(
            "SELECT id FROM a UNION SELECT id FROM b UNION SELECT id FROM c");
        Assert.IsType<SetOperationList>(stmt);
        var setOp = (SetOperationList)stmt!;
        Assert.Equal(3, setOp.Selects.Count);
    }

    #endregion

    #region parse(string) - ORDER BY / GROUP BY / HAVING / LIMIT

    [Fact]
    public void Parse_OrderBy_ShouldHaveOrderBy()
    {
        var stmt = SqlParser.Parse("SELECT id FROM users ORDER BY name");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.OrderByElements);
    }

    [Fact]
    public void Parse_OrderByDesc_ShouldHaveOrderBy()
    {
        var stmt = SqlParser.Parse("SELECT id FROM users ORDER BY name DESC");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.OrderByElements);
    }

    [Fact]
    public void Parse_OrderByMultiple_ShouldHaveMultipleElements()
    {
        var stmt = SqlParser.Parse("SELECT id FROM users ORDER BY name ASC, id DESC");
        var select = (PlainSelect)stmt!;
        Assert.Equal(2, select.OrderByElements!.Count);
    }

    [Fact]
    public void Parse_GroupBy_ShouldHaveGroupBy()
    {
        var stmt = SqlParser.Parse("SELECT dept, COUNT(*) FROM users GROUP BY dept");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.GroupBy);
    }

    [Fact]
    public void Parse_Having_ShouldHaveHaving()
    {
        var stmt = SqlParser.Parse(
            "SELECT dept, COUNT(*) FROM users GROUP BY dept HAVING COUNT(*) > 5");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Having);
    }

    [Fact]
    public void Parse_Limit_ShouldHaveLimit()
    {
        var stmt = SqlParser.Parse("SELECT id FROM users LIMIT 10");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Limit);
    }

    [Fact]
    public void Parse_LimitOffset_ShouldHaveLimitAndOffset()
    {
        var stmt = SqlParser.Parse("SELECT id FROM users LIMIT 10 OFFSET 20");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Limit);
        Assert.NotNull(select.Offset);
    }

    #endregion

    #region parse(string) - DISTINCT / TOP

    [Fact]
    public void Parse_Distinct_ShouldHaveDistinct()
    {
        var stmt = SqlParser.Parse("SELECT DISTINCT name FROM users");
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.Distinct);
    }

    #endregion

    #region parse(string) - DML 语句

    [Fact]
    public void Parse_Insert_ShouldReturnInsert()
    {
        var stmt = SqlParser.Parse("INSERT INTO users (id, name) VALUES (1, 'test')");
        Assert.IsType<Insert>(stmt);
    }

    [Fact]
    public void Parse_InsertSelect_ShouldReturnInsert()
    {
        var stmt = SqlParser.Parse("INSERT INTO archive (id) SELECT id FROM users");
        Assert.IsType<Insert>(stmt);
    }

    [Fact]
    public void Parse_Update_ShouldReturnUpdate()
    {
        var stmt = SqlParser.Parse("UPDATE users SET name = 'test' WHERE id = 1");
        Assert.IsType<Update>(stmt);
    }

    [Fact]
    public void Parse_UpdateMultipleSet_ShouldReturnUpdate()
    {
        var stmt = SqlParser.Parse("UPDATE users SET name = 'test', age = 20 WHERE id = 1");
        Assert.IsType<Update>(stmt);
    }

    [Fact]
    public void Parse_Delete_ShouldReturnDelete()
    {
        var stmt = SqlParser.Parse("DELETE FROM users WHERE id = 1");
        Assert.IsType<Delete>(stmt);
    }

    [Fact]
    public void Parse_DeleteWithoutWhere_ShouldReturnDelete()
    {
        var stmt = SqlParser.Parse("DELETE FROM users");
        Assert.IsType<Delete>(stmt);
    }

    #endregion

    #region parse(string) - DDL 语句

    [Fact]
    public void Parse_CreateTable_ShouldReturnCreateTable()
    {
        var stmt = SqlParser.Parse(
            "CREATE TABLE users (id INT PRIMARY KEY, name VARCHAR(100))");
        Assert.NotNull(stmt);
        Assert.IsType<CreateTable>(stmt);
    }

    [Fact]
    public void Parse_DropTable_ShouldReturnDrop()
    {
        var stmt = SqlParser.Parse("DROP TABLE users");
        Assert.IsType<Drop>(stmt);
    }

    [Fact]
    public void Parse_Truncate_ShouldReturnTruncate()
    {
        var stmt = SqlParser.Parse("TRUNCATE TABLE users");
        Assert.IsType<Truncate>(stmt);
    }

    #endregion

    #region parse(string) - WITH (CTE)

    [Fact]
    public void Parse_WithCte_ShouldHaveWithItems()
    {
        var stmt = SqlParser.Parse(
            "WITH cte AS (SELECT id FROM users) SELECT * FROM cte");
        Assert.IsType<PlainSelect>(stmt);
        var select = (PlainSelect)stmt!;
        Assert.NotNull(select.WithItemsList);
    }

    #endregion

    #region parse(string) - 函数

    [Fact]
    public void Parse_CountFunction_ShouldReturnPlainSelect()
    {
        var stmt = SqlParser.Parse("SELECT COUNT(*) FROM users");
        Assert.NotNull(stmt);
    }

    [Fact]
    public void Parse_SumFunction_ShouldReturnPlainSelect()
    {
        var stmt = SqlParser.Parse("SELECT SUM(amount) FROM orders");
        Assert.NotNull(stmt);
    }

    [Fact]
    public void Parse_CaseExpression_ShouldReturnPlainSelect()
    {
        var stmt = SqlParser.Parse(
            "SELECT CASE WHEN id = 1 THEN 'one' ELSE 'other' END FROM users");
        Assert.NotNull(stmt);
    }

    #endregion

    #region ParseExpression

    [Fact]
    public void ParseExpression_SimpleComparison_ShouldReturnExpression()
    {
        var expr = SqlParser.ParseExpression("id = 1");
        Assert.NotNull(expr);
    }

    [Fact]
    public void ParseExpression_Null_ShouldReturnNull()
    {
        var expr = SqlParser.ParseExpression((string?)null);
        Assert.Null(expr);
    }

    [Fact]
    public void ParseExpression_Empty_ShouldReturnNull()
    {
        var expr = SqlParser.ParseExpression("");
        Assert.Null(expr);
    }

    [Fact]
    public void ParseExpression_TrailingTokens_ShouldThrow()
    {
        Assert.Throws<JSqlParserException>(() => SqlParser.ParseExpression("id = 1 trailing"));
    }

    #endregion

    #region ParseCondExpression

    [Fact]
    public void ParseCondExpression_Simple_ShouldReturnExpression()
    {
        var expr = SqlParser.ParseCondExpression("id = 1 AND name = 'test'");
        Assert.NotNull(expr);
    }

    [Fact]
    public void ParseCondExpression_Null_ShouldReturnNull()
    {
        var expr = SqlParser.ParseCondExpression(null);
        Assert.Null(expr);
    }

    #endregion

    #region ParseStatements

    [Fact]
    public void ParseStatements_MultipleStatements_ShouldReturnStatements()
    {
        var sqls = "SELECT id FROM users; SELECT name FROM orders;";
        var stmts = SqlParser.ParseStatements(sqls);
        Assert.NotNull(stmts);
        Assert.True(stmts.StatementList.Count >= 2);
    }

    [Fact]
    public void ParseStatements_Null_ShouldReturnNull()
    {
        var stmts = SqlParser.ParseStatements((string?)null);
        Assert.Null(stmts);
    }

    [Fact]
    public void ParseStatements_Empty_ShouldReturnNull()
    {
        var stmts = SqlParser.ParseStatements("");
        Assert.Null(stmts);
    }

    #endregion
}
