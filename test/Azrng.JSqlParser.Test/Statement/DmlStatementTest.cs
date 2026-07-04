using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;
using Insert = Azrng.JSqlParser.Statement.Insert.Insert;
using Update = Azrng.JSqlParser.Statement.Update.Update;
using Delete = Azrng.JSqlParser.Statement.Delete.Delete;

namespace Azrng.JSqlParser.Test.Statement;

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

    /// <summary>
    /// openGauss 的 ON DUPLICATE KEY UPDATE NOTHING 应正确解析并往返。
    /// 对应上游 commit 468aefae。注：用 INSERT...SELECT 规避既有 VALUES 序列化缺陷。
    /// </summary>
    [Fact]
    public void Insert_OnDuplicateKeyUpdateNothing_ShouldRoundTrip()
    {
        var sql = "INSERT INTO example (num, name) VALUES (1, 'name') ON DUPLICATE KEY UPDATE NOTHING";
        var insert = (Insert)CCJSqlParserUtil.Parse(sql)!;
        Assert.True(insert.DuplicateUpdateNothing);
        Assert.Equal(sql, insert.ToString());
    }

    /// <summary>
    /// 传统 MySQL 的 ON DUPLICATE KEY UPDATE col=val 应正确解析并往返。
    /// 此前 VisitInsertStatement 漏处理 onDuplicateKey 且 ToString 未输出，已修复。
    /// </summary>
    [Fact]
    public void Insert_OnDuplicateKeyUpdate_ShouldRoundTrip()
    {
        var sql = "INSERT INTO users (id, name) VALUES (1, 'a') ON DUPLICATE KEY UPDATE name = 'b'";
        var insert = (Insert)CCJSqlParserUtil.Parse(sql)!;
        Assert.NotNull(insert.DuplicateUpdateSets);
        Assert.Single(insert.DuplicateUpdateSets!);
        Assert.Equal(sql, insert.ToString());
    }

    /// <summary>
    /// INSERT VALUES 的值数据应正确保存并往返序列化。
    /// 此前 VisitInsertStatement 仅设 UseValues 标志但未保存值，ToString 丢失 VALUES 子句，已修复。
    /// </summary>
    [Fact]
    public void Insert_Values_ShouldRoundTrip()
    {
        var sql = "INSERT INTO users (id, name) VALUES (1, 'test')";
        var insert = (Insert)CCJSqlParserUtil.Parse(sql)!;
        Assert.NotNull(insert.ValuesItems);
        Assert.Single(insert.ValuesItems!);
        Assert.Equal(2, insert.ValuesItems![0].Expressions.Count);
        Assert.Equal(sql, insert.ToString());
    }

    [Fact]
    public void Insert_MultipleValues_ShouldRoundTrip()
    {
        var sql = "INSERT INTO users (id, name) VALUES (1, 'a'), (2, 'b'), (3, 'c')";
        var insert = (Insert)CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal(3, insert.ValuesItems!.Count);
        Assert.Equal(sql, insert.ToString());
    }

    [Fact]
    public void Insert_ValuesNoColumns_ShouldRoundTrip()
    {
        var sql = "INSERT INTO users VALUES (1, 'test')";
        var insert = (Insert)CCJSqlParserUtil.Parse(sql)!;
        Assert.NotNull(insert.ValuesItems);
        Assert.Equal(sql, insert.ToString());
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
        Assert.Single(stmt.UpdateSets!);
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
