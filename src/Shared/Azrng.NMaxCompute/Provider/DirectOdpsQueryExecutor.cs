using Azrng.NMaxCompute.Adapter;
using Azrng.NMaxCompute.Accounts;
using Azrng.NMaxCompute.Core;
using Azrng.NMaxCompute.Rest;
using Azrng.NMaxCompute.Models;
using Azrng.NMaxCompute.Tunnel;
using Microsoft.Extensions.Logging;

namespace Azrng.NMaxCompute.Provider;

/// <summary>
/// IQueryExecutor 的直连实现：直接调用阿里云 MaxCompute REST API，无需 Python 中转
/// </summary>
public sealed class DirectOdpsQueryExecutor : IQueryExecutor
{
    /// <summary>
    /// 命中这些错误码/关键字时，Tunnel 路径回退到 Result API。
    /// 对应 PyODPS Tunnel 在 <c>InvalidProjectTable / InvalidArgument / NoSuchProject / InstanceTypeNotSupported</c> 时的回退。
    /// </summary>
    private static readonly string[] TunnelFallbackMarkers =
    {
        "InvalidProjectTable",
        "InvalidArgument",
        "NoSuchProject",
        "InstanceTypeNotSupported",
        "NoDownload"
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DirectOdpsQueryExecutor>? _logger;
    private readonly TimeSpan _executeTimeout;
    private readonly bool _preferTunnel;

    public DirectOdpsQueryExecutor(
        IHttpClientFactory httpClientFactory,
        ILogger<DirectOdpsQueryExecutor>? logger = null,
        TimeSpan? executeTimeout = null,
        bool preferTunnel = true)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger;
        _executeTimeout = executeTimeout ?? TimeSpan.FromMinutes(10);
        _preferTunnel = preferTunnel;
    }

    /// <summary>
    /// 执行查询：提交 SQL → 等待完成 → 拉取结果。
    /// <para>默认优先走 Instance Tunnel（无行数限制、类型化），失败回退 Result API（CSV，限 10000 行）。</para>
    /// </summary>
    public async Task<QueryResult> ExecuteQueryAsync(MaxComputeConfig config, string sql, CancellationToken cancellationToken = default)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("SQL is required", nameof(sql));

        var (odps, restClient) = Build(config);
        var hints = config.Hints is null ? null : new SqlHints(config.Hints);

        // 提交并等待 Instance 完成（两条路径共用）
        var instance = await odps.RunSqlAsync(sql, hints, cancellationToken: cancellationToken).ConfigureAwait(false);
        await instance.WaitForTerminationAsync(_executeTimeout, cancellationToken).ConfigureAwait(false);

        if (_preferTunnel)
        {
            try
            {
                var tunnelClient = restClient;
                // 自定义 Tunnel 端点：单独建一个指向 TunnelEndpoint 的 client
                if (!string.IsNullOrWhiteSpace(config.TunnelEndpoint))
                {
                    var account = BuildAccount(config);
                    var tunnelHttpClient = _httpClientFactory.CreateClient();
                    tunnelClient = new OdpsRestClient(tunnelHttpClient, account, config.TunnelEndpoint!, _logger);
                }

                return await ExecuteViaTunnelAsync(tunnelClient, config, instance.Id, cancellationToken).ConfigureAwait(false);
            }
            catch (OdpsException ex) when (ShouldFallbackToResultApi(ex))
            {
                _logger?.LogWarning("Tunnel path failed (code={Code}), falling back to Result API. msg={Message}", ex.Code, ex.Message);
            }
        }

        // Result API 路径（回退或默认）
        var result = await instance.GetResultAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (result.IsEmpty)
            return new QueryResult();

        var parsed = CsvResultParser.Parse(result.RawContent, config.MaxRows);
        _logger?.LogDebug("Query executed via Result API: {RowCount} rows (InstanceId={InstanceId})", parsed.RowCount, result.InstanceId);
        return parsed;
    }

    /// <summary>
    /// Tunnel 路径：创建 session → 拉取全部行 → 物化。
    /// </summary>
    private async Task<QueryResult> ExecuteViaTunnelAsync(OdpsRestClient restClient, MaxComputeConfig config, string instanceId, CancellationToken cancellationToken)
    {
        var session = await InstanceDownloadSession.CreateAsync(
            restClient, config.Project, instanceId,
            quotaName: null, tags: null, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (session.RecordCount == 0)
        {
            var empty = new QueryResult();
            FillSchema(empty, session.Schema);
            _logger?.LogDebug("Tunnel session returned 0 rows (InstanceId={InstanceId})", instanceId);
            return empty;
        }

        var maxRows = config.MaxRows > 0 ? config.MaxRows : (int)session.RecordCount;
        var count = Math.Min(maxRows, (int)session.RecordCount);

        using var reader = await session.OpenRecordReaderAsync(0, count, cancellationToken: cancellationToken).ConfigureAwait(false);
        var queryResult = TunnelResultMaterializer.Materialize(reader, session.Schema);
        _logger?.LogDebug("Query executed via Tunnel: {RowCount} rows (InstanceId={InstanceId})", queryResult.RowCount, instanceId);
        return queryResult;
    }

    private static void FillSchema(QueryResult result, TableSchema schema)
    {
        result.Columns = new string[schema.Columns.Count];
        result.ColumnTypes = new string[schema.Columns.Count];
        for (var i = 0; i < schema.Columns.Count; i++)
        {
            result.Columns[i] = schema.Columns[i].Name;
            result.ColumnTypes[i] = schema.Columns[i].Type;
        }
    }

    private bool ShouldFallbackToResultApi(OdpsException ex)
        => ShouldFallbackToResultApi(ex.Code, ex.Message);

    /// <summary>
    /// 判断给定的错误码/消息是否应触发 Tunnel → Result API 回退。internal 便于测试。
    /// </summary>
    internal static bool ShouldFallbackToResultApi(string? code, string? message)
    {
        foreach (var marker in TunnelFallbackMarkers)
        {
            if ((code?.IndexOf(marker, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0)
                return true;
            if ((message?.IndexOf(marker, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0)
                return true;
        }
        return false;
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

    private (Odps Odps, OdpsRestClient RestClient) Build(MaxComputeConfig config)
    {
        var account = BuildAccount(config);

        var httpClient = _httpClientFactory.CreateClient();
        var restClient = new OdpsRestClient(httpClient, account, config.Endpoint, _logger);
        return (new Odps(restClient, config.Project), restClient);
    }

    private static IAccount BuildAccount(MaxComputeConfig config)
    {
        // 优先 STS（临时凭证），否则主账号
        if (!string.IsNullOrWhiteSpace(config.SecurityToken))
        {
            return new StsAccount(
                config.AccessId, config.SecretAccessKey, config.SecurityToken,
                config.Region, config.UseV4Signature);
        }

        return new CloudAccount(
            config.AccessId,
            config.SecretAccessKey,
            config.Region,
            config.UseV4Signature);
    }
}
