using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Util;

namespace Azrng.JSqlParser.Test.Util;

/// <summary>
/// TablesNamesFinder 对 VALUES 表构造器的表名提取测试（T097 补充）。
/// 验证 VisitValues 分支正确遍历 Rows 内表达式并提取列引用中的表名。
/// </summary>
public class ValuesTablesNamesFinderTest
{
    private static HashSet<string> GetTables(string sql)
    {
        var finder = new TablesNamesFinder();
        return finder.GetTables(SqlParser.Parse(sql)!);
    }

    /// <summary>独立 VALUES 语句含列引用，应提取出表名。</summary>
    [Fact]
    public void FindTables_StandaloneValues_WithColumnRef_ShouldExtractTable()
    {
        var tables = GetTables("VALUES (users.id, users.name)");
        Assert.Contains("users", tables);
    }

    /// <summary>独立 VALUES 语句多行列引用，提取去重后的表名集合。</summary>
    [Fact]
    public void FindTables_StandaloneValues_MultiRowColumnRefs_ShouldExtractTables()
    {
        var tables = GetTables("VALUES (users.id), (orders.code, users.name)");
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
        Assert.Equal(2, tables.Count);
    }

    /// <summary>FROM (VALUES ...) 子查询内含列引用，应提取表名。</summary>
    [Fact]
    public void FindTables_FromValuesSubquery_ShouldExtractTable()
    {
        var tables = GetTables("SELECT * FROM (VALUES (users.id)) AS t");
        Assert.Contains("users", tables);
    }

    /// <summary>纯字面量 VALUES（无列引用）不应提取出任何表名。</summary>
    [Fact]
    public void FindTables_StandaloneValues_OnlyLiterals_ShouldReturnEmpty()
    {
        var tables = GetTables("VALUES (1, 'a'), (2, 'b')");
        Assert.Empty(tables);
    }

    /// <summary>VALUES UNION VALUES，两侧列引用的表名都应被提取。</summary>
    [Fact]
    public void FindTables_ValuesUnionValues_ShouldExtractBothSides()
    {
        var tables = GetTables("VALUES (users.id) UNION VALUES (orders.code)");
        Assert.Contains("users", tables);
        Assert.Contains("orders", tables);
    }
}
