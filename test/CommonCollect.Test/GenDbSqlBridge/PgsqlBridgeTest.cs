using Azrng.Core.NewtonsoftJson.Utils;
using CommonCollect.GenDbSqlBridge;
using CommonCollect.GenDbSqlBridge.Model;
using Xunit.Abstractions;

namespace CommonCollect.Test.GenDbSqlBridge;

public class PgsqlBridgeTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PgsqlBridgeTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// 添加schema
    /// </summary>
    [Fact]
    public void AddSchemaSqlTest()
    {
        var sql = File.ReadAllText("GenDbSqlBridge/Script/addSchema.json");
        var obj = JsonHelper.ToObject<ModelStructDiffBo>(sql);

        var pgsqlBridge = new PgsqlBridge(StructDbTypeEnum.MYSQL);

        var genTableSql = pgsqlBridge.AddSchemaSql(obj.SchemaDiff.Adds.ToArray());
        Assert.NotEmpty(genTableSql.ToString());
        _testOutputHelper.WriteLine(genTableSql.ToString());
    }

    /// <summary>
    /// 移除schema
    /// </summary>
    [Fact]
    public void RemoveSchemaSqlTest()
    {
        var sql = File.ReadAllText("GenDbSqlBridge/Script/removeSchema.json");
        var obj = JsonHelper.ToObject<ModelStructDiffBo>(sql);

        var pgsqlBridge = new PgsqlBridge(StructDbTypeEnum.MYSQL);

        var genTableSql = pgsqlBridge.RemoveSchemaSql(obj.SchemaDiff.Removes.ToArray());
        Assert.NotEmpty(genTableSql.ToString());
        _testOutputHelper.WriteLine(genTableSql.ToString());
    }

    /// <summary>
    /// 添加表sql
    /// </summary>
    [Fact]
    public void AddTableSqlTest()
    {
        var sql = File.ReadAllText("GenDbSqlBridge/Script/addTable.json");
        var obj = JsonHelper.ToObject<ModelStructDiffBo>(sql);

        var pgsqlBridge = new PgsqlBridge(StructDbTypeEnum.MYSQL);

        var genTableSql = pgsqlBridge.AddTableSql(obj.TableStructDiff.Adds);
        Assert.NotEmpty(genTableSql.ToString());
        _testOutputHelper.WriteLine(genTableSql.ToString());
    }

    /// <summary>
    /// 移除表sql
    /// </summary>
    [Fact]
    public void RemoveTableSqlTest()
    {
        var sql = File.ReadAllText("GenDbSqlBridge/Script/removeTable.json");
        var obj = JsonHelper.ToObject<ModelStructDiffBo>(sql);

        var pgsqlBridge = new PgsqlBridge(StructDbTypeEnum.MYSQL);

        var genTableSql = pgsqlBridge.RemoveTableSql(obj.TableStructDiff.Removes);
        Assert.NotEmpty(genTableSql.ToString());
        _testOutputHelper.WriteLine(genTableSql.ToString());
    }

    /// <summary>
    /// 移除列sql
    /// </summary>
    [Fact]
    public void RemoveColumnSqlTest()
    {
        var sql = File.ReadAllText("GenDbSqlBridge/Script/removeColumn.json");
        var obj = JsonHelper.ToObject<ModelStructDiffBo>(sql);

        var pgsqlBridge = new PgsqlBridge(StructDbTypeEnum.MYSQL);

        var genTableSql = pgsqlBridge.RemoveColumnSql(obj.ColumnStructInfoDiff.Removes);
        Assert.NotEmpty(genTableSql.ToString());
        _testOutputHelper.WriteLine(genTableSql.ToString());
    }
}