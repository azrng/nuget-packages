using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Create.Schema;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// CREATE SCHEMA 语句测试。
/// 对应上游 commit ac46c434（catalog 支持）。
/// </summary>
public class CreateSchemaTest
{
    [Fact]
    public void CreateSchema_Simple_ShouldRoundTrip()
    {
        var sql = "CREATE SCHEMA myschema";
        var stmt = SqlParser.Parse(sql)!;
        var create = Assert.IsType<CreateSchema>(stmt);
        Assert.Equal("myschema", create.SchemaName);
        Assert.Null(create.CatalogName);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void CreateSchema_WithCatalog_ShouldRoundTrip()
    {
        var sql = "CREATE SCHEMA unnamed.myschema";
        var stmt = SqlParser.Parse(sql)!;
        var create = Assert.IsType<CreateSchema>(stmt);
        Assert.Equal("unnamed", create.CatalogName);
        Assert.Equal("myschema", create.SchemaName);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void CreateSchema_WithAuthorization_ShouldRoundTrip()
    {
        var sql = "CREATE SCHEMA myschema AUTHORIZATION myauth";
        var stmt = SqlParser.Parse(sql)!;
        var create = Assert.IsType<CreateSchema>(stmt);
        Assert.Equal("myschema", create.SchemaName);
        Assert.Equal("myauth", create.Authorization);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void CreateSchema_IfNotExists_ShouldRoundTrip()
    {
        var sql = "CREATE SCHEMA IF NOT EXISTS myschema";
        var stmt = SqlParser.Parse(sql)!;
        var create = Assert.IsType<CreateSchema>(stmt);
        Assert.True(create.IfNotExists);
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void CreateSchema_CatalogWithIfNotExists_ShouldRoundTrip()
    {
        var sql = "CREATE SCHEMA IF NOT EXISTS cat.myschema";
        var stmt = SqlParser.Parse(sql)!;
        var create = Assert.IsType<CreateSchema>(stmt);
        Assert.True(create.IfNotExists);
        Assert.Equal("cat", create.CatalogName);
        Assert.Equal("myschema", create.SchemaName);
        Assert.Equal(sql, stmt.ToString());
    }
}
