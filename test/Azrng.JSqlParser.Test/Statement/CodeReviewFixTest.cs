using System.Globalization;
using System.Threading;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Util;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// 库代码审查修复的回归测试（H1/H2/H4 等）。
/// 固化关键修复行为，防止回归。
/// </summary>
public class CodeReviewFixTest
{
    // ===== H1: Merge 三连失 =====

    [Fact]
    public void H1_Merge_UsingSourceTable_RoundTrip()
    {
        var sql = "MERGE INTO t USING src ON t.id = src.id WHEN MATCHED THEN UPDATE SET t.a = src.a";
        Assert.Equal(sql, SqlParser.Parse(sql)!.ToString());
    }

    [Fact]
    public void H1_Merge_WhenAndCondition_RoundTrip()
    {
        var sql = "MERGE INTO t USING src ON t.id = src.id WHEN MATCHED AND x > 0 THEN DELETE";
        Assert.Equal(sql, SqlParser.Parse(sql)!.ToString());
    }

    [Fact]
    public void H1_Merge_InsertValues_RoundTrip()
    {
        var sql = "MERGE INTO t USING src ON t.id = src.id WHEN NOT MATCHED THEN INSERT (a, b) VALUES (1, 2)";
        Assert.Equal(sql, SqlParser.Parse(sql)!.ToString());
    }

    [Fact]
    public void H1_Merge_SourceTable_Extracted()
    {
        var tables = new TablesNamesFinder().GetTables(
            SqlParser.Parse("MERGE INTO t USING src ON t.id = src.id WHEN MATCHED THEN DELETE")!);
        Assert.Contains("t", tables);
        Assert.Contains("src", tables);
    }

    // ===== H2: 区域性数值解析 =====

    [Fact]
    public void H2_DoubleLiteral_InvariantCulture()
    {
        var prev = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            // de-DE 小数分隔符为逗号，但 SQL 字面量必须用点，不能被区域污染
            Assert.Equal("SELECT 1.5", SqlParser.Parse("SELECT 1.5")!.ToString());
            Assert.Equal("SELECT 1.5, 2.5", SqlParser.Parse("SELECT 1.5, 2.5")!.ToString());
        }
        finally { Thread.CurrentThread.CurrentCulture = prev; }
    }

    [Fact]
    public void H2_DoubleValue_ToString_Invariant()
    {
        var prev = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            Assert.Equal("1.5", new Azrng.JSqlParser.Expression.DoubleValue(1.5).ToString());
        }
        finally { Thread.CurrentThread.CurrentCulture = prev; }
    }

    // ===== H4: TablesNamesFinder 表名提取 =====

    [Fact]
    public void H4_TableStatement_ExtractsTableName()
    {
        var tables = new TablesNamesFinder().GetTables(SqlParser.Parse("TABLE users")!);
        Assert.Contains("users", tables);
    }

    [Fact]
    public void H4_UpdateJoin_ExtractsBothTables()
    {
        var tables = new TablesNamesFinder().GetTables(
            SqlParser.Parse("UPDATE a JOIN b ON a.id = b.a_id SET a.x = b.y")!);
        Assert.Contains("a", tables);
        Assert.Contains("b", tables);
    }

    [Fact]
    public void H4_OrderBySubquery_Extracted()
    {
        var tables = new TablesNamesFinder().GetTables(
            SqlParser.Parse("SELECT * FROM users ORDER BY (SELECT name FROM cfg)")!);
        Assert.Contains("users", tables);
        Assert.Contains("cfg", tables);
    }

    [Fact]
    public void H4_GroupBySubquery_Extracted()
    {
        var tables = new TablesNamesFinder().GetTables(
            SqlParser.Parse("SELECT * FROM users GROUP BY id HAVING COUNT(*) > (SELECT COUNT(*) FROM orders)")!);
        Assert.Contains("orders", tables);
    }

    // ===== H3: 多语句显式报错 =====

    [Fact]
    public void H3_MultipleStatements_Throws()
    {
        Assert.Throws<JSqlParserException>(() =>
            SqlParser.Parse("SELECT 1; DROP TABLE x"));
    }

    // ===== M4: ParseNullable 不吞非语法异常 =====

    [Fact]
    public void M4_ParseNullable_ReturnsNull_OnSyntaxError()
    {
        Assert.Null(SqlParser.ParseNullable("SELECT FROM"));
    }

    // ===== L1: CTE 括号 round-trip =====

    [Fact]
    public void L1_CteBrackets_RoundTrip()
    {
        var sql = "WITH t AS (SELECT 1) SELECT * FROM t";
        Assert.Equal(sql, SqlParser.Parse(sql)!.ToString());
    }

    // ===== L2: OFFSET ROWS round-trip =====

    [Fact]
    public void L2_OffsetRows_RoundTrip()
    {
        Assert.Equal("SELECT * FROM t OFFSET 1 ROWS",
            SqlParser.Parse("SELECT * FROM t OFFSET 1 ROWS")!.ToString());
    }

    [Fact]
    public void L2_OffsetNoRow_RoundTrip()
    {
        Assert.Equal("SELECT * FROM t OFFSET 1",
            SqlParser.Parse("SELECT * FROM t OFFSET 1")!.ToString());
    }
}
