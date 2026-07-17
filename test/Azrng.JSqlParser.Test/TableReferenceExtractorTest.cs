using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Models;
using SelectStatement = Azrng.JSqlParser.Statement.Select.Select;

namespace Azrng.JSqlParser.Test;

/// <summary>
/// GetTableReferences 测试 — 提取 SELECT 中所有表引用（含别名映射、子查询、JOIN）。
/// </summary>
public class TableReferenceExtractorTest
{
    private static SelectStatement ParseSelect(string sql) => (SelectStatement)CCJSqlParserUtil.Parse(sql)!;

    [Fact]
    public void GetTableReferences_SingleTable_ShouldReturnNameAndAlias()
    {
        var refs = ParseSelect("SELECT id FROM users u").GetTableReferences();
        var table = Assert.Single(refs);
        Assert.Equal("users", table.Name);
        Assert.Equal("u", table.Alias);
    }

    [Fact]
    public void GetTableReferences_NoAlias_ShouldHaveNullAlias()
    {
        var refs = ParseSelect("SELECT id FROM users").GetTableReferences();
        var table = Assert.Single(refs);
        Assert.Equal("users", table.Name);
        Assert.Null(table.Alias);
        Assert.Equal("users", table.Key); // 无别名时 Key 取表名
    }

    [Fact]
    public void GetTableReferences_Join_ShouldReturnAllTables()
    {
        var refs = ParseSelect(
            "SELECT a.id FROM users a INNER JOIN orders b ON a.id = b.uid").GetTableReferences();
        Assert.Equal(2, refs.Count);
        Assert.Contains(refs, r => r.Name == "users" && r.Alias == "a");
        Assert.Contains(refs, r => r.Name == "orders" && r.Alias == "b");
    }

    [Fact]
    public void GetTableReferences_SelfJoin_ShouldReturnBothOccurrences()
    {
        // 自连接：同表两次出现，不去重（业务方自行 DistinctBy）
        var refs = ParseSelect(
            "SELECT x.id FROM users x JOIN users y ON x.id = y.pid").GetTableReferences();
        Assert.Equal(2, refs.Count);
        Assert.All(refs, r => Assert.Equal("users", r.Name));
    }

    [Fact]
    public void GetTableReferences_FromSubquery_ShouldReturnInnerTable()
    {
        var refs = ParseSelect(
            "SELECT id FROM (SELECT uid FROM orders) sub").GetTableReferences();
        // FROM 子查询内的 orders 表被提取；外层 sub 是别名不是表
        Assert.Contains(refs, r => r.Name == "orders");
    }

    [Fact]
    public void GetTableReferences_WhereSubqueryNotIncluded_ShouldOnlyReturnFromTables()
    {
        // WHERE 中 IN 子查询的表不属于 FROM 引用，不在 GetTableResults 范围
        // （需要 WHERE 子查询的表请用 GetTableNames，它遍历全部表达式）
        var refs = ParseSelect(
            "SELECT id FROM main WHERE id IN (SELECT id FROM logs)").GetTableReferences();
        var table = Assert.Single(refs);
        Assert.Equal("main", table.Name);
    }

    [Fact]
    public void GetTableReferences_Cte_ShouldTraverseWithClause()
    {
        var refs = ParseSelect(
            "WITH cte AS (SELECT id FROM base) SELECT id FROM cte").GetTableReferences();
        // CTE 定义里的 base 表，以及外层引用的 cte
        Assert.Contains(refs, r => r.Name == "base");
    }

    [Fact]
    public void GetTableReferences_Union_ShouldReturnAllBranches()
    {
        var refs = ParseSelect(
            "SELECT id FROM a UNION SELECT id FROM b").GetTableReferences();
        Assert.Contains(refs, r => r.Name == "a");
        Assert.Contains(refs, r => r.Name == "b");
    }

    [Fact]
    public void GetTableReferences_SchemaQualified_ShouldReturnFullName()
    {
        var refs = ParseSelect("SELECT id FROM dbo.users").GetTableReferences();
        var table = Assert.Single(refs);
        Assert.Equal("users", table.Name);
        Assert.Contains("dbo", table.FullName);
    }

    [Fact]
    public void GetTableReferences_NonSelect_ShouldReturnEmpty()
    {
        var stmt = CCJSqlParserUtil.Parse("INSERT INTO users VALUES (1)")!;
        Assert.Empty(stmt.GetTableReferences());
    }

    [Fact]
    public void GetTableReferences_OnNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((Azrng.JSqlParser.Statement.Statement)null!).GetTableReferences());
    }
}
