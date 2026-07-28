using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Alter;
using Azrng.JSqlParser.Statement.CreateTable;
using Azrng.JSqlParser.Statement.Select;
using AlterStatement = Azrng.JSqlParser.Statement.Alter.Alter;
using PlainSelectType = Azrng.JSqlParser.Statement.Select.PlainSelect;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// T125 批次：分区 / interval / ON PRIMARY / LATERAL VIEW 探针转绿。
/// 覆盖 #1668、#673、#2020（ON PRIMARY 剩余）、#2433（核实已支持）。
/// </summary>
public class DdlBatch3FixRoundTripTest
{
    #region #1668 MySQL PARTITION BY

    [Fact]
    public void CreateTable_PartitionByRangeWithDefs_RoundTrips()
    {
        var sql = "CREATE TABLE t1 (year_col INT) PARTITION BY RANGE (year_col) (PARTITION p0 VALUES LESS THAN (1991), PARTITION p1 VALUES LESS THAN (1995), PARTITION p2 VALUES LESS THAN (1999))";
        var stmt = Assert.IsType<CreateTable>(SqlParser.Parse(sql));
        Assert.NotNull(stmt.TableOptions);
        Assert.Contains(stmt.TableOptions!, o => o.Contains("PARTITION BY RANGE", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(stmt.TableOptions!, o => o.Contains("PARTITION p0", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("PARTITION BY RANGE", stmt.ToString());
        Assert.Contains("PARTITION p2", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void CreateTable_PartitionByHashPartitions_StillWorks()
    {
        var sql = "CREATE TABLE t (col VARCHAR(32)) PARTITION BY HASH (col) PARTITIONS 4";
        var stmt = Assert.IsType<CreateTable>(SqlParser.Parse(sql));
        Assert.Contains("PARTITION BY HASH", stmt.ToString());
        Assert.Contains("PARTITIONS 4", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void AlterTable_AddMultiplePartitions_RoundTrips()
    {
        var sql = "ALTER TABLE t1 ADD PARTITION (PARTITION p3 VALUES LESS THAN (2002), PARTITION p4 VALUES LESS THAN (2010))";
        var stmt = Assert.IsType<AlterStatement>(SqlParser.Parse(sql));
        var expr = stmt.AlterExpressions![0];
        Assert.Equal(AlterOperation.AddPartition, expr.Operation);
        Assert.NotNull(expr.PartitionDefinitions);
        Assert.Equal(2, expr.PartitionDefinitions!.Count);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void AlterTable_AddSinglePartition_StillWorks()
    {
        var sql = "ALTER TABLE t1 ADD PARTITION (PARTITION p3 VALUES LESS THAN (2002))";
        var stmt = Assert.IsType<AlterStatement>(SqlParser.Parse(sql));
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    #endregion

    #region #673 DAY TO SECOND

    [Fact]
    public void Interval_DayToSecond_RoundTrips()
    {
        var sql = "SELECT INTERVAL '1' DAY TO SECOND FROM dual";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        var item = Assert.IsType<SelectItem>(stmt.SelectItems![0]);
        var interval = Assert.IsType<IntervalExpression>(item.Expression);
        Assert.Equal("DAY TO SECOND", interval.IntervalType);
        Assert.Contains("DAY TO SECOND", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void Extract_DayToSecond_RoundTrips()
    {
        var sql = "SELECT EXTRACT(DAY FROM (SYSDATE - to_date('20180101', 'YYYYMMDD')) DAY TO SECOND) FROM dual";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        var item = Assert.IsType<SelectItem>(stmt.SelectItems![0]);
        var extract = Assert.IsType<ExtractExpression>(item.Expression);
        Assert.Equal("DAY", extract.Name);
        Assert.Equal("DAY TO SECOND", extract.IntervalQualifier);
        Assert.Contains("DAY TO SECOND", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void Cast_IntervalDayToSecond_StillWorks()
    {
        var sql = "SELECT CAST(x AS INTERVAL DAY TO SECOND) FROM t";
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Contains("INTERVAL", stmt!.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    #endregion

    #region #2020 ON PRIMARY

    [Fact]
    public void CreateTable_WithOptionsOnPrimary_RoundTrips()
    {
        var sql = "CREATE TABLE dbo.t (id int NOT NULL, name varchar(50) NOT NULL) WITH (PAD_INDEX = OFF, FILLFACTOR = 80) ON PRIMARY";
        var stmt = Assert.IsType<CreateTable>(SqlParser.Parse(sql));
        Assert.NotNull(stmt.TableOptions);
        Assert.Contains(stmt.TableOptions!, o => o.Contains("WITH", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(stmt.TableOptions!, o => o.Equals("ON PRIMARY", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("ON PRIMARY", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void CreateTable_OnQuotedPrimary_RoundTrips()
    {
        var sql = "CREATE TABLE dbo.t (id int PRIMARY KEY) ON [PRIMARY]";
        var stmt = Assert.IsType<CreateTable>(SqlParser.Parse(sql));
        Assert.Contains("ON [PRIMARY]", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    #endregion

    #region #2433 LATERAL VIEW multi aliases（核实已支持）

    [Fact]
    public void LateralView_ThreeOrMoreAliases_NotMisparsedAsJoins()
    {
        var sql = "SELECT a FROM t LATERAL VIEW json_tuple(j, 'a', 'b', 'c') x AS c1, c2, c3, c4";
        var stmt = Assert.IsType<PlainSelectType>(SqlParser.Parse(sql));
        Assert.IsType<Table>(stmt.FromItem);
        // 仅一个 LATERAL VIEW join，不应把 c3/c4 误解析为 cross-join 表
        Assert.NotNull(stmt.Joins);
        Assert.Single(stmt.Joins!);
        Assert.IsType<LateralView>(stmt.Joins[0].RightItem);
        var lv = (LateralView)stmt.Joins[0].RightItem!;
        Assert.Contains("c1", lv.GeneratorFunction);
        Assert.Contains("c4", lv.GeneratorFunction);
        Assert.DoesNotContain("JOIN c3", stmt.ToString(), StringComparison.OrdinalIgnoreCase);
        SqlParser.Parse(stmt.ToString()!);
    }

    #endregion
}
