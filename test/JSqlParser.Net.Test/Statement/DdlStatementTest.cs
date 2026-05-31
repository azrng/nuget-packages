using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Test.Statement;

/// <summary>
/// DDL 语句详细测试 (CREATE/ALTER/DROP)
/// </summary>
public class DdlStatementTest
{
    #region CREATE TABLE

    [Fact]
    public void CreateTable_Simple_ShouldHaveTable()
    {
        var stmt = (JSqlParser.Net.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE users (id INT, name VARCHAR(100))")!;
        Assert.NotNull(stmt.Table);
        Assert.Equal("users", stmt.Table!.Name);
    }

    [Fact]
    public void CreateTable_WithColumns_ShouldHaveColumnDefinitions()
    {
        var stmt = (JSqlParser.Net.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE users (id INT, name VARCHAR(100), email VARCHAR(200))")!;
        Assert.NotNull(stmt.ColumnDefinitions);
        Assert.Equal(3, stmt.ColumnDefinitions!.Count);
    }

    [Fact]
    public void CreateTable_WithPrimaryKey_ShouldParse()
    {
        var stmt = (JSqlParser.Net.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE users (id INT PRIMARY KEY, name VARCHAR(100))")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void CreateTable_WithNotNull_ShouldParse()
    {
        var stmt = (JSqlParser.Net.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE users (id INT NOT NULL, name VARCHAR(100) NOT NULL)")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void CreateTable_WithDefaultValue_ShouldParse()
    {
        var stmt = (JSqlParser.Net.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE users (id INT, status VARCHAR(20) DEFAULT 'active')")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void CreateTable_WithForeignKey_ShouldParse()
    {
        var stmt = (JSqlParser.Net.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE orders (id INT, user_id INT, FOREIGN KEY (user_id) REFERENCES users(id))")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void CreateTable_WithSchema_ShouldHaveSchemaTable()
    {
        var stmt = (JSqlParser.Net.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE mydb.users (id INT, name VARCHAR(100))")!;
        Assert.NotNull(stmt.Table);
        Assert.Equal("mydb", stmt.Table!.SchemaName);
    }

    [Fact]
    public void CreateTable_IfNotExists_ShouldParse()
    {
        var stmt = (JSqlParser.Net.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE IF NOT EXISTS users (id INT, name VARCHAR(100))")!;
        Assert.NotNull(stmt);
    }

    #endregion

    #region CREATE VIEW

    [Fact]
    public void CreateView_Simple_ShouldHaveViewName()
    {
        var stmt = (JSqlParser.Net.Statement.CreateView.CreateView)CCJSqlParserUtil.Parse(
            "CREATE VIEW active_users AS SELECT * FROM users WHERE status = 'active'")!;
        Assert.NotNull(stmt.View);
        Assert.Equal("active_users", stmt.View!.Name);
    }

    [Fact]
    public void CreateView_WithSelect_ShouldHaveSelect()
    {
        var stmt = (JSqlParser.Net.Statement.CreateView.CreateView)CCJSqlParserUtil.Parse(
            "CREATE VIEW user_summary AS SELECT id, name FROM users")!;
        Assert.NotNull(stmt.Select);
    }

    #endregion

    #region CREATE INDEX

    [Fact]
    public void CreateIndex_Simple_ShouldHaveIndexName()
    {
        var stmt = (JSqlParser.Net.Statement.CreateIndex.CreateIndex)CCJSqlParserUtil.Parse(
            "CREATE INDEX idx_users_name ON users(name)")!;
        Assert.NotNull(stmt.Index);
        Assert.NotNull(stmt.Table);
    }

    [Fact]
    public void CreateIndex_Unique_ShouldParse()
    {
        var stmt = (JSqlParser.Net.Statement.CreateIndex.CreateIndex)CCJSqlParserUtil.Parse(
            "CREATE UNIQUE INDEX idx_users_email ON users(email)")!;
        Assert.NotNull(stmt);
    }

    #endregion

    #region ALTER TABLE

    [Fact]
    public void AlterTable_AddColumn_ShouldParse()
    {
        var stmt = (JSqlParser.Net.Statement.Alter.Alter)CCJSqlParserUtil.Parse(
            "ALTER TABLE users ADD COLUMN email VARCHAR(200)")!;
        Assert.NotNull(stmt.Table);
        Assert.Equal("users", stmt.Table!.Name);
    }

    [Fact]
    public void AlterTable_DropColumn_ShouldParse()
    {
        var stmt = (JSqlParser.Net.Statement.Alter.Alter)CCJSqlParserUtil.Parse(
            "ALTER TABLE users DROP COLUMN email")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void AlterTable_ModifyColumn_ShouldParse()
    {
        var stmt = (JSqlParser.Net.Statement.Alter.Alter)CCJSqlParserUtil.Parse(
            "ALTER TABLE users MODIFY COLUMN name VARCHAR(200)")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void AlterTable_AddConstraint_ShouldParse()
    {
        var stmt = (JSqlParser.Net.Statement.Alter.Alter)CCJSqlParserUtil.Parse(
            "ALTER TABLE users ADD CONSTRAINT pk_users PRIMARY KEY (id)")!;
        Assert.NotNull(stmt);
    }

    #endregion

    #region DROP TABLE

    [Fact]
    public void DropTable_Simple_ShouldHaveTableName()
    {
        var stmt = (JSqlParser.Net.Statement.Drop.Drop)CCJSqlParserUtil.Parse("DROP TABLE users")!;
        Assert.NotNull(stmt.Name);
        Assert.Equal("users", stmt.Name!.Name);
    }

    [Fact]
    public void DropTable_IfExists_ShouldParse()
    {
        var stmt = (JSqlParser.Net.Statement.Drop.Drop)CCJSqlParserUtil.Parse("DROP TABLE IF EXISTS users")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void DropTable_Multiple_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("DROP TABLE users, orders, products");
        Assert.NotNull(stmt);
    }

    #endregion

    #region DROP VIEW / INDEX

    [Fact]
    public void DropView_Simple_ShouldParse()
    {
        var stmt = (JSqlParser.Net.Statement.Drop.Drop)CCJSqlParserUtil.Parse("DROP VIEW active_users")!;
        Assert.NotNull(stmt.Name);
    }

    [Fact]
    public void DropIndex_Simple_ShouldParse()
    {
        var stmt = (JSqlParser.Net.Statement.Drop.Drop)CCJSqlParserUtil.Parse("DROP INDEX idx_users_name ON users")!;
        Assert.NotNull(stmt);
    }

    #endregion
}
