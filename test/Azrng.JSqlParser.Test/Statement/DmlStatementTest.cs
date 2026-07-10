using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Insert;
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
    /// MySQL 8.0.20+ 的 ON DUPLICATE KEY UPDATE ... WHERE 条件应正确解析（BL-19g）。
    /// </summary>
    [Fact]
    public void Insert_OnDuplicateKeyUpdate_WithWhere_ShouldParse()
    {
        var sql = "INSERT INTO users (id, name) VALUES (1, 'a') ON DUPLICATE KEY UPDATE name = 'b' WHERE id > 0";
        var insert = (Insert)CCJSqlParserUtil.Parse(sql)!;
        Assert.NotNull(insert.DuplicateUpdateSets);
        Assert.NotNull(insert.DuplicateUpdateWhereExpression);
        Assert.Contains("WHERE id > 0", insert.ToString()!);
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

    [Fact]
    public void Delete_Using_Single_ShouldHaveUsingItems()
    {
        var stmt = (Delete)CCJSqlParserUtil.Parse(
            "DELETE FROM users USING orders WHERE users.id = orders.uid")!;
        Assert.NotNull(stmt.UsingItems);
        Assert.Single(stmt.UsingItems!);
        Assert.Contains("USING", stmt.ToString()!);
        Assert.Contains("orders", stmt.ToString()!);
    }

    [Fact]
    public void Delete_Using_Multiple_ShouldHaveAllItems()
    {
        var stmt = (Delete)CCJSqlParserUtil.Parse(
            "DELETE FROM users USING orders, items WHERE users.id = orders.uid AND orders.item_id = items.id")!;
        Assert.NotNull(stmt.UsingItems);
        Assert.Equal(2, stmt.UsingItems!.Count);
        var sql = stmt.ToString()!;
        Assert.Contains("USING", sql);
        Assert.Contains("orders", sql);
        Assert.Contains("items", sql);
    }

    [Fact]
    public void Delete_Using_RoundTrip_ShouldPreserveSyntax()
    {
        var sql = "DELETE FROM users USING orders WHERE users.id = orders.uid";
        var stmt = (Delete)CCJSqlParserUtil.Parse(sql)!;
        var output = stmt.ToString()!;
        Assert.Contains("DELETE FROM users", output);
        Assert.Contains("USING orders", output);
        Assert.Contains("WHERE users.id = orders.uid", output);
    }

    [Fact]
    public void Delete_WithoutUsing_ShouldHaveNullUsingItems()
    {
        var stmt = (Delete)CCJSqlParserUtil.Parse("DELETE FROM users WHERE id = 1")!;
        Assert.Null(stmt.UsingItems);
    }

    [Theory]
    [InlineData("LOW_PRIORITY", InsertModifierPriority.LowPriority)]
    [InlineData("DELAYED", InsertModifierPriority.Delayed)]
    [InlineData("HIGH_PRIORITY", InsertModifierPriority.HighPriority)]
    public void Insert_PriorityModifier_ShouldRoundTrip(string modifier, InsertModifierPriority expected)
    {
        var sql = $"INSERT {modifier} INTO users (id) VALUES (1)";
        var stmt = (Insert)CCJSqlParserUtil.Parse(sql)!;
        Assert.Equal(expected, stmt.ModifierPriority);
        // 往返保留修饰符
        var output = stmt.ToString()!;
        Assert.Contains(modifier, output);
    }

    [Fact]
    public void Insert_Ignore_ShouldHaveModifierIgnoreFlag()
    {
        var stmt = (Insert)CCJSqlParserUtil.Parse("INSERT IGNORE INTO users (id) VALUES (1)")!;
        Assert.True(stmt.ModifierIgnore);
        Assert.Contains("IGNORE", stmt.ToString()!);
    }

    [Fact]
    public void Insert_PriorityAndIgnore_ShouldRoundTrip()
    {
        var stmt = (Insert)CCJSqlParserUtil.Parse("INSERT LOW_PRIORITY IGNORE INTO users (id) VALUES (1)")!;
        Assert.Equal(InsertModifierPriority.LowPriority, stmt.ModifierPriority);
        Assert.True(stmt.ModifierIgnore);
    }

    [Fact]
    public void Insert_NoModifier_ShouldHaveDefaults()
    {
        var stmt = (Insert)CCJSqlParserUtil.Parse("INSERT INTO users (id) VALUES (1)")!;
        Assert.Equal(InsertModifierPriority.None, stmt.ModifierPriority);
        Assert.False(stmt.ModifierIgnore);
    }

    /// <summary>
    /// PostgreSQL ON CONFLICT DO NOTHING 应正确解析并往返。
    /// </summary>
    [Fact]
    public void Insert_OnConflictDoNothing_ShouldRoundTrip()
    {
        var stmt = (Insert)CCJSqlParserUtil.Parse(
            "INSERT INTO users (id, name) VALUES (1, 'a') ON CONFLICT DO NOTHING")!;
        Assert.NotNull(stmt.ConflictAction);
        Assert.Equal(ConflictActionType.DoNothing, stmt.ConflictAction!.ConflictActionType);
        Assert.Null(stmt.ConflictTarget);
        Assert.Contains("ON CONFLICT DO NOTHING", stmt.ToString()!);
    }

    [Fact]
    public void Insert_OnConflictColumnDoNothing_ShouldHaveConflictTarget()
    {
        var stmt = (Insert)CCJSqlParserUtil.Parse(
            "INSERT INTO users (id, name) VALUES (1, 'a') ON CONFLICT (id) DO NOTHING")!;
        Assert.NotNull(stmt.ConflictTarget);
        Assert.Contains("id", stmt.ConflictTarget!.IndexColumnNames);
        Assert.Null(stmt.ConflictTarget.ConstraintName);
        Assert.Contains("ON CONFLICT (id) DO NOTHING", stmt.ToString()!);
    }

    [Fact]
    public void Insert_OnConflictMultiColumn_ShouldHaveAllColumns()
    {
        var stmt = (Insert)CCJSqlParserUtil.Parse(
            "INSERT INTO t (a, b) VALUES (1, 2) ON CONFLICT (a, b) DO NOTHING")!;
        Assert.Equal(2, stmt.ConflictTarget!.IndexColumnNames.Count);
    }

    [Fact]
    public void Insert_OnConflictConstraint_ShouldHaveConstraintName()
    {
        var stmt = (Insert)CCJSqlParserUtil.Parse(
            "INSERT INTO t (a) VALUES (1) ON CONFLICT ON CONSTRAINT uniq_a DO NOTHING")!;
        Assert.Equal("uniq_a", stmt.ConflictTarget!.ConstraintName);
        Assert.Contains("ON CONFLICT ON CONSTRAINT uniq_a DO NOTHING", stmt.ToString()!);
    }

    [Fact]
    public void Insert_OnConflictDoUpdate_ShouldHaveUpdateSets()
    {
        var stmt = (Insert)CCJSqlParserUtil.Parse(
            "INSERT INTO users (id, name) VALUES (1, 'a') ON CONFLICT (id) DO UPDATE SET name = 'b'")!;
        Assert.Equal(ConflictActionType.DoUpdate, stmt.ConflictAction!.ConflictActionType);
        Assert.NotNull(stmt.ConflictAction.UpdateSets);
        Assert.Single(stmt.ConflictAction.UpdateSets!);
        Assert.Contains("DO UPDATE SET name = 'b'", stmt.ToString()!);
    }

    [Fact]
    public void Insert_OnConflictDoUpdateWithWhere_ShouldHaveWhere()
    {
        var stmt = (Insert)CCJSqlParserUtil.Parse(
            "INSERT INTO users (id, name) VALUES (1, 'a') ON CONFLICT (id) DO UPDATE SET name = 'b' WHERE users.id > 0")!;
        Assert.NotNull(stmt.ConflictAction!.WhereExpression);
        Assert.Contains("WHERE users.id > 0", stmt.ToString()!);
    }

    [Fact]
    public void Insert_OnConflictTargetWithWhere_ShouldHaveTargetWhere()
    {
        var stmt = (Insert)CCJSqlParserUtil.Parse(
            "INSERT INTO t (a) VALUES (1) ON CONFLICT (a) WHERE a > 0 DO NOTHING")!;
        Assert.NotNull(stmt.ConflictTarget!.WhereExpression);
    }

    #endregion
}
