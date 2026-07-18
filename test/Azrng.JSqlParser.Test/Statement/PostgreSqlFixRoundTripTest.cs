using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// PostgreSQL 专项修复的 round-trip 验证（补强探针的"能解析"为"解析且语义结构正确"）。
/// 对照 issue 分类清单 ④ 已修复项，断言 ToString 保留关键语法结构。
/// </summary>
public class PostgreSqlFixRoundTripTest
{
    [Fact]
    public void LikeAnyArray_RoundTripsQuantifier()
    {
        var sql = "SELECT * FROM t WHERE col LIKE ANY (ARRAY['%a%', '%b%'])";
        var output = SqlParser.Parse(sql)!.ToString();
        Assert.Contains("LIKE ANY", output);
    }

    [Fact]
    public void LikeAllArray_RoundTripsQuantifier()
    {
        var sql = "SELECT * FROM t WHERE col LIKE ALL (ARRAY['%a%', '%b%'])";
        var output = SqlParser.Parse(sql)!.ToString();
        Assert.Contains("LIKE ALL", output);
    }

    [Fact]
    public void IntervalHourToMinute_RoundTripsInCreateTable()
    {
        var sql = "CREATE TABLE films (len interval hour to minute)";
        var output = SqlParser.Parse(sql)!.ToString();
        Assert.Contains("interval hour to minute", output);
    }

    [Fact]
    public void FtsAtAtOperator_RoundTrips()
    {
        var sql = "SELECT to_tsvector('fat cats') @@ to_tsquery('fat & rat')";
        var output = SqlParser.Parse(sql)!.ToString();
        Assert.Contains("@@", output);
    }

    [Fact]
    public void ExplainParenOptions_RoundTrips()
    {
        var sql = "EXPLAIN (ANALYZE, VERBOSE, COSTS, BUFFERS) SELECT 1";
        var output = SqlParser.Parse(sql)!.ToString();
        Assert.Contains("ANALYZE", output);
        Assert.Contains("BUFFERS", output);
    }

    [Fact]
    public void ExplainFormatJson_RoundTrips()
    {
        var sql = "EXPLAIN (FORMAT JSON) SELECT 1";
        var output = SqlParser.Parse(sql)!.ToString();
        Assert.Contains("FORMAT JSON", output);
    }

    [Fact]
    public void WithOrdinality_RoundTrips()
    {
        var sql = "SELECT ARR.ITEM FROM t, jsonb_array_elements(d) WITH ORDINALITY ARR(ITEM, POS)";
        var output = SqlParser.Parse(sql)!.ToString();
        Assert.Contains("WITH ORDINALITY", output);
    }

    [Fact]
    public void RowsFrom_RoundTrips()
    {
        var sql = "SELECT * FROM ROWS FROM (generate_series(1,3), generate_series(10,12)) AS t(a,b)";
        var output = SqlParser.Parse(sql)!.ToString();
        Assert.Contains("ROWS FROM", output);
    }

    [Fact]
    public void RowExpansionStar_RoundTripsParens()
    {
        var sql = "SELECT (json_populate_record(NULL::users, data)).* FROM staging_users";
        var output = SqlParser.Parse(sql)!.ToString();
        // 外层括号必须保留，否则 PG 语义改变
        Assert.Contains(").*", output);
    }

    [Fact]
    public void XmlTable_RoundTripsColumns()
    {
        var sql = "SELECT * FROM XMLTABLE('//ROWS/ROW' PASSING data COLUMNS (id int PATH '@id', n FOR ORDINALITY))";
        var output = SqlParser.Parse(sql)!.ToString();
        Assert.Contains("XMLTABLE", output);
        Assert.Contains("PASSING", output);
        Assert.Contains("FOR ORDINALITY", output);
    }

    [Fact]
    public void CreateIndex_UsingMethodAndColumns_RoundTrips()
    {
        var sql = "CREATE INDEX idx1 ON t USING gist (col1, col2 DESC) WHERE active";
        var output = SqlParser.Parse(sql)!.ToString();
        Assert.Contains("USING gist", output);
        Assert.Contains("col1, col2 DESC", output);
        Assert.Contains("WHERE active", output);
    }

    [Fact]
    public void XmlTable_WithNamespaces_RoundTrips()
    {
        var sql = "SELECT * FROM XMLTABLE(XMLNAMESPACES('http://x' AS x, DEFAULT 'http://d'), '//x:ROW' PASSING data COLUMNS (id int))";
        var output = SqlParser.Parse(sql)!.ToString();
        Assert.Contains("XMLNAMESPACES", output);
        Assert.Contains("DEFAULT", output);
        Assert.Contains("PASSING", output);
    }
}
