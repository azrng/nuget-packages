using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// ksqlDB 流式窗口测试（HOPPING/TUMBLING/SESSION + WITHIN + EMIT CHANGES），对齐上游 KSQLTest。
/// </summary>
public class KSQLTest
{
    [Fact]
    public void Ksql_HoppingWindow_ShouldParseStructuredFields()
    {
        var sql = "SELECT * FROM orders WINDOW HOPPING (SIZE 30 SECONDS, ADVANCE BY 10 MINUTES) GROUP BY region.id";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.NotNull(select.KsqlWindow);
        Assert.True(select.KsqlWindow!.Hopping);
        Assert.Equal(30, select.KsqlWindow.SizeDuration);
        Assert.Equal(KSQLTimeUnit.SECONDS, select.KsqlWindow.SizeTimeUnit);
        Assert.Equal(10, select.KsqlWindow.AdvanceDuration);
        Assert.Equal(KSQLTimeUnit.MINUTES, select.KsqlWindow.AdvanceTimeUnit);
    }

    [Fact]
    public void Ksql_TumblingWindow_ShouldParse()
    {
        var sql = "SELECT * FROM orders WINDOW TUMBLING (SIZE 30 SECONDS) GROUP BY region.id";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.NotNull(select.KsqlWindow);
        Assert.True(select.KsqlWindow!.Tumbling);
        Assert.Equal(30, select.KsqlWindow.SizeDuration);
        Assert.Equal(KSQLTimeUnit.SECONDS, select.KsqlWindow.SizeTimeUnit);
    }

    [Fact]
    public void Ksql_SessionWindow_ShouldParse()
    {
        var sql = "SELECT * FROM orders WINDOW SESSION (5 MINUTES) GROUP BY region.id";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.NotNull(select.KsqlWindow);
        Assert.True(select.KsqlWindow!.Session);
        Assert.Equal(5, select.KsqlWindow.SizeDuration);
        Assert.Equal(KSQLTimeUnit.MINUTES, select.KsqlWindow.SizeTimeUnit);
    }

    [Fact]
    public void Ksql_HoppingWindow_RoundTrip_ShouldPreserve()
    {
        var sql = "SELECT * FROM orders WINDOW HOPPING (SIZE 30 SECONDS, ADVANCE BY 10 MINUTES) GROUP BY region.id";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.Contains("WINDOW HOPPING (SIZE 30 SECONDS, ADVANCE BY 10 MINUTES)", select.ToString());
    }

    [Fact]
    public void Ksql_TumblingWindow_RoundTrip_ShouldPreserve()
    {
        var sql = "SELECT * FROM orders WINDOW TUMBLING (SIZE 30 SECONDS) GROUP BY region.id";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.Contains("WINDOW TUMBLING (SIZE 30 SECONDS)", select.ToString());
    }

    [Fact]
    public void Ksql_SessionWindow_RoundTrip_ShouldPreserve()
    {
        var sql = "SELECT * FROM orders WINDOW SESSION (5 MINUTES) GROUP BY region.id";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.Contains("WINDOW SESSION (5 MINUTES)", select.ToString());
    }

    [Fact]
    public void Ksql_EmitChanges_ShouldParse()
    {
        var sql = "SELECT * FROM orders GROUP BY region.id EMIT CHANGES";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.True(select.EmitChanges);
    }

    [Fact]
    public void Ksql_EmitChangesWithLimit_ShouldParse()
    {
        var sql = "SELECT * FROM orders GROUP BY region.id EMIT CHANGES LIMIT 2";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.True(select.EmitChanges);
        Assert.NotNull(select.Limit);
    }

    [Fact]
    public void Ksql_EmitChanges_RoundTrip_ShouldPreserve()
    {
        var sql = "SELECT * FROM orders GROUP BY region.id EMIT CHANGES";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.Contains("EMIT CHANGES", select.ToString()!.Trim());
    }

    [Fact]
    public void Ksql_WindowedJoin_SingleWindow_ShouldParse()
    {
        var sql = "SELECT * FROM table1 t1 INNER JOIN table2 t2 WITHIN (5 HOURS) ON t1.id = t2.id";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.Single(select.Joins!);
        var join = select.Joins![0];
        Assert.NotNull(join.JoinWindow);
        Assert.False(join.JoinWindow!.BeforeAfter);
        Assert.Equal(5, join.JoinWindow.Duration);
        Assert.Equal(KSQLTimeUnit.HOURS, join.JoinWindow.TimeUnit);
    }

    [Fact]
    public void Ksql_WindowedJoin_BeforeAfterWindow_ShouldParse()
    {
        var sql = "SELECT * FROM table1 t1 INNER JOIN table2 t2 WITHIN (1 MINUTE, 5 MINUTES) ON t1.id = t2.id";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        var join = select.Joins![0];
        Assert.NotNull(join.JoinWindow);
        Assert.True(join.JoinWindow!.BeforeAfter);
        Assert.Equal(1, join.JoinWindow.BeforeDuration);
        Assert.Equal(KSQLTimeUnit.MINUTE, join.JoinWindow.BeforeTimeUnit);
        Assert.Equal(5, join.JoinWindow.AfterDuration);
        Assert.Equal(KSQLTimeUnit.MINUTES, join.JoinWindow.AfterTimeUnit);
    }

    [Fact]
    public void Ksql_WindowedJoin_RoundTrip_ShouldPreserve()
    {
        var sql = "SELECT * FROM table1 t1 INNER JOIN table2 t2 WITHIN (5 HOURS) ON t1.id = t2.id";
        var select = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.Contains("WITHIN (5 HOURS)", select.ToString());
    }
}
