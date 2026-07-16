using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement;
using Azrng.JSqlParser.Statement.Select;
using Azrng.JSqlParser.Util;

namespace Azrng.JSqlParser.Test;

/// <summary>
/// 语句扩展方法（ExtractTableNames / Descendants / Walk）测试。
/// ExtractTableNames 验证与旧 TablesNamesFinder.GetTables 结果等价；
/// Descendants/Walk 验证语句层遍历与嵌套子语句递归。
/// </summary>
public class StatementExtensionTest
{
    // ---------- ExtractTableNames：与旧 GetTables 等价 ----------

    [Fact]
    public void ExtractTableNames_SimpleSelect_ShouldEqualLegacyGetTables()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM users")!;
        var viaExtension = stmt.ExtractTableNames();

#pragma warning disable CS0618 // 等价对照
        var viaLegacy = new TablesNamesFinder().GetTables(stmt);
#pragma warning restore CS0618

        Assert.Equal(viaLegacy.OrderBy(n => n), viaExtension.OrderBy(n => n));
        Assert.Contains("users", viaExtension);
    }

    [Fact]
    public void ExtractTableNames_SelectWithJoin_ShouldReturnBothTables()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT u.id, o.total FROM users u INNER JOIN orders o ON u.id = o.user_id")!;
        var tables = stmt.ExtractTableNames();
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
    }

    [Fact]
    public void ExtractTableNames_WithSubquery_ShouldReturnInnerAndOuterTables()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM (SELECT uid FROM orders) sub WHERE id IN (SELECT id FROM logs)")!;
        var tables = stmt.ExtractTableNames();
        Assert.Contains("orders", tables);
        Assert.Contains("logs", tables);
    }

    [Fact]
    public void ExtractTableNames_TableStatement_ShouldReturnTable()
    {
        var stmt = CCJSqlParserUtil.Parse("TABLE users")!;
        var tables = stmt.ExtractTableNames();
        Assert.Contains("users", tables);
    }

    [Fact]
    public void ExtractTableNames_ShouldReturnReadOnlyCollection()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM users")!;
        var tables = stmt.ExtractTableNames();
        Assert.IsAssignableFrom<IReadOnlyCollection<string>>(tables);
    }

    // ---------- ExtractTableNames：DML 语句 ----------

    [Fact]
    public void ExtractTableNames_Insert_ShouldReturnTargetTable()
    {
        var stmt = CCJSqlParserUtil.Parse("INSERT INTO users (name) VALUES ('Alice')")!;
        Assert.Contains("users", stmt.ExtractTableNames());
    }

    [Fact]
    public void ExtractTableNames_InsertFromSelect_ShouldReturnBothTables()
    {
        var stmt = CCJSqlParserUtil.Parse("INSERT INTO target SELECT id FROM source")!;
        var tables = stmt.ExtractTableNames();
        Assert.Contains("target", tables);
        Assert.Contains("source", tables);
    }

    [Fact]
    public void ExtractTableNames_Update_ShouldReturnTable()
    {
        var stmt = CCJSqlParserUtil.Parse("UPDATE users SET name = 'Bob' WHERE id = 1")!;
        Assert.Contains("users", stmt.ExtractTableNames());
    }

    [Fact]
    public void ExtractTableNames_Delete_ShouldReturnTable()
    {
        var stmt = CCJSqlParserUtil.Parse("DELETE FROM logs WHERE expired = 1")!;
        Assert.Contains("logs", stmt.ExtractTableNames());
    }

    [Fact]
    public void ExtractTableNames_Merge_ShouldReturnTargetAndSource()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "MERGE INTO target t USING source s ON t.id = s.id WHEN MATCHED THEN UPDATE SET t.name = s.name")!;
        var tables = stmt.ExtractTableNames();
        Assert.Contains("target", tables);
        Assert.Contains("source", tables);
    }

    [Fact]
    public void ExtractTableNames_ShouldDeduplicateRepeatedTables()
    {
        // 同一表出现多次（自连接不同别名）—— 表名去重
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT a.id FROM users a JOIN users b ON a.id = b.parent_id")!;
        var tables = stmt.ExtractTableNames();
        var count = tables.Count(t => t == "users");
        Assert.Equal(1, count);
    }

    // ---------- Descendants<TStatement>：嵌套子语句递归 ----------

    [Fact]
    public void Descendants_OfSelect_WithUnion_ShouldCollectAllBranches()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM a UNION SELECT id FROM b UNION SELECT id FROM c")!;
        // 根是 SetOperationList，内部含 3 个 PlainSelect
        var plainSelects = stmt.Descendants<PlainSelect>().ToList();
        // SetOperationList 本身不是 PlainSelect，3 个分支都是
        Assert.Equal(3, plainSelects.Count);
    }

    [Fact]
    public void Descendants_OfStatements_ShouldCollectNestedStatements()
    {
        var stmt = CCJSqlParserUtil.ParseStatements(
            "SELECT id FROM a; SELECT id FROM b; INSERT INTO c VALUES (1)")!;
        var selects = stmt.Descendants<Select>().ToList();
        Assert.Equal(2, selects.Count);
    }

    // ---------- Walk<TStatement> ----------

    [Fact]
    public void Walk_OfSelect_ShouldInvokeActionForEachSelect()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "SELECT id FROM a UNION SELECT id FROM b")!;
        var count = 0;
        stmt.Walk<Select>(_ => count++);
        // 根 SetOperationList(1) + 2 个 PlainSelect 分支 = 3 个 Select 节点
        Assert.Equal(3, count);
    }

    // ---------- 语句层 Descendants/Walk 边界 ----------

    [Fact]
    public void Descendants_OfInsert_ShouldCollectSelf()
    {
        var stmt = CCJSqlParserUtil.Parse("INSERT INTO users VALUES (1)")!;
        var inserts = stmt.Descendants<Azrng.JSqlParser.Statement.Insert.Insert>().ToList();
        Assert.Single(inserts);
    }

    [Fact]
    public void Descendants_OfNonExistentType_ShouldReturnEmpty()
    {
        // 单条 SELECT 语句中没有 UPDATE 节点
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM a")!;
        Assert.Empty(stmt.Descendants<Azrng.JSqlParser.Statement.Update.Update>());
    }

    [Fact]
    public void Walk_ShouldCollectFromMultipleStatementsContainer()
    {
        var stmt = CCJSqlParserUtil.ParseStatements(
            "UPDATE a SET x = 1; DELETE FROM b; UPDATE c SET y = 2")!;
        var updates = new List<Azrng.JSqlParser.Statement.Update.Update>();
        stmt.Walk<Azrng.JSqlParser.Statement.Update.Update>(updates.Add);
        Assert.Equal(2, updates.Count);
    }

    [Fact]
    public void Descendants_OnNullStatement_ShouldThrow()
    {
        Azrng.JSqlParser.Statement.Statement stmt = null!;
        Assert.Throws<ArgumentNullException>(() => stmt.Descendants<Select>());
    }

    [Fact]
    public void Walk_OnNullStatement_ShouldThrow()
    {
        Azrng.JSqlParser.Statement.Statement stmt = null!;
        Assert.Throws<ArgumentNullException>(() => stmt.Walk<Select>(_ => { }));
    }

    [Fact]
    public void Walk_OnNullAction_ShouldThrow()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM a")!;
        Assert.Throws<ArgumentNullException>(() => stmt.Walk<Select>(null!));
    }

    [Fact]
    public void ExtractTableNames_OnNullStatement_ShouldThrow()
    {
        Azrng.JSqlParser.Statement.Statement stmt = null!;
        Assert.Throws<ArgumentNullException>(() => stmt.ExtractTableNames());
    }
}
