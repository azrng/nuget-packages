using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.CreateIndex;
using Azrng.JSqlParser.Statement.Insert;
using Azrng.JSqlParser.Statement.Merge;
using Azrng.JSqlParser.Statement.Select;
using Azrng.JSqlParser.Util;
using PlainSelectType = Azrng.JSqlParser.Statement.Select.PlainSelect;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// T148 批次：同步上游 JSqlParser 5.4 正式版第一档缺口（7 项）round-trip。
/// 覆盖 #2473 GROUPS 非保留字、#2462 CREATE INDEX INCLUDE、#2605 SET IDENTITY_INSERT、
/// #2604 SET 布尔开关、#2421/#2480 MERGE NOT MATCHED BY TARGET/SOURCE、
/// #2569 MERGE RETURNING 与 INSERT OVERRIDING、#2566 递归 CTE CYCLE。
/// </summary>
public class Upstream54SyncRoundTripTest
{
    private static void AssertRoundTrip(string sql)
    {
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    #region #2473 GROUPS 非保留字

    [Theory]
    [InlineData("SELECT groups FROM t")]
    [InlineData("SELECT groups + 1 AS groups FROM t")]
    [InlineData("SELECT * FROM t WHERE groups > 1")]
    public void Groups_AsIdentifier_Parses(string sql) => AssertRoundTrip(sql);

    [Fact]
    public void Groups_WindowFrame_StillWorks()
    {
        var sql = "SELECT SUM(x) OVER (ORDER BY y GROUPS BETWEEN 1 PRECEDING AND CURRENT ROW) FROM t";
        AssertRoundTrip(sql);
    }

    #endregion

    #region #2462 CREATE INDEX INCLUDE

    [Fact]
    public void CreateIndex_IncludeColumns_RoundTrips()
    {
        var sql = "CREATE INDEX ix_t ON t (col) INCLUDE (c2, c3)";
        var stmt = Assert.IsType<CreateIndex>(SqlParser.Parse(sql));
        Assert.Equal(new[] { "c2", "c3" }, stmt.IncludedColumns);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void CreateIndex_IncludeWithWhere_RoundTrips()
    {
        var sql = "CREATE INDEX ix_t ON t (a) INCLUDE (b) WHERE a > 1";
        var stmt = Assert.IsType<CreateIndex>(SqlParser.Parse(sql));
        Assert.Equal(new[] { "b" }, stmt.IncludedColumns);
        Assert.Equal(sql, stmt.ToString());
    }

    #endregion

    #region #2605 SET IDENTITY_INSERT / #2604 SET 布尔开关

    [Theory]
    [InlineData("SET IDENTITY_INSERT t ON", true)]
    [InlineData("SET IDENTITY_INSERT t OFF", false)]
    public void SetIdentityInsert_RoundTrips(string sql, bool on)
    {
        var stmt = Assert.IsType<Azrng.JSqlParser.Statement.SetStatement>(SqlParser.Parse(sql));
        Assert.NotNull(stmt.IdentityInsertTable);
        Assert.Equal("t", stmt.IdentityInsertTable!.Name);
        Assert.Equal(on, stmt.SwitchValue);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void SetIdentityInsert_WithSchemaName_RoundTrips()
    {
        var sql = "SET IDENTITY_INSERT dbo.t ON";
        var stmt = Assert.IsType<Azrng.JSqlParser.Statement.SetStatement>(SqlParser.Parse(sql));
        Assert.Equal("dbo.t", stmt.IdentityInsertTable!.ToString());
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void SetIdentityInsert_TableFoundByTablesNamesFinder()
    {
        var stmt = SqlParser.Parse("SET IDENTITY_INSERT dbo.t ON");
        var tables = stmt!.GetTableNames();
        Assert.Contains("t", tables);
    }

    [Theory]
    [InlineData("SET NOCOUNT ON", true)]
    [InlineData("SET NOCOUNT OFF", false)]
    [InlineData("SET ANSI_NULLS OFF", false)]
    public void SetBooleanSwitch_RoundTrips(string sql, bool on)
    {
        var stmt = Assert.IsType<Azrng.JSqlParser.Statement.SetStatement>(SqlParser.Parse(sql));
        Assert.Null(stmt.IdentityInsertTable);
        Assert.Null(stmt.Value);
        Assert.Equal(on, stmt.SwitchValue);
        Assert.Equal(sql, stmt.ToString());
    }

    [Theory]
    [InlineData("SET a = 1")]
    [InlineData("SET @name = 'abc'")]
    public void SetAssignment_Regression_StillWorks(string sql) => AssertRoundTrip(sql);

    #endregion

    #region #2421/#2480 MERGE NOT MATCHED BY TARGET/SOURCE

    [Fact]
    public void Merge_NotMatchedByTargetAndSource_RoundTrips()
    {
        // USING 源子查询别名沿用移植版既有渲染（不带 AS）
        var sql = "MERGE INTO target_table AS tt USING (SELECT key, field FROM source_table) st " +
                  "ON tt.key = st.key " +
                  "WHEN NOT MATCHED BY TARGET THEN INSERT (key, field) VALUES (st.key, st.field) " +
                  "WHEN NOT MATCHED BY SOURCE THEN DELETE";
        var stmt = Assert.IsType<Merge>(SqlParser.Parse(sql));
        Assert.Equal(2, stmt.Operations.Count);
        Assert.Equal(MergeSide.Target, stmt.Operations[0].Side);
        Assert.Equal(MergeSide.Source, stmt.Operations[1].Side);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void Merge_NotMatchedBySourceUpdate_RoundTrips()
    {
        var sql = "MERGE INTO t USING s ON t.id = s.id " +
                  "WHEN NOT MATCHED BY SOURCE THEN UPDATE SET t.flag = 0";
        var stmt = Assert.IsType<Merge>(SqlParser.Parse(sql));
        var update = Assert.IsType<MergeUpdate>(stmt.Operations[0]);
        Assert.Equal(MergeSide.Source, update.Side);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void Merge_PlainNotMatchedInsert_Regression_StillWorks()
    {
        var sql = "MERGE INTO t USING s ON t.id = s.id " +
                  "WHEN NOT MATCHED THEN INSERT (id) VALUES (s.id)";
        var stmt = Assert.IsType<Merge>(SqlParser.Parse(sql));
        Assert.Equal(MergeSide.None, stmt.Operations[0].Side);
        Assert.Equal(sql, stmt.ToString());
    }

    [Theory]
    [InlineData("MERGE INTO t USING s ON t.id = s.id WHEN NOT MATCHED BY SOURCE THEN INSERT (id) VALUES (1)")]
    [InlineData("MERGE INTO t USING s ON t.id = s.id WHEN NOT MATCHED BY TARGET THEN DELETE")]
    [InlineData("MERGE INTO t USING s ON t.id = s.id WHEN NOT MATCHED BY OTHER THEN DELETE")]
    public void Merge_IllegalPairing_Throws(string sql) =>
        Assert.ThrowsAny<JSqlParserException>(() => SqlParser.Parse(sql));

    #endregion

    #region #2569 MERGE RETURNING

    [Fact]
    public void Merge_Returning_RoundTrips()
    {
        var sql = "MERGE INTO t USING s ON t.id = s.id " +
                  "WHEN MATCHED THEN UPDATE SET t.v = s.v RETURNING t.id";
        var stmt = Assert.IsType<Merge>(SqlParser.Parse(sql));
        Assert.NotNull(stmt.Returning);
        Assert.Equal(sql, stmt.ToString());
    }

    #endregion

    #region #2569 INSERT OVERRIDING USER/SYSTEM VALUE

    [Theory]
    [InlineData("INSERT INTO t (a, b) OVERRIDING USER VALUE VALUES (1, 2)", "USER")]
    [InlineData("INSERT INTO t (a) OVERRIDING SYSTEM VALUE VALUES (1)", "SYSTEM")]
    public void Insert_OverridingValue_RoundTrips(string sql, string mode)
    {
        var stmt = Assert.IsType<Insert>(SqlParser.Parse(sql));
        Assert.Equal(mode, stmt.Overriding);
        Assert.Equal(sql, stmt.ToString());
    }

    #endregion

    #region #2566 递归 CTE CYCLE

    [Fact]
    public void RecursiveCte_Cycle_Minimal_RoundTrips()
    {
        var sql = "WITH RECURSIVE t (n) AS (SELECT 1 AS n UNION ALL SELECT n + 1 FROM t WHERE n < 5) " +
                  "CYCLE n SET is_cycle USING path SELECT * FROM t";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        var withItem = Assert.IsType<WithItem>(stmt.WithItemsList!.Single());
        var cycle = withItem.CycleClause;
        Assert.NotNull(cycle);
        Assert.Equal(new[] { "n" }, cycle!.CycleColumns);
        Assert.Equal("is_cycle", cycle.MarkColumnName);
        Assert.Equal("path", cycle.PathColumnName);
        Assert.Null(cycle.MarkValue);
        Assert.Null(cycle.MarkDefault);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void RecursiveCte_Cycle_WithMarkValues_RoundTrips()
    {
        var sql = "WITH RECURSIVE t (n) AS (SELECT 1 AS n UNION ALL SELECT n + 1 FROM t WHERE n < 5) " +
                  "CYCLE n, m SET is_cycle TO 'Y' DEFAULT 'N' USING path SELECT * FROM t";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        var withItem = Assert.IsType<WithItem>(stmt.WithItemsList!.Single());
        var cycle = withItem.CycleClause!;
        Assert.Equal(new[] { "n", "m" }, cycle.CycleColumns);
        Assert.NotNull(cycle.MarkValue);
        Assert.NotNull(cycle.MarkDefault);
        Assert.Equal(sql, stmt.ToString());
    }

    #endregion
}
