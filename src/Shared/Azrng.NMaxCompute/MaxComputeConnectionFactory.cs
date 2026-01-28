using Azrng.NMaxCompute.Adapter;
using Azrng.NMaxCompute.Models;
using Microsoft.Extensions.Logging;

namespace Azrng.NMaxCompute;

/// <summary>
/// MaxCompute 连接工厂
/// </summary>
public static class MaxComputeConnectionFactory
{
    /// <summary>
    /// 创建连接
    /// </summary>
    /// <param name="queryExecutor">查询执行器</param>
    /// <param name="config">配置</param>
    /// <param name="logger">日志记录器</param>
    /// <returns>MaxCompute 连接</returns>
    public static MaxComputeConnection CreateConnection(
        IQueryExecutor queryExecutor,
        MaxComputeConfig config,
        ILogger? logger = null)
    {
        if (queryExecutor == null)
            throw new ArgumentNullException(nameof(queryExecutor));

        if (config == null)
            throw new ArgumentNullException(nameof(config));

        if (!config.IsValid())
            throw new ArgumentException("Invalid configuration. Required: Url, AccessId, SecretKey, JdbcUrl", nameof(config));

        return new MaxComputeConnection(config, queryExecutor, logger);
    }

    /// <summary>
    /// 创建连接
    /// </summary>
    /// <param name="queryExecutor">查询执行器</param>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="logger">日志记录器</param>
    /// <returns>MaxCompute 连接</returns>
    public static MaxComputeConnection CreateConnection(
        IQueryExecutor queryExecutor,
        string connectionString,
        ILogger? logger = null)
    {
        if (queryExecutor == null)
            throw new ArgumentNullException(nameof(queryExecutor));

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        return new MaxComputeConnection(connectionString, queryExecutor, logger);
    }

    /// <summary>
    /// 创建连接
    /// </summary>
    /// <param name="queryExecutor">查询执行器</param>
    /// <param name="url">接口地址</param>
    /// <param name="accessId">Access ID</param>
    /// <param name="secretKey">Secret Key</param>
    /// <param name="jdbcUrl">JDBC URL</param>
    /// <param name="project">项目名称</param>
    /// <param name="maxRows">最大返回行数</param>
    /// <param name="logger">日志记录器</param>
    /// <returns>MaxCompute 连接</returns>
    public static MaxComputeConnection CreateConnection(
        IQueryExecutor queryExecutor,
        string url,
        string accessId,
        string secretKey,
        string jdbcUrl,
        string? project = null,
        int maxRows = 1000,
        ILogger? logger = null)
    {
        if (queryExecutor == null)
            throw new ArgumentNullException(nameof(queryExecutor));

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentNullException(nameof(url));

        if (string.IsNullOrWhiteSpace(accessId))
            throw new ArgumentNullException(nameof(accessId));

        if (string.IsNullOrWhiteSpace(secretKey))
            throw new ArgumentNullException(nameof(secretKey));

        if (string.IsNullOrWhiteSpace(jdbcUrl))
            throw new ArgumentNullException(nameof(jdbcUrl));

        var config = new MaxComputeConfig
        {
            ServerUrl = url,
            AccessId = accessId,
            SecretKey = secretKey,
            JdbcUrl = jdbcUrl,
            Project = project,
            MaxRows = maxRows
        };

        return new MaxComputeConnection(config, queryExecutor, logger);
    }
}
