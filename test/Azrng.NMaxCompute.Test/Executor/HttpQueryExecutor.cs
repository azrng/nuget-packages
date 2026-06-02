using System.Text;
using Azrng.NMaxCompute.Adapter;
using Azrng.NMaxCompute.Models;
using Azrng.NMaxCompute.Test.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Azrng.NMaxCompute.Test.Executor;

/// <summary>
/// 基于 HTTP 的 MaxCompute 查询执行器实现
/// </summary>
public class HttpQueryExecutor : IQueryExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpQueryExecutor> _logger;

    public HttpQueryExecutor(IHttpClientFactory httpClientFactory, ILogger<HttpQueryExecutor> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueryResult> ExecuteQueryAsync(MaxComputeConfig config, string sql, CancellationToken cancellationToken = default)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentNullException(nameof(sql));

        using var client = _httpClientFactory.CreateClient();
        var serverUrl = config.ServerUrl;
        var url = serverUrl.TrimEnd('/') + "/api/sql/execute";

        var request = new QuerySingleSqlRequestHo(config)
        {
            Sql = sql
        };

        var json = JsonConvert.SerializeObject(request);
        _logger.LogInformation("Executing query: {Sql}", sql);

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content, cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        var result = JsonConvert.DeserializeObject<QueryResponse<QuerySingleSqlResponseHo>>(responseString);

        if (result == null || result.Status != "success")
        {
            _logger.LogError("Query failed: {Message}", result?.Message ?? responseString);
            throw new InvalidOperationException($"Query failed: {result?.Message ?? responseString}");
        }

        return new QueryResult
        {
            Columns = result.Data?.Columns ?? Array.Empty<string>(),
            Rows = result.Data?.Rows ?? Array.Empty<object[]>(),
            RowCount = result.Data?.RowCount ?? 0,
            ExecutionTime = result.Data?.ExecutionTime
        };
    }

    public async Task<bool> TestConnectionAsync(MaxComputeConfig config, CancellationToken cancellationToken = default)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        try
        {
            var result = await ExecuteQueryAsync(config, "SELECT 1", cancellationToken);
            return result.RowCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection test failed");
            return false;
        }
    }
}
