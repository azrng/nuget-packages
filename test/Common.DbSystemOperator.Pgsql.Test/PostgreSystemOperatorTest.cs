using Azrng.Core;
using Azrng.DbOperator;
using Xunit.Abstractions;

namespace Common.DbSystemOperator.Pgsql.Test;

public class PostgreBasicDbBridgeTest
{
    private readonly IBasicDbBridge _basicDbBridge;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IJsonSerializer _jsonSerializer;

    public PostgreBasicDbBridgeTest(IBasicDbBridge basicDbBridge, ITestOutputHelper testOutputHelper, IJsonSerializer jsonSerializer)
    {
        _basicDbBridge = basicDbBridge;
        _testOutputHelper = testOutputHelper;
        _jsonSerializer = jsonSerializer;
    }

    [Fact]
    public void GetDbTypeTest()
    {
        var result = _basicDbBridge.DatabaseType;
        Assert.Equal(DatabaseType.PostgresSql, result);
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
        var result = await _basicDbBridge.GetTableInfoListAsync("sample");
        Assert.True(result.Count > 0);
    }

    /// <summary>
    /// 获取某个schema下所有列
    /// </summary>
    [Fact]
    public async Task GetSchemaColumnTest()
    {
        var result = await _basicDbBridge.GetColumnListAsync("sample");
        Assert.True(result.Count > 0);
        _testOutputHelper.WriteLine(_jsonSerializer.ToJson(result));
    }

    /// <summary>
    /// 获取某个schema下某一个表的所有列
    /// </summary>
    [Fact]
    public async Task GetSchemaTableColumnTest()
    {
        var result = await _basicDbBridge.GetColumnListAsync("sample", "data_types");
        Assert.True(result.Count > 0);
        _testOutputHelper.WriteLine(_jsonSerializer.ToJson(result));
    }

    /// <summary>
    /// 获取某个schema下Primary
    /// </summary>
    [Fact]
    public async Task GetSchemaPrimaryTest()
    {
        var result = await _basicDbBridge.GetPrimaryListAsync("sample");
        Assert.True(result.Count > 0);
        _testOutputHelper.WriteLine(_jsonSerializer.ToJson(result));
    }

    /// <summary>
    /// 获取某个schema下某一个表的Primary
    /// </summary>
    [Fact]
    public async Task GetSchemaTablePrimaryTest()
    {
        var result = await _basicDbBridge.GetPrimaryListAsync("sample", "data_types");
        Assert.True(result.Count > 0);
        _testOutputHelper.WriteLine(_jsonSerializer.ToJson(result));
    }

    /// <summary>
    /// 获取某个schema下Foreign
    /// </summary>
    [Fact]
    public async Task GetSchemaForeignTest()
    {
        var result = await _basicDbBridge.GetForeignListAsync("sample");
        Assert.True(result.Count > 0);
        _testOutputHelper.WriteLine(_jsonSerializer.ToJson(result));
    }

    /// <summary>
    /// 获取某个schema下某一个表的Foreign
    /// </summary>
    [Fact]
    public async Task GetSchemaTableForeignTest()
    {
        var result = await _basicDbBridge.GetForeignListAsync("sample", "related_data");
        Assert.True(result.Count > 0);
        _testOutputHelper.WriteLine(_jsonSerializer.ToJson(result));
    }

    /// <summary>
    /// 获取某个schema下Index
    /// </summary>
    [Fact]
    public async Task GetSchemaIndexTest()
    {
        var result = await _basicDbBridge.GetIndexListAsync("sample");
        Assert.True(result.Count > 0);
        _testOutputHelper.WriteLine(_jsonSerializer.ToJson(result));
    }

    /// <summary>
    /// 获取某个schema下某一个表的Index
    /// </summary>
    [Fact]
    public async Task GetSchemaTableIndexTest()
    {
        var result = await _basicDbBridge.GetIndexListAsync("sample", "data_types");
        Assert.True(result.Count > 0);
        _testOutputHelper.WriteLine(_jsonSerializer.ToJson(result));
    }

    /// <summary>
    /// 获取某个schema下View
    /// </summary>
    [Fact]
    public async Task GetSchemaViewTest()
    {
        var result = await _basicDbBridge.GetSchemaViewListAsync("sample");
        Assert.True(result.Count > 0);
        _testOutputHelper.WriteLine(_jsonSerializer.ToJson(result));
    }

    /// <summary>
    /// 获取某个schema下Proc
    /// </summary>
    [Fact]
    public async Task GetSchemaProcTest()
    {
        var result = await _basicDbBridge.GetSchemaProcListAsync("sample");
        Assert.True(result.Count > 0);
        _testOutputHelper.WriteLine(_jsonSerializer.ToJson(result));
    }
}