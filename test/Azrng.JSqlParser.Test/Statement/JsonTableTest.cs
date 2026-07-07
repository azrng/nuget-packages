using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// JSON_TABLE 表函数测试（SQL/JSON）。
/// 移植自上游 JSqlParser commit c5e2fdcd，简化为核心语法，适配为 xUnit。
/// </summary>
public class JsonTableTest
{
    [Fact]
    public void JsonTable_MinimalWithOrdinality_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' COLUMNS (id FOR ORDINALITY)) AS jt";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.FromItem);
        Assert.Single(jsonTable.Columns);
        Assert.True(jsonTable.Columns[0].ForOrdinality);
        Assert.Equal("jt", jsonTable.Alias!.Name);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonTable_WithTypedColumnsAndPaths_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE(d, '$.path' COLUMNS (id INT PATH '$.id', name VARCHAR(100) PATH '$.name')) jt";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.FromItem);
        Assert.Equal(2, jsonTable.Columns.Count);
        Assert.Equal("id", jsonTable.Columns[0].Name);
        Assert.False(jsonTable.Columns[0].ForOrdinality);
        Assert.Equal("'$.id'", jsonTable.Columns[0].Path);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonTable_WithoutPath_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE(d COLUMNS (id FOR ORDINALITY, val INT)) jt";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.FromItem);
        Assert.Equal(2, jsonTable.Columns.Count);
        Assert.True(jsonTable.Columns[0].ForOrdinality);
        Assert.False(jsonTable.Columns[1].ForOrdinality);
        Assert.Equal("INT", jsonTable.Columns[1].DataType);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonTable_ColumnWithoutPath_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE(d, '$' COLUMNS (id INT, name VARCHAR(50))) AS jt";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.FromItem);
        Assert.Equal(2, jsonTable.Columns.Count);
        Assert.Null(jsonTable.Columns[0].Path);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonTable_InJoin_ShouldRoundTrip()
    {
        var sql = "SELECT jt.id FROM t INNER JOIN JSON_TABLE(t.data, '$' COLUMNS (id INT PATH '$.id')) jt ON 1 = 1";
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void JsonTable_WithPassingClause_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' PASSING 5 AS x COLUMNS (id FOR ORDINALITY)) AS jt";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.FromItem);
        Assert.Single(jsonTable.PassingClauses);
        Assert.Equal("x", jsonTable.PassingClauses[0].ParameterName);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonTable_WithOnErrorNull_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' NULL ON ERROR COLUMNS (id FOR ORDINALITY)) AS jt";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.FromItem);
        Assert.Equal("NULL", jsonTable.OnErrorBehavior);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonTable_WithNestedPath_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' COLUMNS (id FOR ORDINALITY, NESTED PATH '$.items' COLUMNS (item_id INT PATH '$.id'))) AS jt";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.FromItem);
        Assert.Equal(2, jsonTable.Columns.Count);
        Assert.True(jsonTable.Columns[1].IsNested);
        Assert.Single(jsonTable.Columns[1].NestedColumns!);
        Assert.Equal("'$.items'", jsonTable.Columns[1].Path);
        Assert.Equal(sql, select.ToString());
    }
}
