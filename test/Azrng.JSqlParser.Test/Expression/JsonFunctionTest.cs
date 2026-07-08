using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// JSON_OBJECT / JSON_ARRAY 标量函数测试（移植自上游 JsonFunctionTest）。
/// 注意上游 OBJECT 输出 "JSON_OBJECT( " + " )"（不对称空格），ARRAY 输出 "JSON_ARRAY( " + ")"。
/// </summary>
public class JsonFunctionTest
{
    [Fact]
    public void JsonObject_ColonSeparator_ShouldParse()
    {
        // 'foo' : bar 带空格，: 是独立 COLON（避免 :bar 被识别为命名参数）
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT JSON_OBJECT( 'foo' : bar) FROM dual")!;
        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Equal(JsonFunction.FunctionType.OBJECT, func.Type);
        Assert.Single(func.KeyValuePairs);
        Assert.Equal(JsonKeyValuePair.SeparatorKind.COLON, func.KeyValuePairs[0].Separator);
    }

    /// <summary>
    /// BL-05：JSON_OBJECT 无空格冒号形式（key:bar）应可解析。
    /// 此前 :bar 被词法分析为 S_JDBC_NAMED_PARAM（lexer 最大匹配），导致解析失败。
    /// </summary>
    [Theory]
    [InlineData("SELECT JSON_OBJECT(foo:bar) FROM dual")]
    [InlineData("SELECT JSON_OBJECT('foo':bar) FROM dual")]
    [InlineData("SELECT JSON_OBJECT('foo':'bar') FROM dual")]
    public void JsonObject_NoSpaceColon_ShouldParse(string sql)
    {
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Equal(JsonKeyValuePair.SeparatorKind.COLON, func.KeyValuePairs[0].Separator);
    }

    [Fact]
    public void JsonObject_KeyValueKeyword_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_OBJECT( KEY 'foo' VALUE bar ) FROM dual";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.True(func.KeyValuePairs[0].UsingKeyKeyword);
        Assert.Equal(JsonKeyValuePair.SeparatorKind.VALUE, func.KeyValuePairs[0].Separator);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonObject_MultiplePairs_FormatJson_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_OBJECT( KEY 'foo' VALUE bar, KEY 'foo' VALUE bar FORMAT JSON ) FROM dual";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Equal(2, func.KeyValuePairs.Count);
        Assert.True(func.KeyValuePairs[1].UsingFormatJson);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonObject_NullOnNullStrictUniqueKeys_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_OBJECT( KEY 'foo' VALUE bar, KEY 'fob' VALUE baz NULL ON NULL STRICT WITH UNIQUE KEYS ) FROM dual";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Equal(JsonFunction.OnNullType.NULL, func.OnNull);
        Assert.True(func.Strict);
        Assert.Equal(JsonFunction.UniqueKeysType.WITH, func.UniqueKeys);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonObject_AbsentOnNullWithoutUniqueKeys_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_OBJECT( KEY 'foo' VALUE bar ABSENT ON NULL WITHOUT UNIQUE KEYS ) FROM dual";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Equal(JsonFunction.OnNullType.ABSENT, func.OnNull);
        Assert.Equal(JsonFunction.UniqueKeysType.WITHOUT, func.UniqueKeys);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonObject_ReturningClause_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_OBJECT( KEY 'x' VALUE 1 RETURNING VARCHAR FORMAT JSON ENCODING UTF32 ) FROM dual";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Equal("VARCHAR", func.ReturningType);
        Assert.True(func.ReturningFormatJson);
        Assert.Equal("UTF32", func.ReturningEncoding);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonArray_Simple_ShouldRoundTrip()
    {
        // ARRAY 输出 "JSON_ARRAY( " + 元素 + ")"（末尾无空格）
        var sql = "SELECT JSON_ARRAY( 1, 2, 3) FROM dual";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Equal(JsonFunction.FunctionType.ARRAY, func.Type);
        Assert.Equal(3, func.Expressions.Count);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonArray_NullOnNull_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_ARRAY( 1 NULL ON NULL) FROM dual";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Equal(JsonFunction.OnNullType.NULL, func.OnNull);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonArray_Empty_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_ARRAY() FROM dual";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Empty(func.Expressions);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonArray_ReturningClause_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_ARRAY( 1, 2 RETURNING VARBINARY FORMAT JSON ENCODING UTF16) FROM dual";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Equal("VARBINARY", func.ReturningType);
        Assert.True(func.ReturningFormatJson);
        Assert.Equal("UTF16", func.ReturningEncoding);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonValue_Simple_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_VALUE(payload, '$.customer.id') FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Equal(JsonFunction.FunctionType.VALUE, func.Type);
        Assert.Equal("'$.customer.id'", func.JsonPathExpression?.ToString());
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonValue_ReturningAndOnError_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_VALUE(payload, '$.x' RETURNING VARCHAR NULL ON ERROR) FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.NotNull(func.OnErrorBehavior);
        Assert.Equal(JsonFunction.OnResponseBehaviorType.NULL, func.OnErrorBehavior!.Type);
        Assert.Equal("VARCHAR", func.ReturningType);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonExists_Simple_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_EXISTS(payload, '$.children[2]') FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Equal(JsonFunction.FunctionType.EXISTS, func.Type);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonExists_OnErrorUnknown_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_EXISTS(payload, '$.children[2]' UNKNOWN ON ERROR) FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Equal(JsonFunction.OnResponseBehaviorType.UNKNOWN, func.OnErrorBehavior!.Type);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonQuery_Simple_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_QUERY(payload, '$.items') FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Equal(JsonFunction.FunctionType.QUERY, func.Type);
        Assert.Equal(sql, select.ToString());
    }

    /// <summary>
    /// BL-01：JSON_QUERY Legacy 多 path 参数（MySQL/Oracle: JSON_QUERY(input, path1, path2...)）
    /// 应正确 round-trip，对齐上游 additionalQueryPathArguments。
    /// </summary>
    [Fact]
    public void JsonQuery_LegacyMultiplePaths_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_QUERY(payload, '$.a', '$.b', '$.c') FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Equal(2, func.AdditionalQueryPathArguments.Count);
        Assert.Equal("'$.b'", func.AdditionalQueryPathArguments[0]);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonQuery_WithWrapperAndQuotes_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_QUERY(payload, '$.items' WITH ARRAY WRAPPER KEEP QUOTES) FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.Equal(JsonFunction.WrapperType.WITH, func.Wrapper);
        Assert.True(func.WrapperArray);
        Assert.Equal(JsonFunction.QuotesType.KEEP, func.Quotes);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonQuery_OnErrorEmptyArray_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_QUERY(payload, '$.items' EMPTY ARRAY ON EMPTY) FROM t";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonFunction>(select.SelectItems![0].Expression);
        Assert.NotNull(func.OnEmptyBehavior);
        Assert.Equal(JsonFunction.OnResponseBehaviorType.EMPTY_ARRAY, func.OnEmptyBehavior!.Type);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonObjectAgg_ColonSpaced_ShouldParse()
    {
        // foo : bar 带空格，避免 :bar 被识别为命名参数
        var select = (PlainSelect)CCJSqlParserUtil.Parse("SELECT JSON_OBJECTAGG( foo : bar) FROM dual")!;
        var func = Assert.IsType<JsonAggregateFunction>(select.SelectItems![0].Expression);
        Assert.Equal(JsonAggregateFunction.AggregateType.OBJECT, func.AggregateFunctionType);
        Assert.False(func.UsingValueSeparator);
    }

    [Fact]
    public void JsonObjectAgg_KeyValueKeyword_ShouldRoundTrip()
    {
        // OBJECTAGG 输出 " )"（末尾带空格，与上游 OBJECT 对齐）
        var sql = "SELECT JSON_OBJECTAGG( KEY foo VALUE bar ) FROM dual";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonAggregateFunction>(select.SelectItems![0].Expression);
        Assert.True(func.UsingKeyKeyword);
        Assert.True(func.UsingValueSeparator);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonObjectAgg_NullOnNullWithUniqueKeys_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_OBJECTAGG( KEY foo VALUE bar NULL ON NULL WITH UNIQUE KEYS ) FROM dual";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonAggregateFunction>(select.SelectItems![0].Expression);
        Assert.Equal(JsonFunction.OnNullType.NULL, func.OnNull);
        Assert.Equal(JsonFunction.UniqueKeysType.WITH, func.UniqueKeys);
        Assert.Equal(sql, select.ToString());
    }

    /// <summary>
    /// BL-04：JSON_OBJECTAGG 逗号分隔（MySQL 风格 key,value）应正确 round-trip，
    /// 不再静默退化为冒号（对齐上游 MYSQL_OBJECT）。
    /// </summary>
    [Fact]
    public void JsonObjectAgg_CommaSeparator_ShouldRoundTrip()
    {
        // 注意：OBJECTAGG 输出 " foo, bar "（与冒号/VALUE 分支一致的括号内空格风格）
        var sql = "SELECT JSON_OBJECTAGG( foo, bar ) FROM dual";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        var func = Assert.IsType<JsonAggregateFunction>(select.SelectItems![0].Expression);
        Assert.True(func.UseCommaSeparator);
        Assert.False(func.UsingValueSeparator);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonArrayAgg_Simple_ShouldRoundTrip()
    {
        // ARRAYAGG 输出 "JSON_ARRAYAGG( a)"（a 后无空格）
        var sql = "SELECT JSON_ARRAYAGG( a) FROM dual";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonAggregateFunction>(select.SelectItems![0].Expression);
        Assert.Equal(JsonAggregateFunction.AggregateType.ARRAY, func.AggregateFunctionType);
        Assert.Equal(sql, select.ToString());
    }

    [Fact]
    public void JsonArrayAgg_OrderByAndOnNull_ShouldRoundTrip()
    {
        var sql = "SELECT JSON_ARRAYAGG( a ORDER BY a NULL ON NULL) FROM dual";
        var select = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;

        var func = Assert.IsType<JsonAggregateFunction>(select.SelectItems![0].Expression);
        Assert.NotNull(func.OrderByElements);
        Assert.Single(func.OrderByElements);
        Assert.Equal(JsonFunction.OnNullType.NULL, func.OnNull);
        Assert.Equal(sql, select.ToString());
    }
}
