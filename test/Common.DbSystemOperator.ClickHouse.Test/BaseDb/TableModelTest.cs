using Azrng.Core;
using Azrng.DbOperator;
using Common.DbSystemOperator.ClickHouse.Test.Helper;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Common.DbSystemOperator.ClickHouse.Test.BaseDb;

/// <summary>
/// 表结构测试
/// </summary>
public class TableStructTest
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<TableStructTest> _logger;
    private readonly IBasicDbBridge _basicDbBridge;

    public TableStructTest(IJsonSerializer jsonSerializer, ILogger<TableStructTest> logger, IBasicDbBridge basicDbBridge)
    {
        _jsonSerializer = jsonSerializer;
        _logger = logger;
        _basicDbBridge = basicDbBridge;
    }

    /// <summary>
    /// 查询指定表下列信息
    /// </summary>
    [Fact]
    public async Task QueryTableColumnStruct_Test()
    {
        var result = await _basicDbBridge.GetColumnListAsync("default", "fee_detail");
        Assert.True(result.Count >= 1);
        _logger.LogInformation(_jsonSerializer.ToJson(result));
    }
}