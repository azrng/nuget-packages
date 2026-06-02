using Azrng.NMaxCompute.Adapter;
using Azrng.NMaxCompute.Models;
using Azrng.NMaxCompute.Test.Executor;
using Azrng.NMaxCompute.Test.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Azrng.NMaxCompute.Test;

/// <summary>
/// 集成测试示例 - 展示如何使用 HttpQueryExecutor 进行真实查询
/// 注意：这些测试需要真实的 MaxCompute 服务才能运行
/// </summary>
public class IntegrationTest
{
    /// <summary>
    /// 使用 HttpQueryExecutor 创建连接的示例
    /// </summary>
    [Fact]
    public async Task HttpQueryExecutor_Connect_Test()
    {
        // 配置 DI 容器
        var service = new ServiceCollection();
        service.AddHttpClient();
        service.AddLogging(builder => builder.AddConsole());
        service.AddScoped<IQueryExecutor, HttpQueryExecutor>();
        var provider = service.BuildServiceProvider();

        var queryExecutor = provider.GetRequiredService<IQueryExecutor>();
        var logger = provider.GetRequiredService<ILogger<IntegrationTest>>();

        // 配置 MaxCompute 连接信息
        var config = new MaxComputeConfig
        {
            ServerUrl = "http://your-server-url",  // 替换为实际的服务器地址
            AccessId = "your-access-id",            // 替换为实际的 Access ID
            SecretKey = "your-secret-key",          // 替换为实际的 Secret Key
            JdbcUrl = "jdbc:odps:https://service.cn-shanghai.maxcompute.aliyun.com/api?project=your-project&tunnelEndpoint=https://dt.cn-shanghai.maxcompute.aliyun.com",
            Project = "your-project",
            MaxRows = 10000
        };

        // 创建连接
        await using var connection = MaxComputeConnectionFactory.CreateConnection(queryExecutor, config, logger);
        await connection.OpenAsync();

        logger.LogInformation("连接已打开");
    }

    /// <summary>
    /// 使用 Dapper 进行查询的示例
    /// </summary>
    [Fact]
    public async Task Dapper_Query_Example()
    {
        // 配置 DI 容器
        var service = new ServiceCollection();
        service.AddHttpClient();
        service.AddLogging(builder => builder.AddConsole());
        service.AddScoped<IQueryExecutor, HttpQueryExecutor>();
        var provider = service.BuildServiceProvider();

        var queryExecutor = provider.GetRequiredService<IQueryExecutor>();
        var logger = provider.GetRequiredService<ILogger<IntegrationTest>>();

        // 配置 MaxCompute 连接信息
        var config = new MaxComputeConfig
        {
            ServerUrl = "http://your-server-url",
            AccessId = "your-access-id",
            SecretKey = "your-secret-key",
            JdbcUrl = "jdbc:odps:https://service.cn-shanghai.maxcompute.aliyun.com/api?project=your-project&tunnelEndpoint=https://dt.cn-shanghai.maxcompute.aliyun.com",
            Project = "your-project",
            MaxRows = 1000
        };

        // 创建连接并查询
        await using var connection = MaxComputeConnectionFactory.CreateConnection(queryExecutor, config, logger);
        await connection.OpenAsync();

        // 使用 Dapper 查询强类型对象
        var sql = "SELECT * FROM your_table LIMIT 5";
        var users = await connection.QueryAsync<UserInfoDto>(sql);

        logger.LogInformation("查询到 {Count} 条记录", users.Count());

        foreach (var user in users)
        {
            logger.LogInformation("用户: {Name}, 年龄: {Age}", user.Name, user.Age);
        }
    }
}
