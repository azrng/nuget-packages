using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;
using PlainSelectType = Azrng.JSqlParser.Statement.Select.PlainSelect;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// 非 PostgreSQL 专项修复的 round-trip 验证（补强探针的"能解析"为"解析且语义结构正确"）。
/// 对照 issue 分类清单已修复项（T114 批次），断言 ToString 保留关键语法结构 + AST 关键字段。
/// </summary>
public class NonPgFixRoundTripTest
{
    // ===== Commit 1：通用 + SQL Server =====

    #region #1169 GROUP BY ASC/DESC

    [Fact]
    public void GroupByDesc_RoundTrips()
    {
        var sql = "SELECT a FROM b GROUP BY c DESC";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("GROUP BY c DESC", output);
        // round-trip：再次解析不抛异常
        SqlParser.Parse(output);
    }

    [Fact]
    public void GroupByMultipleDirections_RoundTrips()
    {
        var sql = "SELECT a FROM b GROUP BY c ASC, d DESC";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("c ASC", output);
        Assert.Contains("d DESC", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void GroupByDesc_StructuredField()
    {
        var sql = "SELECT a FROM b GROUP BY c DESC";
        var stmt = SqlParser.Parse(sql) as PlainSelectType;
        Assert.NotNull(stmt);
        Assert.NotNull(stmt!.GroupBy);
        Assert.NotNull(stmt.GroupBy!.GroupByColumnReferences);
        Assert.Single(stmt.GroupBy.GroupByColumnReferences!);
        Assert.True(stmt.GroupBy.GroupByColumnReferences![0].IsDesc);
    }

    [Fact]
    public void GroupByNoDirection_KeepsLegacyField()
    {
        // 无 ASC/DESC 时沿用旧 GroupByExpressions 字段，保持向后兼容
        var stmt = SqlParser.Parse("SELECT a FROM b GROUP BY c, d") as PlainSelectType;
        Assert.NotNull(stmt);
        Assert.NotNull(stmt!.GroupBy);
        Assert.Null(stmt.GroupBy!.GroupByColumnReferences);
        Assert.Equal(2, stmt.GroupBy.GroupByExpressions.Count);
    }

    #endregion

    #region #911 SQL Server @table 表变量

    [Fact]
    public void TableVariableAtPrefix_RoundTrips()
    {
        var sql = "SELECT columnName FROM @table";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("@table", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void TableVariable_AtName_Structured()
    {
        var stmt = SqlParser.Parse("SELECT * FROM @myTableVar") as PlainSelectType;
        Assert.NotNull(stmt);
        var table = stmt!.FromItem as Table;
        Assert.NotNull(table);
        Assert.Equal("@myTableVar", table!.Name);
    }

    [Fact]
    public void ParameterAtName_StillWorks()
    {
        // 确保 @x 在 WHERE 中仍作为命名参数，不被 table 规则抢匹配
        var stmt = SqlParser.Parse("SELECT * FROM t WHERE id = @p") as PlainSelectType;
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("@p", output);
    }

    #endregion

    #region #1589 PRIMARY KEY NONCLUSTERED

    [Fact]
    public void PrimaryKeyNonclustered_TableLevel_RoundTrips()
    {
        var sql = "CREATE TABLE actor (actor_id INT NOT NULL, first_name VARCHAR(45) NOT NULL, PRIMARY KEY NONCLUSTERED (actor_id))";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("NONCLUSTERED", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void PrimaryKeyClustered_ColumnLevel_RoundTrips()
    {
        var sql = "CREATE TABLE t (id INT PRIMARY KEY CLUSTERED)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("CLUSTERED", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void UniqueNonclustered_RoundTrips()
    {
        var sql = "CREATE TABLE t (id INT, name VARCHAR(50), UNIQUE NONCLUSTERED (name))";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("NONCLUSTERED", output);
        SqlParser.Parse(output);
    }

    #endregion

    #region #161 OPTION hint

    [Fact]
    public void OptionMaxRecursion_RoundTrips()
    {
        var sql = "SELECT CustomerID FROM cte OPTION (MAXRECURSION 2)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("OPTION", output);
        Assert.Contains("MAXRECURSION 2", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void OptionMultipleHints_RoundTrips()
    {
        var sql = "SELECT * FROM t OPTION (HASH JOIN, MAXDOP 4)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("HASH JOIN", output);
        Assert.Contains("MAXDOP 4", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void OptionHint_StructuredField()
    {
        var stmt = SqlParser.Parse("SELECT * FROM t OPTION (MAXRECURSION 2)") as PlainSelectType;
        Assert.NotNull(stmt);
        Assert.NotNull(stmt!.OptionHints);
        Assert.Contains("MAXRECURSION", stmt.OptionHints!);
    }

    #endregion

    // ===== Commit 2：MySQL 专项 =====

    #region #854 SELECT INTO @var

    [Fact]
    public void IntoUserVariable_RoundTrips()
    {
        var sql = "SELECT COUNT(*) INTO @countTotal FROM employee";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("INTO @countTotal", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void IntoMultipleUserVariables_RoundTrips()
    {
        var sql = "SELECT a, b INTO @x, @y FROM t";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("INTO @x, @y", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void IntoUserVariable_StructuredField()
    {
        var stmt = SqlParser.Parse("SELECT a INTO @x FROM t") as PlainSelectType;
        Assert.NotNull(stmt);
        Assert.NotNull(stmt!.IntoVariables);
        Assert.Single(stmt.IntoVariables!);
        Assert.Equal("@x", stmt.IntoVariables![0]);
    }

    #endregion

    #region #1314 INSERT SET AS alias

    [Fact]
    public void InsertSetBasic_RoundTrips()
    {
        var sql = "INSERT INTO t SET a = 1, b = 2";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("SET a = 1, b = 2", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void InsertSetWithAsAlias_RoundTrips()
    {
        var sql = "INSERT INTO t1 SET a = 1, b = 2, c = 3 AS new(m, n, p) ON DUPLICATE KEY UPDATE c = m + n";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("SET a = 1, b = 2, c = 3", output);
        Assert.Contains("AS new", output);
        Assert.Contains("m, n, p", output);
        Assert.Contains("ON DUPLICATE KEY UPDATE", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void InsertSetAsAlias_StructuredFields()
    {
        var stmt = SqlParser.Parse("INSERT INTO t SET a = 1 AS new(m, n)") as Azrng.JSqlParser.Statement.Insert.Insert;
        Assert.NotNull(stmt);
        Assert.True(stmt!.UseSet);
        Assert.Equal("new", stmt.AliasName);
        Assert.NotNull(stmt.ColumnAlias);
        Assert.Equal(new[] { "m", "n" }, stmt.ColumnAlias!);
    }

    #endregion

    #region #2298 CAST CHARACTER SET

    [Fact]
    public void CastCharCharacterSet_RoundTrips()
    {
        var sql = "SELECT CAST('abc' AS CHAR CHARACTER SET utf8mb4)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("CHAR CHARACTER SET utf8mb4", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void CastCharCharacterSetCollate_RoundTrips()
    {
        var sql = "SELECT CAST('abc' AS CHAR CHARACTER SET utf8mb4 COLLATE utf8mb4_bin)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("CHARACTER SET utf8mb4", output);
        Assert.Contains("COLLATE utf8mb4_bin", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void CastCharacterSet_StructuredField()
    {
        // 通过 ToString 间接验证结构化字段（CastExpression 是内部 AST）
        var output = SqlParser.Parse("SELECT CAST('x' AS CHAR CHARACTER SET utf8mb4)")!.ToString();
        Assert.Contains("CHARACTER SET utf8mb4", output);
    }

    #endregion

    #region #2427 + #2006 _utf8mb4 introducer

    [Fact]
    public void IntroducerNoSpace_RoundTrips()
    {
        // _utf8mb4'text' 紧贴形式（#2427）
        var sql = "SELECT _utf8mb4'some text' COLLATE utf8mb4_unicode_ci AS custom_string";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("_utf8mb4", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void IntroducerWithSpace_RoundTrips()
    {
        // _utf8mb4 'text' 带空格形式（#2006）
        var sql = "SELECT short_name FROM player_table WHERE `short_name` LIKE _utf8mb4 '%Felipe%'";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("_utf8mb4", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void Latin1Introducer_RoundTrips()
    {
        // 其他 MySQL introducer：_latin1
        var sql = "SELECT _latin1'some text'";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("_latin1", output);
        SqlParser.Parse(output);
    }

    #endregion

    #region #2428 PROCEDURE ANALYSE

    [Fact]
    public void ProcedureAnalyse_RoundTrips()
    {
        var sql = "SELECT col1, col2 FROM heavy_table PROCEDURE ANALYSE(10, 256)";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("PROCEDURE", output);
        Assert.Contains("ANALYSE", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void ProcedureAnalyseNoArgs_RoundTrips()
    {
        var sql = "SELECT * FROM t PROCEDURE ANALYSE()";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        var output = stmt!.ToString();
        Assert.Contains("PROCEDURE ANALYSE", output);
        SqlParser.Parse(output);
    }

    [Fact]
    public void ProcedureAnalyse_StructuredField()
    {
        var stmt = SqlParser.Parse("SELECT * FROM t PROCEDURE ANALYSE(10, 256)") as PlainSelectType;
        Assert.NotNull(stmt);
        Assert.NotNull(stmt!.MySqlProcedure);
        Assert.Contains("ANALYSE", stmt.MySqlProcedure!);
    }

    #endregion
}
