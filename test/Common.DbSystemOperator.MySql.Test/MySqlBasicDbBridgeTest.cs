using Azrng.Core;
using Azrng.DbOperator;
using Xunit.Abstractions;

namespace Common.DbSystemOperator.MySql.Test;

public class MySqlBasicDbBridgeTest
{
    private readonly IBasicDbBridge _basicDbBridge;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _schemaName;
    private readonly string _tableName;
    private readonly IJsonSerializer _jsonSerializer;

    public MySqlBasicDbBridgeTest(IBasicDbBridge basicDbBridge, ITestOutputHelper testOutputHelper, IJsonSerializer jsonSerializer)
    {
        _basicDbBridge = basicDbBridge;
        _testOutputHelper = testOutputHelper;
        _jsonSerializer = jsonSerializer;

        _schemaName = "zyp-test";
        _tableName = "test";
    }

    [Fact]
    public void GetDbTypeTest()
    {
        var result = _basicDbBridge.DatabaseType;
        Assert.Equal(DatabaseType.MySql, result);
    }

    /// <summary>
    /// 获取schema列表
    /// </summary>
    [Fact]
    public async Task GetSchemaTest()
    {
        var result = await _basicDbBridge.GetSchemaNameListAsync();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        _testOutputHelper.WriteLine(_jsonSerializer.ToJson(result));
    }

    /// <summary>
    /// 获取某个schema下表列表
    /// </summary>
    [Fact]
    public async Task GetSchemaTableTest()
    {
        var result = await _basicDbBridge.GetTableInfoListAsync(_schemaName);
        Assert.True(result.Count > 0);
    }

    /// <summary>
    /// 获取某个schema下某一个表的所有列
    /// </summary>
    [Fact]
    public async Task GetSchemaTableColumnTest()
    {
        var result = await _basicDbBridge.GetColumnListAsync(_schemaName, _tableName);
        Assert.True(result.Count > 0);
        _testOutputHelper.WriteLine(_jsonSerializer.ToJson(result));
    }

    /// <summary>
    /// 获取某个schema下的所有列
    /// </summary>
    [Fact]
    public async Task GetSchemaColumnTest()
    {
        var result = await _basicDbBridge.GetColumnListAsync(_schemaName);
        Assert.True(result.Count > 0);
        _testOutputHelper.WriteLine(_jsonSerializer.ToJson(result));
    }

    /// <summary>
    /// 获取某个schema下某个表的所有主键
    /// </summary>
    [Fact]
    public async Task GetSchemaTablePrimaryTest()
    {
        var result = await _basicDbBridge.GetPrimaryListAsync(_schemaName, _tableName);
        Assert.True(result.Count > 0);
        _testOutputHelper.WriteLine(_jsonSerializer.ToJson(result));
    }

    /// <summary>
    /// 获取某个schema下的所有主键
    /// </summary>
    [Fact]
    public async Task GetSchemaPrimaryTest()
    {
        var result = await _basicDbBridge.GetPrimaryListAsync(_schemaName);
        Assert.True(result.Count > 0);
        _testOutputHelper.WriteLine(_jsonSerializer.ToJson(result));
    }
}