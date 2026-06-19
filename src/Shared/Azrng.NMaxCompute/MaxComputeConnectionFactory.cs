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
    /// 创建连接（基于配置对象）
    /// </summary>
    public static MaxComputeConnection CreateConnection(
        IQueryExecutor queryExecutor,
        MaxComputeConfig config,
        ILogger? logger = null)
    {
        if (queryExecutor == null) throw new ArgumentNullException(nameof(queryExecutor));
        if (config == null) throw new ArgumentNullException(nameof(config));
        if (!config.IsValid())
            throw new ArgumentException("Invalid configuration. Required: Endpoint, AccessId, SecretAccessKey, Project", nameof(config));

        return new MaxComputeConnection(config, queryExecutor, logger);
    }

    /// <summary>
    /// 创建连接（基于连接字符串）
    /// </summary>
    public static MaxComputeConnection CreateConnection(
        IQueryExecutor queryExecutor,
        string connectionString,
        ILogger? logger = null)
    {
        if (queryExecutor == null) throw new ArgumentNullException(nameof(queryExecutor));
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));

        return new MaxComputeConnection(connectionString, queryExecutor, logger);
    }
}
