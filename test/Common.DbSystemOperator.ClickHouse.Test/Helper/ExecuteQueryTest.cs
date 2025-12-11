using Azrng.Core;
using Azrng.DbOperator;
using Azrng.DbOperator.Dto;
using Common.DbSystemOperator.ClickHouse.Test.Dto;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Common.DbSystemOperator.ClickHouse.Test.Helper;

public class ExecuteQueryTest
{
    private readonly IDbHelper _dbHelper;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<ExecuteQueryTest> _logger;

    public ExecuteQueryTest(IDbHelper dbHelper, IJsonSerializer jsonSerializer, ILogger<ExecuteQueryTest> logger)
    {
        _dbHelper = dbHelper;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    /// <summary>
    /// 单个列查询
    /// </summary>
    [Fact]
    public async Task SingleColumn_Test()
    {
        var sql = @"select coalesce(sum(total_cost), 0) as ind_10842797
from fee_detail
where fee_detail.bill_time >= '2022-12-31 00:00:00'
order by ind_10842797 desc;";
        var result = await _dbHelper.QueryArrayAsync(sql);
        Assert.Equal(2, result.Length);
        _logger.LogInformation(_jsonSerializer.ToJson(result));
    }

    /// <summary>
    /// 查询字符串列返回
    /// </summary>
    [Fact]
    public async Task QueryString_Test()
    {
        var str = "select patient_name from fee_detail;";
        var result = await _dbHelper.QueryAsync<string>(str);
        Assert.True(result.Count >= 1);
        _logger.LogInformation(_jsonSerializer.ToJson(result));
    }

    /// <summary>
    /// 查询表列信息
    /// </summary>
    [Fact]
    public async Task QueryTableColumnInfo_Test()
    {
        var sql = "DESCRIBE TABLE fee_detail";
        var result = await _dbHelper.QueryAsync<ColumnPropertyDto>(sql);
        Assert.True(result.Count >= 1);
        _logger.LogInformation(_jsonSerializer.ToJson(result));
    }
 }