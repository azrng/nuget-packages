using Azrng.JSqlParser.Parser;
using PlainSelect = Azrng.JSqlParser.Statement.Select.PlainSelect;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// PostgreSQL/Informix/ClickHouse 方言 SELECT 子句测试（批次8）：
/// - SELECT INTO target_table / INTO TEMP
/// - SELECT DISTINCT ON (cols)
/// - LIMIT n BY expr / LIMIT offset, n BY expr
/// </summary>
public class DialectSelectClauseTest
{
    #region SELECT INTO

    [Fact]
    public void SelectInto_TargetTable_ShouldRoundTrip()
    {
        // PostgreSQL SELECT * INTO new_table FROM old_table
        var sql = "SELECT * INTO archive FROM users";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.NotNull(stmt.IntoTables);
        Assert.Single(stmt.IntoTables!);
        Assert.Equal("archive", stmt.IntoTables![0].Name);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void SelectInto_Temp_ShouldSetIntoTempTable()
    {
        // Informix SELECT * INTO TEMP tmp FROM t
        var sql = "SELECT * INTO TEMP tmp FROM users";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.Null(stmt.IntoTables);
        Assert.NotNull(stmt.IntoTempTable);
        Assert.Equal("tmp", stmt.IntoTempTable!.Name);
        Assert.Contains("INTO TEMP tmp", stmt.ToString()!);
    }

    [Fact]
    public void Select_NoInto_IntoTablesNull()
    {
        var stmt = (PlainSelect)SqlParser.Parse("SELECT * FROM users")!;
        Assert.Null(stmt.IntoTables);
        Assert.Null(stmt.IntoTempTable);
    }

    #endregion

    #region DISTINCT ON

    [Fact]
    public void DistinctOn_ShouldCollectOnSelectItems()
    {
        // PostgreSQL SELECT DISTINCT ON (a) a, b FROM t
        // 对齐上游 PlainSelectCC.jjt:4994-4995
        var sql = "SELECT DISTINCT ON (a) a, b FROM t";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.NotNull(stmt.Distinct);
        Assert.NotNull(stmt.Distinct!.OnSelectItems);
        Assert.Single(stmt.Distinct.OnSelectItems!);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void DistinctOn_MultipleColumns_ShouldRoundTrip()
    {
        var sql = "SELECT DISTINCT ON (a, b) a, b, c FROM t";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.Equal(2, stmt.Distinct!.OnSelectItems!.Count);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void Distinct_NoOn_ShouldHaveNullOnSelectItems()
    {
        var stmt = (PlainSelect)SqlParser.Parse("SELECT DISTINCT a FROM t")!;
        Assert.NotNull(stmt.Distinct);
        Assert.Null(stmt.Distinct!.OnSelectItems);
    }

    [Fact]
    public void Distinct_OnRoundTrip_PreservesOn()
    {
        var sql = "SELECT DISTINCT ON (dept) name, salary FROM emp";
        var stmt = SqlParser.Parse(sql)!;
        Assert.Contains("DISTINCT ON (dept)", stmt.ToString()!);
    }

    #endregion

    #region LIMIT BY (ClickHouse)

    [Fact]
    public void LimitBy_SingleExpression_ShouldRoundTrip()
    {
        // ClickHouse LIMIT 10 BY user_id
        var sql = "SELECT * FROM t LIMIT 10 BY user_id";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.NotNull(stmt.Limit);
        Assert.NotNull(stmt.Limit!.ByExpressions);
        Assert.Single(stmt.Limit.ByExpressions!);
        Assert.Contains("BY user_id", stmt.ToString()!);
    }

    [Fact]
    public void LimitBy_MultipleExpressions_ShouldRoundTrip()
    {
        var sql = "SELECT * FROM t LIMIT 10 BY a, b";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.Equal(2, stmt.Limit!.ByExpressions!.Count);
        Assert.Contains("BY a, b", stmt.ToString()!);
    }

    [Fact]
    public void Limit_OffsetCommaCount_ByExpression_ShouldRoundTrip()
    {
        // ClickHouse LIMIT 5, 10 BY a（offset=5, rowCount=10, BY a）
        // ToString 走等价重写 LIMIT count OFFSET offset（与 MySQL offset,count 语法一致行为）
        var sql = "SELECT * FROM t LIMIT 5, 10 BY a";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        Assert.NotNull(stmt.Limit);
        Assert.NotNull(stmt.Limit!.ByExpressions);
        // offset=5, rowCount=10 正确解析；BY 子句保留
        Assert.Contains("LIMIT 10 OFFSET 5 BY a", stmt.ToString()!);
    }

    [Fact]
    public void Limit_NoBy_ByExpressionsNull()
    {
        var stmt = (PlainSelect)SqlParser.Parse("SELECT * FROM t LIMIT 10")!;
        Assert.Null(stmt.Limit!.ByExpressions);
    }

    #endregion
}
