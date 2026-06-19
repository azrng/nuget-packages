using Azrng.NMaxCompute.Adapter;
using Azrng.NMaxCompute.Accounts;
using Azrng.NMaxCompute.Core;
using Azrng.NMaxCompute.Rest;
using Azrng.NMaxCompute.Models;
using Microsoft.Extensions.Logging;

namespace Azrng.NMaxCompute.Provider;

/// <summary>
/// IQueryExecutor 的直连实现：直接调用阿里云 MaxCompute REST API，无需 Python 中转
/// </summary>
public sealed class DirectOdpsQueryExecutor : IQueryExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DirectOdpsQueryExecutor>? _logger;
    private readonly TimeSpan _executeTimeout;

    public DirectOdpsQueryExecutor(IHttpClientFactory httpClientFactory, ILogger<DirectOdpsQueryExecutor>? logger = null, TimeSpan? executeTimeout = null)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger;
        _executeTimeout = executeTimeout ?? TimeSpan.FromMinutes(10);
    }

    /// <summary>
    /// 执行查询：提交 SQL → 阻塞等待 → 拉取结果（Result API 路径，S0 MVP）
    /// </summary>
    public async Task<QueryResult> ExecuteQueryAsync(MaxComputeConfig config, string sql, CancellationToken cancellationToken = default)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("SQL is required", nameof(sql));

        var odps = BuildOdps(config);
        var hints = config.Hints is null ? null : new SqlHints(config.Hints);

        var result = await odps.ExecuteSqlAsync(sql, hints, _executeTimeout, cancellationToken).ConfigureAwait(false);

        if (result.IsEmpty)
        {
            return new QueryResult();
        }

        var parsed = CsvResultParser.Parse(result.RawContent, config.MaxRows);
        _logger?.LogDebug("Query executed: {RowCount} rows returned (InstanceId={InstanceId})", parsed.RowCount, result.InstanceId);
        return parsed;
    }

    /// <summary>
    /// 测试连接：执行 SELECT 1
    /// </summary>
    public async Task<bool> TestConnectionAsync(MaxComputeConfig config, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ExecuteQueryAsync(config, "SELECT 1", cancellationToken).ConfigureAwait(false);
            return result.RowCount > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "MaxCompute connection test failed");
            return false;
        }
    }

    private Odps BuildOdps(MaxComputeConfig config)
    {
        var account = new CloudAccount(
            config.AccessId,
            config.SecretAccessKey,
            config.Region,
            config.UseV4Signature);

        var httpClient = _httpClientFactory.CreateClient();
        var restClient = new OdpsRestClient(httpClient, account, config.Endpoint, _logger);
        return new Odps(restClient, config.Project);
    }
}
