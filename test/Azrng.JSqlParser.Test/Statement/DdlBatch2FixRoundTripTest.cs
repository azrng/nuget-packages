using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Alter;
using Azrng.JSqlParser.Statement.CreateIndex;
using Azrng.JSqlParser.Statement.CreateTable;
using Azrng.JSqlParser.Statement.Insert;
using AlterStatement = Azrng.JSqlParser.Statement.Alter.Alter;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// T124 批次：SQL Server/Oracle 索引与 BULK 相关上游 issue 修复 round-trip。
/// 覆盖 #2033 INSERT BULK、#2039 USING INDEX TABLESPACE、#2020 CREATE INDEX WITH、
/// MySQL 前缀索引 col(n)。
/// </summary>
public class DdlBatch2FixRoundTripTest
{
    #region #2033 INSERT BULK

    [Fact]
    public void InsertBulk_Simple_RoundTrips()
    {
        var sql = "INSERT BULK tpch.dbo.order_line([ol_o_id] int,[ol_d_id] tinyint) WITH(ROWS_PER_BATCH=500000)";
        var stmt = Assert.IsType<Insert>(SqlParser.Parse(sql));
        Assert.True(stmt.Bulk);
        Assert.Equal("tpch.dbo.order_line", stmt.Table!.ToString());
        Assert.NotNull(stmt.BulkColumnDefinitions);
        Assert.Equal(2, stmt.BulkColumnDefinitions!.Count);
        Assert.Contains("ol_o_id", stmt.BulkColumnDefinitions[0], StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(stmt.BulkWithOptions);
        Assert.Contains(stmt.BulkWithOptions!, o => o.Contains("ROWS_PER_BATCH", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void InsertBulk_WithCollate_RoundTrips()
    {
        var sql = "INSERT BULK dbo.t([c] char(24) collate Chinese_PRC_CI_AS) WITH(ROWS_PER_BATCH=1)";
        var stmt = Assert.IsType<Insert>(SqlParser.Parse(sql));
        Assert.True(stmt.Bulk);
        Assert.Contains("collate", stmt.BulkColumnDefinitions![0], StringComparison.OrdinalIgnoreCase);
        SqlParser.Parse(stmt.ToString()!);
    }

    #endregion

    #region #2039 USING INDEX TABLESPACE

    [Fact]
    public void AlterAddConstraint_UsingIndexTablespace_RoundTrips()
    {
        var sql = "ALTER TABLE bfmcs.your_table ADD CONSTRAINT your_table_pk PRIMARY KEY (ID) USING INDEX TABLESPACE your_tablespace";
        var stmt = Assert.IsType<AlterStatement>(SqlParser.Parse(sql));
        var expr = stmt.AlterExpressions![0];
        Assert.True(expr.HasUsingIndex);
        Assert.Null(expr.UsingIndex);
        Assert.Equal("your_tablespace", expr.UsingIndexTablespace);
        Assert.Contains("USING INDEX TABLESPACE your_tablespace", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void AlterAddConstraint_UsingIndexNameAndTablespace_RoundTrips()
    {
        var sql = "ALTER TABLE t ADD CONSTRAINT pk PRIMARY KEY (ID) USING INDEX idx1 TABLESPACE ts1";
        var stmt = Assert.IsType<AlterStatement>(SqlParser.Parse(sql));
        var expr = stmt.AlterExpressions![0];
        Assert.Equal("idx1", expr.UsingIndex);
        Assert.Equal("ts1", expr.UsingIndexTablespace);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void CreateTable_UsingIndexTablespace_RoundTrips()
    {
        var sql = "CREATE TABLE t (id INT, CONSTRAINT pk PRIMARY KEY (id) USING INDEX TABLESPACE ts1)";
        var stmt = Assert.IsType<CreateTable>(SqlParser.Parse(sql));
        Assert.NotNull(stmt.Constraints);
        var c = stmt.Constraints![0];
        Assert.True(c.HasUsingIndex);
        Assert.Equal("ts1", c.UsingIndexTablespace);
        Assert.Contains("USING INDEX TABLESPACE ts1", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    #endregion

    #region #2020 CREATE INDEX WITH

    [Fact]
    public void CreateIndex_WithOptions_RoundTrips()
    {
        var sql = "CREATE INDEX IX ON t (c) WITH (PAD_INDEX = OFF, FILLFACTOR = 80)";
        var stmt = Assert.IsType<CreateIndex>(SqlParser.Parse(sql));
        Assert.NotNull(stmt.WithOptions);
        Assert.Equal(2, stmt.WithOptions!.Count);
        Assert.Contains("PAD_INDEX", stmt.WithOptions[0], StringComparison.OrdinalIgnoreCase);
        Assert.Contains("FILLFACTOR", stmt.WithOptions[1], StringComparison.OrdinalIgnoreCase);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void CreateIndex_WithOnline_RoundTrips()
    {
        var sql = "CREATE INDEX IX ON t (c) WITH (ONLINE = ON)";
        var stmt = Assert.IsType<CreateIndex>(SqlParser.Parse(sql));
        Assert.Single(stmt.WithOptions!);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    #endregion

    #region MySQL 前缀索引 col(n)

    [Fact]
    public void CreateTable_PrefixIndex_RoundTrips()
    {
        var sql = "CREATE TABLE t (id INT, KEY idx (id(10)))";
        var stmt = Assert.IsType<CreateTable>(SqlParser.Parse(sql));
        Assert.NotNull(stmt.Constraints);
        var c = stmt.Constraints![0];
        Assert.Contains("id(10)", string.Join(",", c.IndexColumnParams ?? c.Columns));
        Assert.Contains("id(10)", stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void CreateTable_MultiPrefixIndex_RoundTrips()
    {
        var sql = "CREATE TABLE t (id INT, name VARCHAR(100), KEY idx (id(10), name(5)))";
        var stmt = Assert.IsType<CreateTable>(SqlParser.Parse(sql));
        var c = stmt.Constraints![0];
        var cols = string.Join(",", c.IndexColumnParams ?? c.Columns);
        Assert.Contains("id(10)", cols);
        Assert.Contains("name(5)", cols);
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void InsertBulk_NoColumns_WithOnly_RoundTrips()
    {
        var sql = "INSERT BULK dbo.t WITH(CHECK_CONSTRAINTS=OFF)";
        var stmt = Assert.IsType<Insert>(SqlParser.Parse(sql));
        Assert.True(stmt.Bulk);
        Assert.Null(stmt.BulkColumnDefinitions);
        Assert.NotNull(stmt.BulkWithOptions);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void CreateIndex_WithMultipleOptions_AndUnique()
    {
        var sql = "CREATE UNIQUE INDEX IX ON dbo.t (a, b) WITH (PAD_INDEX = OFF, ONLINE = ON, FILLFACTOR = 90)";
        var stmt = Assert.IsType<CreateIndex>(SqlParser.Parse(sql));
        Assert.True(stmt.Unique);
        Assert.Equal(3, stmt.WithOptions!.Count);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    #endregion
}
