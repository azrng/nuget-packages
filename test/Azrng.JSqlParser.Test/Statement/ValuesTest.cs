using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// VALUES 表构造器测试（T097）。
/// 对齐上游 net.sf.jsqlparser.statement.select.Values（commit 2b141568）。
/// 覆盖三档：独立 SELECT 语句、集合运算操作数、FROM 子项。
/// </summary>
public class ValuesTest
{
    /// <summary>独立 VALUES 语句：VALUES (1, 'a'), (2, 'b') round-trip。</summary>
    [Fact]
    public void Values_Standalone_RoundTrip()
    {
        var sql = "VALUES (1, 'a'), (2, 'b')";
        var stmt = CCJSqlParserUtil.Parse(sql);

        var values = Assert.IsType<Values>(stmt);
        Assert.Equal(2, values.Rows.Count);
        Assert.Equal(2, values.Rows[0].Expressions.Count);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>单行 VALUES round-trip。</summary>
    [Fact]
    public void Values_SingleRow_RoundTrip()
    {
        var sql = "VALUES (1, 2, 3)";
        var stmt = CCJSqlParserUtil.Parse(sql);

        var values = Assert.IsType<Values>(stmt);
        Assert.Single(values.Rows);
        Assert.Equal(3, values.Rows[0].Expressions.Count);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>VALUES 作为 UNION 操作数：VALUES (1) UNION VALUES (2)。</summary>
    [Fact]
    public void Values_Union_Values_RoundTrip()
    {
        var sql = "VALUES (1) UNION VALUES (2)";
        var stmt = CCJSqlParserUtil.Parse(sql);

        var setOpList = Assert.IsType<SetOperationList>(stmt);
        Assert.Equal(2, setOpList.Selects.Count);
        Assert.All(setOpList.Selects, s => Assert.IsType<Values>(s));
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>VALUES UNION ALL VALUES round-trip。</summary>
    [Fact]
    public void Values_UnionAll_Values_RoundTrip()
    {
        var sql = "VALUES (1, 'x') UNION ALL VALUES (2, 'y')";
        var stmt = CCJSqlParserUtil.Parse(sql);

        var setOpList = Assert.IsType<SetOperationList>(stmt);
        Assert.Equal(2, setOpList.Selects.Count);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>FROM 子项带别名（PostgreSQL 风格）：SELECT * FROM (VALUES (1, 2)) AS t。
    /// 注：ParenthesedSelect 的 Alias 序列化不含 AS（既定行为），round-trip 输出为 ") t"。</summary>
    [Fact]
    public void Values_AsFromItem_WithAlias_RoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT * FROM (VALUES (1, 2)) AS t");

        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var parenthesedSelect = Assert.IsType<ParenthesedSelect>(plainSelect.FromItem);
        Assert.IsType<Values>(parenthesedSelect.Select);
        Assert.NotNull(parenthesedSelect.Alias);
        Assert.Equal("SELECT * FROM (VALUES (1, 2)) t", stmt!.ToString());
    }

    /// <summary>FROM 子项：SELECT * FROM (VALUES (1), (2)) t。</summary>
    [Fact]
    public void Values_AsFromItem_MultiRow_RoundTrip()
    {
        var sql = "SELECT * FROM (VALUES (1), (2)) t";
        var stmt = CCJSqlParserUtil.Parse(sql);

        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var parenthesedSelect = Assert.IsType<ParenthesedSelect>(plainSelect.FromItem);
        var values = Assert.IsType<Values>(parenthesedSelect.Select);
        Assert.Equal(2, values.Rows.Count);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>VALUES 语句带 ORDER BY / LIMIT 修饰符。</summary>
    [Fact]
    public void Values_WithOrderByAndLimit_RoundTrip()
    {
        var sql = "VALUES (1), (2) ORDER BY 1 LIMIT 1";
        var stmt = CCJSqlParserUtil.Parse(sql);

        var values = Assert.IsType<Values>(stmt);
        Assert.NotNull(values.OrderByElements);
        Assert.NotNull(values.Limit);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>VALUES 行内含表达式（函数、运算），非仅字面量。</summary>
    [Fact]
    public void Values_WithExpressions_RoundTrip()
    {
        var sql = "VALUES (1 + 1, UPPER('a')), (ABS(-3), 'b')";
        var stmt = CCJSqlParserUtil.Parse(sql);

        var values = Assert.IsType<Values>(stmt);
        Assert.Equal(2, values.Rows.Count);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>NULL 值在 VALUES 行中正确解析。</summary>
    [Fact]
    public void Values_WithNull_RoundTrip()
    {
        var sql = "VALUES (1, NULL), (NULL, 2)";
        var stmt = CCJSqlParserUtil.Parse(sql);

        var values = Assert.IsType<Values>(stmt);
        Assert.Equal(2, values.Rows.Count);
        Assert.Equal(sql, stmt!.ToString());
    }
}
