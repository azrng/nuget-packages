using JSqlParser.Net.Parser;
using JSqlParser.Net.Statement.Select;
using Insert = JSqlParser.Net.Statement.Insert.Insert;
using Update = JSqlParser.Net.Statement.Update.Update;
using Delete = JSqlParser.Net.Statement.Delete.Delete;

namespace JSqlParser.Net.Test.Statement;

/// <summary>
/// DML 语句详细测试 (INSERT/UPDATE/DELETE)
/// </summary>
public class DmlStatementTest
{
    #region INSERT

    [Fact]
    public void Insert_WithColumns_ShouldHaveColumns()
    {
        var stmt = (Insert)CCJSqlParserUtil.Parse("INSERT INTO users (id, name) VALUES (1, 'test')")!;
        Assert.NotNull(stmt.Table);
        Assert.Equal("users", stmt.Table!.Name);
        Assert.NotNull(stmt.Columns);
        Assert.Equal(2, stmt.Columns!.Count);
    }

    [Fact]
    public void Insert_WithValues_ShouldParse()
    {
        var stmt = (Insert)CCJSqlParserUtil.Parse("INSERT INTO users (id, name) VALUES (1, 'test')")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void Insert_MultipleRows_ShouldParse()
    {
        var stmt = (Insert)CCJSqlParserUtil.Parse(
            "INSERT INTO users (id, name) VALUES (1, 'a'), (2, 'b'), (3, 'c')")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void Insert_WithSelect_ShouldHaveSelect()
    {
        var stmt = (Insert)CCJSqlParserUtil.Parse(
            "INSERT INTO archive (id, name) SELECT id, name FROM users")!;
        Assert.NotNull(stmt.Select);
    }

    [Fact]
    public void Insert_WithoutColumns_ShouldParse()
    {
        var stmt = (Insert)CCJSqlParserUtil.Parse("INSERT INTO users VALUES (1, 'test')")!;
        Assert.NotNull(stmt.Table);
    }

    #endregion

    #region UPDATE

    [Fact]
    public void Update_SingleSet_ShouldHaveUpdateSet()
    {
        var stmt = (Update)CCJSqlParserUtil.Parse("UPDATE users SET name = 'test' WHERE id = 1")!;
        Assert.NotNull(stmt.Table);
        Assert.Equal("users", stmt.Table!.Name);
        Assert.NotNull(stmt.UpdateSets);
        Assert.Equal(1, stmt.UpdateSets!.Count);
    }

    [Fact]
    public void Update_MultipleSet_ShouldHaveMultipleUpdateSets()
    {
        var stmt = (Update)CCJSqlParserUtil.Parse(
            "UPDATE users SET name = 'test', age = 20, email = 'a@b.com' WHERE id = 1")!;
        Assert.Equal(3, stmt.UpdateSets!.Count);
    }

    [Fact]
    public void Update_WithWhere_ShouldHaveWhere()
    {
        var stmt = (Update)CCJSqlParserUtil.Parse("UPDATE users SET name = 'test' WHERE id = 1")!;
        Assert.NotNull(stmt.Where);
    }

    [Fact]
    public void Update_WithoutWhere_ShouldHaveNullWhere()
    {
        var stmt = (Update)CCJSqlParserUtil.Parse("UPDATE users SET name = 'test'")!;
        Assert.Null(stmt.Where);
    }

    [Fact]
    public void Update_WithJoin_ShouldParse()
    {
        var stmt = (Update)CCJSqlParserUtil.Parse(
            "UPDATE users u INNER JOIN orders o ON u.id = o.user_id SET u.name = 'test'")!;
        Assert.NotNull(stmt);
        Assert.NotNull(stmt.Table);
    }

    [Fact]
    public void Update_SetWithExpression_ShouldParse()
    {
        var stmt = (Update)CCJSqlParserUtil.Parse("UPDATE users SET age = age + 1 WHERE id = 1")!;
        Assert.NotNull(stmt.UpdateSets);
    }

    #endregion

    #region DELETE

    [Fact]
    public void Delete_Simple_ShouldHaveTable()
    {
        var stmt = (Delete)CCJSqlParserUtil.Parse("DELETE FROM users WHERE id = 1")!;
        Assert.NotNull(stmt.Table);
        Assert.Equal("users", stmt.Table!.Name);
    }

    [Fact]
    public void Delete_WithWhere_ShouldHaveWhere()
    {
        var stmt = (Delete)CCJSqlParserUtil.Parse("DELETE FROM users WHERE id = 1")!;
        Assert.NotNull(stmt.Where);
    }

    [Fact]
    public void Delete_WithoutWhere_ShouldHaveNullWhere()
    {
        var stmt = (Delete)CCJSqlParserUtil.Parse("DELETE FROM users")!;
        Assert.Null(stmt.Where);
    }

    [Fact]
    public void Delete_WithSchema_ShouldHaveSchemaTable()
    {
        var stmt = (Delete)CCJSqlParserUtil.Parse("DELETE FROM mydb.users WHERE id = 1")!;
        Assert.NotNull(stmt.Table);
        Assert.Equal("mydb", stmt.Table!.SchemaName);
    }

    [Fact]
    public void Delete_WithAlias_ShouldHaveAlias()
    {
        var stmt = (Delete)CCJSqlParserUtil.Parse("DELETE u FROM users u WHERE u.id = 1")!;
        Assert.NotNull(stmt.Table);
    }

    [Fact]
    public void Delete_WithMultipleConditions_ShouldHaveWhere()
    {
        var stmt = (Delete)CCJSqlParserUtil.Parse(
            "DELETE FROM users WHERE id = 1 AND status = 'inactive'")!;
        Assert.NotNull(stmt.Where);
    }

    #endregion
}
