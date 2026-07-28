using Azrng.JSqlParser;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement;
using Azrng.JSqlParser.Statement.Alter;
using Azrng.JSqlParser.Statement.Create.Database;
using Azrng.JSqlParser.Statement.Drop;
using AlterStatement = Azrng.JSqlParser.Statement.Alter.Alter;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// T123 批次：常见 DDL 上游 issue 修复的 round-trip 验证。
/// 覆盖 #2070 CREATE DATABASE、#2065 DROP 多表、#1875 ADD IF NOT EXISTS、
/// #2112 DROP/MODIFY IF EXISTS、#599 MODIFY NULL/NOT NULL。
/// </summary>
public class DdlUpstreamFixRoundTripTest
{
    #region #2070 CREATE DATABASE

    [Fact]
    public void CreateDatabase_Simple_RoundTrips()
    {
        var sql = "CREATE DATABASE USERS";
        var stmt = Assert.IsType<CreateDatabase>(SqlParser.Parse(sql));
        Assert.Equal("USERS", stmt.DatabaseName);
        Assert.False(stmt.IfNotExists);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void CreateDatabase_IfNotExists_RoundTrips()
    {
        var sql = "CREATE DATABASE IF NOT EXISTS mydb";
        var stmt = Assert.IsType<CreateDatabase>(SqlParser.Parse(sql));
        Assert.True(stmt.IfNotExists);
        Assert.Equal("mydb", stmt.DatabaseName);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    #endregion

    #region #2065 DROP multi-table IF EXISTS

    [Fact]
    public void DropTable_MultipleIfExists_RoundTrips()
    {
        var sql = "DROP TABLE IF EXISTS t1, t2, t3";
        var stmt = Assert.IsType<Drop>(SqlParser.Parse(sql));
        Assert.True(stmt.IfExists);
        Assert.Equal("TABLE", stmt.Type);
        Assert.NotNull(stmt.NameList);
        Assert.Equal(3, stmt.NameList!.Count);
        Assert.Equal("t1", stmt.Name!.Name);
        Assert.Equal("t2", stmt.NameList[1].Name);
        Assert.Equal("t3", stmt.NameList[2].Name);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void DropTable_MultipleWithCascade_RoundTrips()
    {
        var sql = "DROP TABLE IF EXISTS t1, t2 CASCADE";
        var stmt = Assert.IsType<Drop>(SqlParser.Parse(sql));
        Assert.Equal("CASCADE", stmt.DropBehavior);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    #endregion

    #region #1875 ADD COLUMN IF NOT EXISTS

    [Fact]
    public void AlterAddColumn_IfNotExists_RoundTrips()
    {
        var sql = "ALTER TABLE t ADD COLUMN IF NOT EXISTS c INT";
        var stmt = Assert.IsType<AlterStatement>(SqlParser.Parse(sql));
        Assert.Single(stmt.AlterExpressions!);
        var expr = stmt.AlterExpressions![0];
        Assert.Equal(AlterOperation.Add, expr.Operation);
        Assert.True(expr.UseColumnKeyword);
        Assert.True(expr.IfNotExists);
        Assert.NotNull(expr.ColDataTypeList);
        Assert.Equal("c", expr.ColDataTypeList![0].ColumnName);
        Assert.Contains("INT", expr.ColDataTypeList[0].DataType);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void AlterAdd_IfNotExistsWithoutColumnKeyword_RoundTrips()
    {
        var sql = "ALTER TABLE t ADD IF NOT EXISTS c INT";
        var stmt = Assert.IsType<AlterStatement>(SqlParser.Parse(sql));
        var expr = stmt.AlterExpressions![0];
        Assert.True(expr.IfNotExists);
        Assert.False(expr.UseColumnKeyword);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    #endregion

    #region #2112 DROP/MODIFY IF EXISTS

    [Fact]
    public void AlterDropColumn_IfExists_RoundTrips()
    {
        var sql = "ALTER TABLE t DROP COLUMN IF EXISTS c";
        var stmt = Assert.IsType<AlterStatement>(SqlParser.Parse(sql));
        var expr = stmt.AlterExpressions![0];
        Assert.Equal(AlterOperation.Drop, expr.Operation);
        Assert.True(expr.UseColumnKeyword);
        Assert.True(expr.IfExists);
        Assert.Equal("c", expr.ColumnName);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void AlterModify_IfExists_RoundTrips()
    {
        var sql = "ALTER TABLE t MODIFY COLUMN IF EXISTS c INT";
        var stmt = Assert.IsType<AlterStatement>(SqlParser.Parse(sql));
        var expr = stmt.AlterExpressions![0];
        Assert.Equal(AlterOperation.Modify, expr.Operation);
        Assert.True(expr.UseColumnKeyword);
        Assert.True(expr.IfExists);
        Assert.NotNull(expr.ColDataTypeList);
        Assert.Equal("c", expr.ColDataTypeList![0].ColumnName);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    #endregion

    #region #599 MODIFY NULL / NOT NULL

    [Fact]
    public void AlterModify_NotNullOnly_RoundTrips()
    {
        var sql = "ALTER TABLE t MODIFY c NOT NULL";
        var stmt = Assert.IsType<AlterStatement>(SqlParser.Parse(sql));
        var expr = stmt.AlterExpressions![0];
        Assert.Equal(AlterOperation.Modify, expr.Operation);
        Assert.Equal("c", expr.ColumnName);
        Assert.Equal("NOT NULL", expr.OptionalSpecifier);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void AlterModify_NullOnly_RoundTrips()
    {
        var sql = "ALTER TABLE t MODIFY c NULL";
        var stmt = Assert.IsType<AlterStatement>(SqlParser.Parse(sql));
        var expr = stmt.AlterExpressions![0];
        Assert.Equal("c", expr.ColumnName);
        Assert.Equal("NULL", expr.OptionalSpecifier);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void AlterModify_IfExistsNotNull_RoundTrips()
    {
        var sql = "ALTER TABLE t MODIFY IF EXISTS c NOT NULL";
        var stmt = Assert.IsType<AlterStatement>(SqlParser.Parse(sql));
        var expr = stmt.AlterExpressions![0];
        Assert.True(expr.IfExists);
        Assert.Equal("c", expr.ColumnName);
        Assert.Equal("NOT NULL", expr.OptionalSpecifier);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void AlterModify_TypeWithNotNull_PreservesSpecs()
    {
        var sql = "ALTER TABLE t MODIFY c VARCHAR(10) NOT NULL";
        var stmt = Assert.IsType<AlterStatement>(SqlParser.Parse(sql));
        var expr = stmt.AlterExpressions![0];
        Assert.NotNull(expr.ColDataTypeList);
        Assert.Contains("VARCHAR", expr.ColDataTypeList![0].DataType);
        Assert.Contains("NOT NULL", expr.ColDataTypeList[0].DataType);
        Assert.Equal(sql, stmt.ToString());
        SqlParser.Parse(stmt.ToString()!);
    }

    [Fact]
    public void AlterModify_IfExistsNotNull_Combined()
    {
        // #2112 + #599 组合
        var sql = "ALTER TABLE t MODIFY COLUMN IF EXISTS c NOT NULL";
        var stmt = Assert.IsType<AlterStatement>(SqlParser.Parse(sql));
        var expr = stmt.AlterExpressions![0];
        Assert.True(expr.IfExists);
        Assert.True(expr.UseColumnKeyword);
        Assert.Equal("NOT NULL", expr.OptionalSpecifier);
        Assert.Equal(sql, stmt.ToString());
    }

    #endregion

    #region visitor / 表名提取

    [Fact]
    public void CreateDatabase_AcceptsStatementVisitor()
    {
        var stmt = Assert.IsType<CreateDatabase>(SqlParser.Parse("CREATE DATABASE IF NOT EXISTS appdb"));
        var visitor = new StatementVisitorAdapter<object?>();
        // 不抛即接线完整
        stmt.Accept<object?, object?>(visitor, null);
        Assert.Equal("appdb", stmt.DatabaseName);
    }

    [Fact]
    public void DropMultiple_GetTableNames()
    {
        var stmt = SqlParser.Parse("DROP TABLE IF EXISTS a, b, c")!;
        var names = stmt.GetTableNames();
        Assert.Contains("a", names);
        Assert.Contains("b", names);
        Assert.Contains("c", names);
    }

    #endregion
}
