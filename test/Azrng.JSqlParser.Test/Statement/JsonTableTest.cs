using Azrng.JSqlParser.Expression;
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
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.IFromItem);
        Assert.Single(jsonTable.Columns);
        Assert.True(jsonTable.Columns[0].ForOrdinality);
        Assert.Equal("jt", jsonTable.Alias!.Name);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonTable_WithTypedColumnsAndPaths_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE(d, '$.path' COLUMNS (id INT PATH '$.id', name VARCHAR(100) PATH '$.name')) jt";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.IFromItem);
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
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.IFromItem);
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
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.IFromItem);
        Assert.Equal(2, jsonTable.Columns.Count);
        Assert.Null(jsonTable.Columns[0].Path);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonTable_InJoin_ShouldRoundTrip()
    {
        var sql = "SELECT jt.id FROM t INNER JOIN JSON_TABLE(t.data, '$' COLUMNS (id INT PATH '$.id')) jt ON 1 = 1";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void JsonTable_WithPassingClause_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' PASSING 5 AS x COLUMNS (id FOR ORDINALITY)) AS jt";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.IFromItem);
        Assert.Single(jsonTable.PassingClauses);
        Assert.Equal("x", jsonTable.PassingClauses[0].ParameterName);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonTable_WithOnErrorNull_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' NULL ON ERROR COLUMNS (id FOR ORDINALITY)) AS jt";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.IFromItem);
        Assert.Equal(JsonFunction.OnResponseBehaviorType.NULL, jsonTable.OnErrorBehavior!.Type);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonTable_WithNestedPath_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' COLUMNS (id FOR ORDINALITY, NESTED PATH '$.items' COLUMNS (item_id INT PATH '$.id'))) AS jt";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.IFromItem);
        Assert.Equal(2, jsonTable.Columns.Count);
        Assert.True(jsonTable.Columns[1].IsNested);
        Assert.Single(jsonTable.Columns[1].NestedColumns!);
        Assert.Equal("'$.items'", jsonTable.Columns[1].Path);
        Assert.Equal(sql, select.ToString());
    }

    #region BL-02 Oracle/Trino 方言子句

    /// <summary>BL-02：函数级 ON EMPTY 行为。</summary>
    [Fact]
    public void JsonTable_FunctionLevel_OnEmpty_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' NULL ON EMPTY COLUMNS (id FOR ORDINALITY)) AS jt";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.IFromItem);
        Assert.Equal(JsonFunction.OnResponseBehaviorType.NULL, jsonTable.OnEmptyBehavior!.Type);
        Assert.Equal(sql, select.ToString());
    }

    /// <summary>BL-02：函数级 EMPTY ON ERROR（比原 NULL/ERROR 更丰富）。</summary>
    [Fact]
    public void JsonTable_FunctionLevel_EmptyOnError_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' EMPTY ON ERROR COLUMNS (id FOR ORDINALITY)) AS jt";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.IFromItem);
        Assert.Equal(JsonFunction.OnResponseBehaviorType.EMPTY, jsonTable.OnErrorBehavior!.Type);
        Assert.Equal(sql, select.ToString());
    }

    /// <summary>BL-02：函数级 TYPE (STRICT|LAX)。</summary>
    [Fact]
    public void JsonTable_FunctionLevel_TypeStrict_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' TYPE (STRICT) COLUMNS (id FOR ORDINALITY)) AS jt";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.IFromItem);
        Assert.Equal("STRICT", jsonTable.ParsingType);
        Assert.Equal(sql, select.ToString());
    }

    /// <summary>BL-02：函数级 FORMAT JSON 输入（Oracle）。</summary>
    [Fact]
    public void JsonTable_FunctionLevel_FormatJsonInput_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}' FORMAT JSON, '$' COLUMNS (id FOR ORDINALITY)) AS jt";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var jsonTable = Assert.IsType<JsonTable>(select.IFromItem);
        Assert.True(jsonTable.InputFormatJson);
        Assert.Equal(sql, select.ToString());
    }

    /// <summary>BL-02：列级 EXISTS（Oracle）。</summary>
    [Fact]
    public void JsonTable_ColumnLevel_Exists_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' COLUMNS (id INT EXISTS PATH '$.id')) AS jt";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var col = Assert.IsType<JsonTable>(select.IFromItem).Columns[0];
        Assert.True(col.Exists);
        Assert.Equal(sql, select.ToString());
    }

    /// <summary>BL-02：列级 FORMAT JSON（PATH 在前，对齐上游顺序）。</summary>
    [Fact]
    public void JsonTable_ColumnLevel_FormatJson_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' COLUMNS (id VARCHAR(100) PATH '$.id' FORMAT JSON)) AS jt";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var col = Assert.IsType<JsonTable>(select.IFromItem).Columns[0];
        Assert.True(col.FormatJson);
        Assert.Equal(sql, select.ToString());
    }

    /// <summary>BL-02：列级 WRAPPER 子句。</summary>
    [Fact]
    public void JsonTable_ColumnLevel_Wrapper_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' COLUMNS (id VARCHAR(100) PATH '$.id' WITH ARRAY WRAPPER)) AS jt";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var col = Assert.IsType<JsonTable>(select.IFromItem).Columns[0];
        Assert.Equal(JsonFunction.WrapperType.WITH, col.Wrapper);
        Assert.True(col.WrapperArray);
        Assert.Equal(sql, select.ToString());
    }

    /// <summary>BL-02：列级 QUOTES 子句。</summary>
    [Fact]
    public void JsonTable_ColumnLevel_Quotes_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' COLUMNS (id VARCHAR(100) PATH '$.id' KEEP QUOTES)) AS jt";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var col = Assert.IsType<JsonTable>(select.IFromItem).Columns[0];
        Assert.Equal(JsonFunction.QuotesType.KEEP, col.Quotes);
        Assert.Equal(sql, select.ToString());
    }

    /// <summary>BL-02：列级 (ALLOW|DISALLOW) SCALARS。</summary>
    [Fact]
    public void JsonTable_ColumnLevel_Scalars_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' COLUMNS (id VARCHAR(100) PATH '$.id' ALLOW SCALARS)) AS jt";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var col = Assert.IsType<JsonTable>(select.IFromItem).Columns[0];
        Assert.Equal(JsonFunction.ScalarsType.ALLOW, col.Scalars);
        Assert.Equal(sql, select.ToString());
    }

    /// <summary>BL-02：列级 ON EMPTY + ON ERROR（含 DEFAULT 表达式）。</summary>
    [Fact]
    public void JsonTable_ColumnLevel_OnEmptyOnError_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM JSON_TABLE('{}', '$' COLUMNS (id VARCHAR(100) PATH '$.id' NULL ON EMPTY DEFAULT 'x' ON ERROR)) AS jt";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var col = Assert.IsType<JsonTable>(select.IFromItem).Columns[0];
        Assert.Equal(JsonFunction.OnResponseBehaviorType.NULL, col.OnEmptyBehavior!.Type);
        Assert.Equal(JsonFunction.OnResponseBehaviorType.DEFAULT, col.OnErrorBehavior!.Type);
        Assert.Equal(sql, select.ToString());
    }

    #endregion
}
