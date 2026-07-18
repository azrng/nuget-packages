using Azrng.JSqlParser.Parser;
using PlainSelect = Azrng.JSqlParser.Statement.Select.PlainSelect;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// ORDER BY WITH ROLLUP（MySQL）+ MySQL 索引提示 FOR JOIN/ORDER BY/GROUP BY 测试（批次9）。
/// </summary>
public class HintAndRollupClauseTest
{
    #region ORDER BY WITH ROLLUP

    [Fact]
    public void OrderBy_WithRollup_ShouldRoundTrip()
    {
        // MySQL 5.7 ORDER BY a WITH ROLLUP（对齐上游 mysqlWithRollup）
        var sql = "SELECT a FROM t ORDER BY a WITH ROLLUP";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        var orderBy = Assert.Single(stmt.OrderByElements!);
        Assert.True(orderBy.MysqlWithRollup);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void OrderBy_NoRollup_FlagFalse()
    {
        var stmt = (PlainSelect)SqlParser.Parse("SELECT a FROM t ORDER BY a")!;
        var orderBy = Assert.Single(stmt.OrderByElements!);
        Assert.False(orderBy.MysqlWithRollup);
    }

    [Fact]
    public void OrderBy_DescWithRollup_ShouldRoundTrip()
    {
        var sql = "SELECT a FROM t ORDER BY a DESC WITH ROLLUP";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        var orderBy = Assert.Single(stmt.OrderByElements!);
        Assert.True(orderBy.MysqlWithRollup);
        Assert.False(orderBy.Asc);
        Assert.Equal(sql, stmt.ToString());
    }

    #endregion

    #region MySQL INDEX FOR

    [Fact]
    public void MySqlIndexHint_ForJoin_ShouldRoundTrip()
    {
        // USE INDEX FOR JOIN (idx)：对齐上游 MySQLIndexHint.forClause
        // 注：MySQLIndexHint.ToString 用 ","（无空格）拼接索引名，round-trip 不严格保形
        var sql = "SELECT * FROM t USE INDEX FOR JOIN (idx1, idx2)";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        var hint = ((Azrng.JSqlParser.Schema.Table)stmt.FromItem!).MySqlIndexHint;
        Assert.NotNull(hint);
        Assert.Equal("FOR JOIN", hint!.ForClause);
        Assert.Equal("USE", hint.Action);
        Assert.Equal("INDEX", hint.IndexQualifier);
        Assert.Equal(2, hint.IndexNames.Count);
        Assert.Contains("USE INDEX FOR JOIN (idx1,idx2)", stmt.ToString()!);
    }

    [Fact]
    public void MySqlIndexHint_ForOrderBy_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM t FORCE INDEX FOR ORDER BY (pk)";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        var hint = ((Azrng.JSqlParser.Schema.Table)stmt.FromItem!).MySqlIndexHint;
        Assert.NotNull(hint);
        Assert.Equal("FOR ORDER BY", hint!.ForClause);
        Assert.Equal("FORCE", hint.Action);
    }

    [Fact]
    public void MySqlIndexHint_ForGroupBy_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM t IGNORE KEY FOR GROUP BY (idx)";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        var hint = ((Azrng.JSqlParser.Schema.Table)stmt.FromItem!).MySqlIndexHint;
        Assert.NotNull(hint);
        Assert.Equal("FOR GROUP BY", hint!.ForClause);
        Assert.Equal("IGNORE", hint.Action);
        Assert.Equal("KEY", hint.IndexQualifier);
    }

    [Fact]
    public void MySqlIndexHint_NoFor_ForClauseNull()
    {
        // 无 FOR 子句：ForClause 为 null，旧行为不变
        var stmt = (PlainSelect)SqlParser.Parse("SELECT * FROM t USE INDEX (idx)")!;
        var hint = ((Azrng.JSqlParser.Schema.Table)stmt.FromItem!).MySqlIndexHint;
        Assert.Null(hint!.ForClause);
    }

    #endregion
}
