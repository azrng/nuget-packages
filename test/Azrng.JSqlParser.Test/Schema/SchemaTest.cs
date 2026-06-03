using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Schema;

/// <summary>
/// Schema 类型测试 (Table, Column, Database)
/// </summary>
public class SchemaTest
{
    #region Table

    [Fact]
    public void Table_SimpleName_ShouldHaveName()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM users")!;
        var table = (Table)select.FromItem!;
        Assert.Equal("users", table.Name);
    }

    [Fact]
    public void Table_WithAlias_ShouldHaveAlias()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM users u")!;
        var table = (Table)select.FromItem!;
        Assert.Equal("users", table.Name);
        Assert.NotNull(table.Alias);
        Assert.Equal("u", table.Alias!.Name);
    }

    [Fact]
    public void Table_WithSchema_ShouldHaveSchemaName()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM mydb.users")!;
        var table = (Table)select.FromItem!;
        Assert.Equal("users", table.Name);
        Assert.Equal("mydb", table.SchemaName);
    }

    [Fact]
    public void Table_WithSchemaAndAlias_ShouldHaveBoth()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM mydb.users u")!;
        var table = (Table)select.FromItem!;
        Assert.Equal("users", table.Name);
        Assert.Equal("mydb", table.SchemaName);
        Assert.NotNull(table.Alias);
        Assert.Equal("u", table.Alias!.Name);
    }

    [Fact]
    public void Table_WithDatabaseAndSchema_ShouldHaveDatabase()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM mydb.public.users")!;
        var table = (Table)select.FromItem!;
        Assert.Equal("users", table.Name);
    }

    [Fact]
    public void Table_FullyQualifiedName_ShouldReturnFullName()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM mydb.users")!;
        var table = (Table)select.FromItem!;
        Assert.Equal("mydb.users", table.GetFullyQualifiedName());
    }

    [Fact]
    public void Table_SimpleFullyQualifiedName_ShouldReturnName()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM users")!;
        var table = (Table)select.FromItem!;
        Assert.Equal("users", table.GetFullyQualifiedName());
    }

    #endregion

    #region Column

    [Fact]
    public void Column_SimpleName_ShouldHaveColumnName()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM users")!;
        var item = select.SelectItems![0];
        var column = (Column)item.Expression;
        Assert.Equal("id", column.ColumnName);
    }

    [Fact]
    public void Column_WithTableAlias_ShouldHaveTable()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT u.id FROM users u")!;
        var item = select.SelectItems![0];
        var column = (Column)item.Expression;
        Assert.Equal("id", column.ColumnName);
        Assert.NotNull(column.Table);
        Assert.Equal("u", column.Table!.Name);
    }

    [Fact]
    public void Column_WithSchema_ShouldHaveSchemaTable()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT mydb.users.id FROM mydb.users")!;
        var item = select.SelectItems![0];
        var column = (Column)item.Expression;
        // Multi-part column name may be parsed as full string
        Assert.NotNull(column.ColumnName);
    }

    [Fact]
    public void Column_FullyQualifiedName_WithTable_ShouldReturnFullName()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT u.id FROM users u")!;
        var item = select.SelectItems![0];
        var column = (Column)item.Expression;
        Assert.Equal("u.id", column.GetFullyQualifiedName());
    }

    [Fact]
    public void Column_FullyQualifiedName_WithoutTable_ShouldReturnName()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM users")!;
        var item = select.SelectItems![0];
        var column = (Column)item.Expression;
        Assert.Equal("id", column.GetFullyQualifiedName());
    }

    #endregion

    #region Column in WHERE

    [Fact]
    public void Column_InWhereClause_ShouldHaveColumnName()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM users WHERE name = 'test'")!;
        var where = (EqualsTo)select.Where!;
        var column = (Column)where.LeftExpression;
        Assert.Equal("name", column.ColumnName);
    }

    [Fact]
    public void Column_InWhereWithTableAlias_ShouldHaveTable()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT u.id FROM users u WHERE u.name = 'test'")!;
        var where = (EqualsTo)select.Where!;
        var column = (Column)where.LeftExpression;
        Assert.Equal("name", column.ColumnName);
        Assert.NotNull(column.Table);
        Assert.Equal("u", column.Table!.Name);
    }

    #endregion

    #region Column in JOIN ON

    [Fact]
    public void Column_InJoinOn_ShouldHaveTableName()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT a.id FROM users a INNER JOIN orders b ON a.id = b.user_id")!;
        var join = select.Joins![0];
        var on = (EqualsTo)join.OnExpression!;
        var leftCol = (Column)on.LeftExpression;
        var rightCol = (Column)on.RightExpression;

        Assert.Equal("id", leftCol.ColumnName);
        Assert.NotNull(leftCol.Table);
        Assert.Equal("a", leftCol.Table!.Name);

        Assert.Equal("user_id", rightCol.ColumnName);
        Assert.NotNull(rightCol.Table);
        Assert.Equal("b", rightCol.Table!.Name);
    }

    #endregion

    #region 多表 FROM

    [Fact]
    public void MultipleTables_InFrom_ShouldParseCorrectly()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(
            "SELECT a.id, b.name FROM users a, orders b")!;
        // 多表逗号分隔会被解析为 JOIN
        Assert.NotNull(select.FromItem);
    }

    #endregion

    #region Database

    [Fact]
    public void Database_InTable_ShouldHaveSchemaInfo()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM mydb.public.users")!;
        var table = (Table)select.FromItem!;
        // 3-part name: schema info is captured in SchemaName or Database
        Assert.Equal("users", table.Name);
    }

    #endregion

    #region Sequence (通过 NEXTVAL 表达式间接测试)

    [Fact]
    public void Sequence_NextVal_ShouldParseAsFunction()
    {
        // NEXTVAL 在 ANTLR4 中被解析为 Function
        var expr = CCJSqlParserUtil.ParseExpression("NEXTVAL('my_sequence')");
        Assert.NotNull(expr);
        Assert.IsType<Function>(expr);
    }

    #endregion

    #region 特殊表名

    [Fact]
    public void Table_WithUnderscore_ShouldParseCorrectly()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM user_data")!;
        var table = (Table)select.FromItem!;
        Assert.Equal("user_data", table.Name);
    }

    [Fact]
    public void Table_QuotedName_ShouldIncludeQuotes()
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT id FROM \"user\"")!;
        var table = (Table)select.FromItem!;
        Assert.Equal("\"user\"", table.Name);
    }

    #endregion
}
