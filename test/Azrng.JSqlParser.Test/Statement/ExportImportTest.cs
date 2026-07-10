using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Export;
using Azrng.JSqlParser.Statement.Import;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-19a Exasol EXPORT/IMPORT 测试（简化透传版）。
/// </summary>
public class ExportImportTest
{
    // ── EXPORT ──

    [Fact]
    public void Export_TableIntoCsv_ShouldRoundTrip()
    {
        var sql = "EXPORT schemaName.tableName INTO LOCAL CSV FILE 'file.csv'";
        var stmt = (ExportStatement)CCJSqlParserUtil.Parse(sql)!;
        Assert.NotNull(stmt.Table);
        Assert.Equal("schemaName.tableName", stmt.Table!.ToString());
        Assert.Contains("LOCAL CSV FILE", stmt.IntoItem);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void Export_TableWithColumns_ShouldRoundTrip()
    {
        var sql = "EXPORT tableName (col1, col2) INTO LOCAL CSV FILE 'file.csv'";
        var stmt = (ExportStatement)CCJSqlParserUtil.Parse(sql)!;
        Assert.NotNull(stmt.Table);
        Assert.NotNull(stmt.Columns);
        Assert.Equal(2, stmt.Columns!.Count);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void Export_SelectIntoCsv_ShouldRoundTrip()
    {
        var sql = "EXPORT (SELECT 1) INTO LOCAL CSV FILE 'file.csv'";
        var stmt = (ExportStatement)CCJSqlParserUtil.Parse(sql)!;
        Assert.NotNull(stmt.Select);
        Assert.Null(stmt.Table);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void Export_ScriptDestination_ShouldRoundTrip()
    {
        var sql = "EXPORT tableName INTO SCRIPT scriptName AT connectionName WITH propertyName = 'value'";
        var stmt = (ExportStatement)CCJSqlParserUtil.Parse(sql)!;
        Assert.Contains("SCRIPT scriptName", stmt.IntoItem);
        Assert.Equal(sql, stmt.ToString());
    }

    // ── IMPORT ──

    [Fact]
    public void Import_IntoTableFromCsv_ShouldRoundTrip()
    {
        var sql = "IMPORT INTO tableName (col1) FROM LOCAL CSV FILE 'file.csv'";
        var stmt = (ImportStatement)CCJSqlParserUtil.Parse(sql)!;
        Assert.NotNull(stmt.Table);
        Assert.NotNull(stmt.Columns);
        Assert.Single(stmt.Columns!);
        Assert.Contains("LOCAL CSV FILE", stmt.FromItem);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void Import_FromCsv_NoInto_ShouldRoundTrip()
    {
        var sql = "IMPORT FROM LOCAL CSV FILE 'file.csv'";
        var stmt = (ImportStatement)CCJSqlParserUtil.Parse(sql)!;
        Assert.Null(stmt.Table);
        Assert.Contains("LOCAL CSV FILE", stmt.FromItem);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void Import_JdbcDriver_ShouldRoundTrip()
    {
        var sql = "IMPORT INTO tableName FROM JDBC DRIVER = 'driverName' AT connectionName STATEMENT 'select 1'";
        var stmt = (ImportStatement)CCJSqlParserUtil.Parse(sql)!;
        Assert.NotNull(stmt.Table);
        Assert.Contains("JDBC DRIVER", stmt.FromItem);
        Assert.Equal(sql, stmt.ToString());
    }
}
