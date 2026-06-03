using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement;
using Azrng.JSqlParser.Statement.CreateTable;
using Azrng.JSqlParser.Statement.Delete;
using Azrng.JSqlParser.Statement.Insert;
using Azrng.JSqlParser.Statement.Select;
using Azrng.JSqlParser.Statement.Update;
using Xunit.Abstractions;

namespace Azrng.JSqlParser.Test.Parser.ANTLR4;

/// <summary>
/// M005: AstBuilderVisitor AST 产出测试
/// 验证 CCJSqlParserUtil.Parse() 能正确生成原生 C# AST 节点
/// </summary>
public class AstBuilderVisitorTest
{
    private readonly ITestOutputHelper _output;

    public AstBuilderVisitorTest(ITestOutputHelper output)
    {
        _output = output;
    }

    #region SELECT

    [Fact]
    public void Parse_SelectAll_ShouldReturnPlainSelect()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM users");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.NotNull(select.FromItem);
        Assert.Single(select.SelectItems!);
    }

    [Fact]
    public void Parse_SelectColumns_ShouldHaveCorrectItemCount()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT a, b, c FROM t");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.Equal(3, select.SelectItems!.Count);
    }

    [Fact]
    public void Parse_SelectWithWhere_ShouldHaveWhereExpression()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM users WHERE id = 1");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.NotNull(select.Where);
        Assert.IsType<EqualsTo>(select.Where);
    }

    [Fact]
    public void Parse_SelectWithJoin_ShouldHaveJoin()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT a.id, b.name FROM a INNER JOIN b ON a.id = b.a_id");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.NotNull(select.Joins);
        Assert.Single(select.Joins);
        Assert.True(select.Joins[0].Inner);
    }

    [Fact]
    public void Parse_SelectWithLeftJoin_ShouldHaveLeftJoin()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT a.id, b.name FROM a LEFT JOIN b ON a.id = b.a_id");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.NotNull(select.Joins);
        Assert.Single(select.Joins);
        Assert.True(select.Joins[0].Left);
    }

    [Fact]
    public void Parse_SelectWithOrderBy_ShouldHaveOrderBy()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM users ORDER BY id ASC");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.NotNull(select.OrderByElements);
        Assert.Single(select.OrderByElements);
        Assert.True(select.OrderByElements[0].Asc);
    }

    [Fact]
    public void Parse_SelectWithGroupBy_ShouldHaveGroupBy()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT dept, COUNT(*) FROM emp GROUP BY dept");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.NotNull(select.GroupBy);
    }

    [Fact]
    public void Parse_SelectWithLimit_ShouldHaveLimit()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM users LIMIT 10");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.NotNull(select.Limit);
    }

    #endregion

    #region INSERT

    [Fact]
    public void Parse_InsertValues_ShouldReturnInsert()
    {
        var stmt = CCJSqlParserUtil.Parse("INSERT INTO users (id, name) VALUES (1, 'test')");
        var insert = Assert.IsType<Insert>(stmt);
        Assert.NotNull(insert.Table);
        Assert.NotNull(insert.Columns);
        Assert.Equal(2, insert.Columns.Count);
    }

    [Fact]
    public void Parse_InsertSelect_ShouldReturnInsert()
    {
        var stmt = CCJSqlParserUtil.Parse("INSERT INTO archive SELECT * FROM users");
        var insert = Assert.IsType<Insert>(stmt);
        Assert.NotNull(insert.Select);
    }

    #endregion

    #region UPDATE

    [Fact]
    public void Parse_UpdateSet_ShouldReturnUpdate()
    {
        var stmt = CCJSqlParserUtil.Parse("UPDATE users SET name = 'test' WHERE id = 1");
        var update = Assert.IsType<Update>(stmt);
        Assert.NotNull(update.Table);
        Assert.Single(update.UpdateSets);
        Assert.NotNull(update.Where);
    }

    [Fact]
    public void Parse_UpdateMultipleColumns_ShouldHaveMultipleSets()
    {
        var stmt = CCJSqlParserUtil.Parse("UPDATE users SET name = 'a', age = 20 WHERE id = 1");
        var update = Assert.IsType<Update>(stmt);
        Assert.Equal(2, update.UpdateSets.Count);
    }

    #endregion

    #region DELETE

    [Fact]
    public void Parse_DeleteWithWhere_ShouldReturnDelete()
    {
        var stmt = CCJSqlParserUtil.Parse("DELETE FROM users WHERE id = 1");
        var delete = Assert.IsType<Delete>(stmt);
        Assert.NotNull(delete.Table);
        Assert.NotNull(delete.Where);
    }

    #endregion

    #region CREATE TABLE

    [Fact]
    public void Parse_CreateTable_ShouldReturnCreateTable()
    {
        var stmt = CCJSqlParserUtil.Parse("CREATE TABLE users (id INT PRIMARY KEY, name VARCHAR(100))");
        var ct = Assert.IsType<CreateTable>(stmt);
        Assert.NotNull(ct.Table);
        Assert.NotNull(ct.ColumnDefinitions);
        Assert.Equal(2, ct.ColumnDefinitions.Count);
    }

    #endregion

    #region Expressions

    [Fact]
    public void Parse_Expression_Equality()
    {
        var expr = CCJSqlParserUtil.ParseExpression("a = 1");
        Assert.IsType<EqualsTo>(expr);
    }

    [Fact]
    public void Parse_Expression_And()
    {
        var expr = CCJSqlParserUtil.ParseExpression("a = 1 AND b = 2");
        Assert.IsType<AndExpression>(expr);
    }

    [Fact]
    public void Parse_Expression_Or()
    {
        var expr = CCJSqlParserUtil.ParseExpression("a = 1 OR b = 2");
        Assert.IsType<OrExpression>(expr);
    }

    [Fact]
    public void Parse_Expression_Not()
    {
        var expr = CCJSqlParserUtil.ParseExpression("NOT a = 1");
        Assert.IsType<NotExpression>(expr);
    }

    [Fact]
    public void Parse_Expression_Addition()
    {
        var expr = CCJSqlParserUtil.ParseExpression("a + b");
        Assert.IsType<Addition>(expr);
    }

    [Fact]
    public void Parse_Expression_Multiplication()
    {
        var expr = CCJSqlParserUtil.ParseExpression("a * b");
        Assert.IsType<Multiplication>(expr);
    }

    [Fact]
    public void Parse_Expression_Cast()
    {
        var expr = CCJSqlParserUtil.ParseExpression("a::int");
        var cast = Assert.IsType<CastExpression>(expr);
        Assert.False(cast.UseCastKeyword);
        Assert.Equal("int", cast.DataType);
    }

    [Fact]
    public void Parse_Expression_Like()
    {
        var expr = CCJSqlParserUtil.ParseExpression("name LIKE '%test%'");
        Assert.IsType<LikeExpression>(expr);
    }

    [Fact]
    public void Parse_Expression_In()
    {
        var expr = CCJSqlParserUtil.ParseExpression("id IN (1, 2, 3)");
        Assert.IsType<InExpression>(expr);
    }

    [Fact]
    public void Parse_Expression_Between()
    {
        var expr = CCJSqlParserUtil.ParseExpression("age BETWEEN 18 AND 65");
        Assert.IsType<Between>(expr);
    }

    [Fact]
    public void Parse_Expression_IsNull()
    {
        var expr = CCJSqlParserUtil.ParseExpression("name IS NULL");
        Assert.IsType<IsNullExpression>(expr);
    }

    [Fact]
    public void Parse_Expression_Exists()
    {
        var expr = CCJSqlParserUtil.ParseExpression("EXISTS (SELECT 1 FROM t)");
        Assert.IsType<ExistsExpression>(expr);
    }

    #endregion

    #region UNION / INTERSECT / EXCEPT

    [Fact]
    public void Parse_Union_ShouldReturnSetOperationList()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM a UNION SELECT id FROM b");
        var setOp = Assert.IsType<SetOperationList>(stmt);
        Assert.NotNull(setOp.Operations);
        Assert.NotEmpty(setOp.Operations);
    }

    [Fact]
    public void Parse_UnionAll_ShouldReturnSetOperationList()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM a UNION ALL SELECT id FROM b");
        var setOp = Assert.IsType<SetOperationList>(stmt);
        Assert.NotNull(setOp.Operations);
    }

    #endregion

    #region Subquery / CTE

    [Fact]
    public void Parse_SubqueryInFrom_ShouldHaveParenthesedSelect()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM (SELECT id FROM users) t");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.IsType<ParenthesedSelect>(select.FromItem);
    }

    [Fact]
    public void Parse_WithCte_ShouldHaveWithItem()
    {
        var stmt = CCJSqlParserUtil.Parse("WITH cte AS (SELECT id FROM users) SELECT * FROM cte");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.NotNull(select.WithItemsList);
        Assert.NotEmpty(select.WithItemsList);
    }

    #endregion

    #region ParseNullable

    [Fact]
    public void ParseNullable_ValidSql_ShouldReturnStatement()
    {
        var stmt = CCJSqlParserUtil.ParseNullable("SELECT 1");
        Assert.NotNull(stmt);
    }

    [Fact]
    public void ParseNullable_InvalidSql_ShouldReturnNull()
    {
        var stmt = CCJSqlParserUtil.ParseNullable("NOT VALID SQL AT ALL !!!");
        Assert.Null(stmt);
    }

    #endregion

    #region Literals

    [Fact]
    public void Parse_SelectStringLiteral_ShouldParseCorrectly()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT 'hello'");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.Single(select.SelectItems!);
    }

    [Fact]
    public void Parse_SelectNumberLiteral_ShouldParseCorrectly()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT 42");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.Single(select.SelectItems!);
    }

    [Fact]
    public void Parse_SelectNull_ShouldParseCorrectly()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT NULL");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.Single(select.SelectItems!);
    }

    #endregion

    #region CASE expression

    [Fact]
    public void Parse_SelectCase_ShouldParseCorrectly()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT CASE WHEN id = 1 THEN 'a' ELSE 'b' END FROM t");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.Single(select.SelectItems!);
    }

    #endregion

    #region CAST expression

    [Fact]
    public void Parse_SelectCast_ShouldParseCorrectly()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT CAST(id AS VARCHAR) FROM t");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.Single(select.SelectItems!);
    }

    #endregion

    #region Function

    [Fact]
    public void Parse_SelectFunction_ShouldParseCorrectly()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT COUNT(*) FROM t");
        var select = Assert.IsType<PlainSelect>(stmt);
        Assert.Single(select.SelectItems!);
    }

    #endregion

    #region Transactions

    [Fact]
    public void Parse_Commit_ShouldReturnCommitStatement()
    {
        var stmt = CCJSqlParserUtil.Parse("COMMIT");
        Assert.IsType<CommitStatement>(stmt);
        Assert.Equal("COMMIT", stmt!.ToString());
    }

    [Fact]
    public void Parse_Rollback_ShouldReturnRollbackStatement()
    {
        var stmt = CCJSqlParserUtil.Parse("ROLLBACK");
        var rb = Assert.IsType<RollbackStatement>(stmt);
        Assert.Null(rb.Savepoint);
    }

    [Fact]
    public void Parse_RollbackToSavepoint_ShouldHaveSavepoint()
    {
        var stmt = CCJSqlParserUtil.Parse("ROLLBACK TO sp1");
        var rb = Assert.IsType<RollbackStatement>(stmt);
        Assert.Equal("sp1", rb.Savepoint);
    }

    #endregion
}
