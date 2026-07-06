using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// DDL 语句详细测试 (CREATE/ALTER/DROP)
/// </summary>
public class DdlStatementTest
{
    #region CREATE TABLE

    [Fact]
    public void CreateTable_Simple_ShouldHaveTable()
    {
        var stmt = (Azrng.JSqlParser.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE users (id INT, name VARCHAR(100))")!;
        Assert.NotNull(stmt.Table);
        Assert.Equal("users", stmt.Table!.Name);
    }

    [Fact]
    public void CreateTable_WithColumns_ShouldHaveColumnDefinitions()
    {
        var stmt = (Azrng.JSqlParser.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE users (id INT, name VARCHAR(100), email VARCHAR(200))")!;
        Assert.NotNull(stmt.ColumnDefinitions);
        Assert.Equal(3, stmt.ColumnDefinitions!.Count);
    }

    [Fact]
    public void CreateTable_WithPrimaryKey_ShouldParse()
    {
        var stmt = (Azrng.JSqlParser.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE users (id INT PRIMARY KEY, name VARCHAR(100))")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void CreateTable_WithNotNull_ShouldParse()
    {
        var stmt = (Azrng.JSqlParser.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE users (id INT NOT NULL, name VARCHAR(100) NOT NULL)")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void CreateTable_WithDefaultValue_ShouldParse()
    {
        var stmt = (Azrng.JSqlParser.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE users (id INT, status VARCHAR(20) DEFAULT 'active')")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void CreateTable_WithForeignKey_ShouldParse()
    {
        var stmt = (Azrng.JSqlParser.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE orders (id INT, user_id INT, FOREIGN KEY (user_id) REFERENCES users(id))")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void CreateTable_WithSchema_ShouldHaveSchemaTable()
    {
        var stmt = (Azrng.JSqlParser.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE mydb.users (id INT, name VARCHAR(100))")!;
        Assert.NotNull(stmt.Table);
        Assert.Equal("mydb", stmt.Table!.SchemaName);
    }

    [Fact]
    public void CreateTable_IfNotExists_ShouldParse()
    {
        var stmt = (Azrng.JSqlParser.Statement.CreateTable.CreateTable)CCJSqlParserUtil.Parse(
            "CREATE TABLE IF NOT EXISTS users (id INT, name VARCHAR(100))")!;
        Assert.NotNull(stmt);
    }

    /// <summary>
    /// MySQL SPATIAL KEY 空间索引应可解析。
    /// 对应上游 commit a019aa01 (issue #2388)。
    /// </summary>
    [Fact]
    public void CreateTable_SpatialKey_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "CREATE TABLE places (id INT NOT NULL, location GEOMETRY NOT NULL, SPATIAL KEY sp_idx_location (location))");
        Assert.NotNull(stmt);
    }

    [Fact]
    public void CreateTable_FulltextKey_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "CREATE TABLE t (id INT, name TEXT, FULLTEXT KEY idx_name (name))");
        Assert.NotNull(stmt);
    }

    [Fact]
    public void CreateTable_UniqueKey_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "CREATE TABLE t (id INT, code VARCHAR(100), UNIQUE KEY idx_code (code))");
        Assert.NotNull(stmt);
    }

    [Fact]
    public void CreateTable_PlainKey_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "CREATE TABLE t (id INT, name VARCHAR(100), KEY idx_name (name))");
        Assert.NotNull(stmt);
    }

    #endregion

    #region CREATE VIEW

    [Fact]
    public void CreateView_Simple_ShouldHaveViewName()
    {
        var stmt = (Azrng.JSqlParser.Statement.CreateView.CreateView)CCJSqlParserUtil.Parse(
            "CREATE VIEW active_users AS SELECT * FROM users WHERE status = 'active'")!;
        Assert.NotNull(stmt.View);
        Assert.Equal("active_users", stmt.View!.Name);
    }

    [Fact]
    public void CreateView_WithSelect_ShouldHaveSelect()
    {
        var stmt = (Azrng.JSqlParser.Statement.CreateView.CreateView)CCJSqlParserUtil.Parse(
            "CREATE VIEW user_summary AS SELECT id, name FROM users")!;
        Assert.NotNull(stmt.Select);
    }

    #endregion

    #region CREATE INDEX

    [Fact]
    public void CreateIndex_Simple_ShouldHaveIndexName()
    {
        var stmt = (Azrng.JSqlParser.Statement.CreateIndex.CreateIndex)CCJSqlParserUtil.Parse(
            "CREATE INDEX idx_users_name ON users(name)")!;
        Assert.NotNull(stmt.Index);
        Assert.NotNull(stmt.Table);
    }

    [Fact]
    public void CreateIndex_Unique_ShouldParse()
    {
        var stmt = (Azrng.JSqlParser.Statement.CreateIndex.CreateIndex)CCJSqlParserUtil.Parse(
            "CREATE UNIQUE INDEX idx_users_email ON users(email)")!;
        Assert.NotNull(stmt);
    }

    #endregion

    #region ALTER TABLE

    [Fact]
    public void AlterTable_AddColumn_ShouldParse()
    {
        var stmt = (Azrng.JSqlParser.Statement.Alter.Alter)CCJSqlParserUtil.Parse(
            "ALTER TABLE users ADD COLUMN email VARCHAR(200)")!;
        Assert.NotNull(stmt.Table);
        Assert.Equal("users", stmt.Table!.Name);
    }

    [Fact]
    public void AlterTable_DropColumn_ShouldParse()
    {
        var stmt = (Azrng.JSqlParser.Statement.Alter.Alter)CCJSqlParserUtil.Parse(
            "ALTER TABLE users DROP COLUMN email")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void AlterTable_ModifyColumn_ShouldParse()
    {
        var stmt = (Azrng.JSqlParser.Statement.Alter.Alter)CCJSqlParserUtil.Parse(
            "ALTER TABLE users MODIFY COLUMN name VARCHAR(200)")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void AlterTable_AddConstraint_ShouldParse()
    {
        var stmt = (Azrng.JSqlParser.Statement.Alter.Alter)CCJSqlParserUtil.Parse(
            "ALTER TABLE users ADD CONSTRAINT pk_users PRIMARY KEY (id)")!;
        Assert.NotNull(stmt);
    }

    /// <summary>
    /// ALTER TABLE ADD CONSTRAINT ... PRIMARY KEY (...) USING INDEX name 应可解析。
    /// 对应上游 commit c7b3bdbd。
    /// </summary>
    [Fact]
    public void AlterTable_AddConstraintPrimaryKey_UsingIndexName_ShouldRoundTrip()
    {
        var sql = "ALTER TABLE TNWAV ADD CONSTRAINT PK_TNWAV PRIMARY KEY (NWNAME, ZEILE, BESTGRU) USING INDEX PK_TNWAV";
        var stmt = CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal(sql, stmt.ToString());
    }

    /// <summary>
    /// 无名 USING INDEX 也应可解析。
    /// </summary>
    [Fact]
    public void AlterTable_AddConstraintPrimaryKey_UsingIndexAnonymous_ShouldRoundTrip()
    {
        var sql = "ALTER TABLE t ADD CONSTRAINT pk PRIMARY KEY (id) USING INDEX";
        var stmt = CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal(sql, stmt.ToString());
    }

    /// <summary>
    /// UNIQUE 约束也支持 USING INDEX。
    /// </summary>
    [Fact]
    public void CreateTable_UniqueConstraint_UsingIndex_ShouldRoundTrip()
    {
        var sql = "CREATE TABLE t (id INT, CONSTRAINT uk_t UNIQUE (id) USING INDEX idx_uk)";
        var stmt = CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal(sql, stmt.ToString());
    }

    #endregion

    #region DROP TABLE

    [Fact]
    public void DropTable_Simple_ShouldHaveTableName()
    {
        var stmt = (Azrng.JSqlParser.Statement.Drop.Drop)CCJSqlParserUtil.Parse("DROP TABLE users")!;
        Assert.NotNull(stmt.Name);
        Assert.Equal("users", stmt.Name!.Name);
    }

    [Fact]
    public void DropTable_IfExists_ShouldParse()
    {
        var stmt = (Azrng.JSqlParser.Statement.Drop.Drop)CCJSqlParserUtil.Parse("DROP TABLE IF EXISTS users")!;
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
        var stmt = (Azrng.JSqlParser.Statement.Drop.Drop)CCJSqlParserUtil.Parse("DROP VIEW active_users")!;
        Assert.NotNull(stmt.Name);
    }

    [Fact]
    public void DropIndex_Simple_ShouldParse()
    {
        var stmt = (Azrng.JSqlParser.Statement.Drop.Drop)CCJSqlParserUtil.Parse("DROP INDEX idx_users_name ON users")!;
        Assert.NotNull(stmt);
    }

    /// <summary>
    /// DROP INDEX 带限定表名(schema.table)应可解析。
    /// 对应上游 commit 8d967803 (issue #2344)。
    /// </summary>
    [Fact]
    public void DropIndex_OnQualifiedTable_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("DROP INDEX idx ON qual.tbl");
        Assert.NotNull(stmt);
    }

    /// <summary>
    /// DATA 关键字应可用作列名（如 ALTER TABLE DROP COLUMN data）。
    /// 对应上游 commit 2d83cea9 (issue #2340)。
    /// ANTLR 版通过 nonReservedKeyword 天然支持 DATA 作标识符。
    /// </summary>
    [Fact]
    public void AlterTable_DropColumnData_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("ALTER TABLE mytable DROP COLUMN data");
        Assert.NotNull(stmt);
    }

    /// <summary>
    /// MySQL 函数式索引键（表达式索引）应可解析。
    /// 对应上游 commit 7b87d081 (issue #2405)。
    /// </summary>
    [Fact]
    public void CreateIndex_FunctionalKey_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("CREATE INDEX idx_lower ON employees ((LOWER(name)))");
        Assert.NotNull(stmt);
    }

    [Fact]
    public void CreateIndex_FunctionalKeyMultiple_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("CREATE INDEX idx_func ON t ((LOWER(a)), (b + 1))");
        Assert.NotNull(stmt);
    }

    /// <summary>
    /// PostgreSQL ALTER TABLE ENABLE/DISABLE/FORCE/NO FORCE ROW LEVEL SECURITY 应可解析。
    /// 对应上游 commit 999cdca2。
    /// </summary>
    [Theory]
    [InlineData("ALTER TABLE mytable ENABLE ROW LEVEL SECURITY")]
    [InlineData("ALTER TABLE mytable DISABLE ROW LEVEL SECURITY")]
    [InlineData("ALTER TABLE mytable FORCE ROW LEVEL SECURITY")]
    [InlineData("ALTER TABLE mytable NO FORCE ROW LEVEL SECURITY")]
    public void AlterTable_RowLevelSecurity_ShouldParse(string sql)
    {
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
    }

    #endregion

    #region GENERATED ... AS IDENTITY

    /// <summary>
    /// PostgreSQL 风格的 GENERATED ALWAYS AS IDENTITY 列约束应可解析。
    /// 对应上游 commit bd3ce05f（JavaCC 版修复 ALWAYS 未被接受为列参数 token），
    /// ANTLR 版通过显式文法规则 GENERATED (ALWAYS | BY DEFAULT) AS IDENTITY 天然规避此问题。
    /// </summary>
    [Fact]
    public void CreateTable_GeneratedAlwaysAsIdentity_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("create table if not exists book_type ( id bigint not null generated always as identity )");
        Assert.NotNull(stmt);
    }

    [Fact]
    public void CreateTable_GeneratedByDefaultAsIdentity_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("create table if not exists book_type ( id bigint not null generated by default as identity )");
        Assert.NotNull(stmt);
    }

    #endregion
}
