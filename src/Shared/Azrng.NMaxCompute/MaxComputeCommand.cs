using Azrng.NMaxCompute.Adapter;
using Azrng.NMaxCompute.Models;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;

namespace Azrng.NMaxCompute;

/// <summary>
/// MaxCompute 命令实现
/// </summary>
public class MaxComputeCommand : DbCommand
{
    private readonly ILogger? _logger;
    private string? _commandText;
    private int _commandTimeout = 30;
    private readonly IQueryExecutor _queryExecutor;
    private readonly MaxComputeParameterCollection _parameters;
    private readonly MaxComputeConfig _config;

    internal MaxComputeCommand(MaxComputeConfig config, IQueryExecutor queryExecutor, ILogger? logger = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _queryExecutor = queryExecutor ?? throw new ArgumentNullException(nameof(queryExecutor));
        _parameters = new MaxComputeParameterCollection();
        _logger = logger;
    }

    public override string? CommandText
    {
        get => _commandText;
        set => _commandText = value;
    }

    public override int CommandTimeout
    {
        get => _commandTimeout;
        set => _commandTimeout = value;
    }

    public override CommandType CommandType { get; set; } = CommandType.Text;

    public override bool DesignTimeVisible { get; set; }

    public override UpdateRowSource UpdatedRowSource { get; set; }

    /// <summary>
    /// 单命令级 SQL hints，覆盖 <see cref="MaxComputeConfig.Hints"/>（同名 key 以本属性为准）。
    /// <para>注入到 SQLTask 的 settings，例：<c>odps.sql.mapper.split.size = 256</c>。</para>
    /// </summary>
    public IDictionary<string, string>? Hints { get; set; }

    protected override DbConnection? DbConnection { get; set; }

    protected override DbParameterCollection DbParameterCollection => _parameters;

    protected override DbTransaction? DbTransaction { get; set; }

    /// <summary>
    /// 获取参数集合
    /// </summary>
    public MaxComputeParameterCollection MaxComputeParameters => _parameters;

    public override void Cancel()
    {
        _logger?.LogWarning("Cancel operation is not supported by MaxCompute");
    }

    protected override DbParameter CreateDbParameter()
    {
        return new MaxComputeParameter();
    }

    public override int ExecuteNonQuery()
    {
        if (string.IsNullOrWhiteSpace(_commandText))
        {
            throw new InvalidOperationException("CommandText must be set before execution.");
        }

        try
        {
            var processedSql = ProcessParameters();
            var result = _queryExecutor.ExecuteQueryAsync(MergeConfig(), processedSql)
                                       .GetAwaiter()
                                       .GetResult();

            return result.RowCount;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to execute non-query command: {CommandText}", _commandText);
            throw;
        }
    }

    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_commandText))
        {
            throw new InvalidOperationException("CommandText must be set before execution.");
        }

        return ExecuteNonQueryAsyncInternal(cancellationToken);
    }

    private async Task<int> ExecuteNonQueryAsyncInternal(CancellationToken cancellationToken)
    {
        try
        {
            var processedSql = ProcessParameters();
            var result = await _queryExecutor.ExecuteQueryAsync(MergeConfig(), processedSql, cancellationToken);
            return result.RowCount;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to execute non-query command: {CommandText}", _commandText);
            throw;
        }
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        if (string.IsNullOrWhiteSpace(_commandText))
        {
            throw new InvalidOperationException("CommandText must be set before execution.");
        }

        try
        {
            var processedSql = ProcessParameters();
            var result = _queryExecutor.ExecuteQueryAsync(MergeConfig(), processedSql)
                                       .GetAwaiter()
                                       .GetResult();

            return new MaxComputeDataReader(result, _logger);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to execute reader command: {CommandText}", _commandText);
            throw;
        }
    }

    protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_commandText))
        {
            throw new InvalidOperationException("CommandText must be set before execution.");
        }

        try
        {
            var processedSql = ProcessParameters();
            var result = await _queryExecutor.ExecuteQueryAsync(MergeConfig(), processedSql, cancellationToken);
            return new MaxComputeDataReader(result, _logger);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to execute reader command: {CommandText}", _commandText);
            throw;
        }
    }

    public override object? ExecuteScalar()
    {
        if (string.IsNullOrWhiteSpace(_commandText))
        {
            throw new InvalidOperationException("CommandText must be set before execution.");
        }

        try
        {
            var processedSql = ProcessParameters();
            var result = _queryExecutor.ExecuteQueryAsync(MergeConfig(), processedSql)
                                       .GetAwaiter()
                                       .GetResult();

            if (result.Rows != null && result.Rows.Length > 0 && result.Rows[0].Length > 0)
            {
                return result.Rows[0][0];
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to execute scalar command: {CommandText}", _commandText);
            throw;
        }
    }

    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_commandText))
        {
            throw new InvalidOperationException("CommandText must be set before execution.");
        }

        return ExecuteScalarAsyncInternal(cancellationToken);
    }

    private async Task<object?> ExecuteScalarAsyncInternal(CancellationToken cancellationToken)
    {
        try
        {
            var processedSql = ProcessParameters();
            var result = await _queryExecutor.ExecuteQueryAsync(MergeConfig(), processedSql, cancellationToken);

            if (result.Rows != null && result.Rows.Length > 0 && result.Rows[0].Length > 0)
            {
                return result.Rows[0][0];
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to execute scalar command: {CommandText}", _commandText);
            throw;
        }
    }

    public override void Prepare()
    {
        _logger?.LogWarning("Prepare operation is not supported by MaxCompute");
    }

    /// <summary>
    /// 处理参数化查询，将参数替换到SQL中
    /// </summary>
    private string ProcessParameters()
    {
        var sql = _commandText!;

        if (_parameters.Count == 0)
            return sql;

        foreach (MaxComputeParameter parameter in _parameters)
        {
            if (string.IsNullOrWhiteSpace(parameter.ParameterName))
                continue;

            var paramName = parameter.ParameterName;
            var paramValue = FormatParameterValue(parameter.Value);

            // 支持 @param 和 :param 格式
            sql = sql.Replace($"@{paramName}", paramValue);
            sql = sql.Replace($":{paramName}", paramValue);

            // 也支持 {param} 格式
            sql = sql.Replace($"{{{paramName}}}", paramValue);
        }

        return sql;
    }

    /// <summary>
    /// 合并 config.Hints 与本命令的 <see cref="Hints"/>：命令级覆盖配置级（同名 key 以命令为准）。
    /// <para>internal 便于测试：确保合并后的 config 保留所有非 Hints 字段（如 UseLocalTimeZone）。</para>
    /// </summary>
    internal MaxComputeConfig MergeConfig()
    {
        if (Hints is null || Hints.Count == 0)
            return _config;

        var merged = new MaxComputeConfig
        {
            Endpoint = _config.Endpoint,
            AccessId = _config.AccessId,
            SecretAccessKey = _config.SecretAccessKey,
            Project = _config.Project,
            Schema = _config.Schema,
            Region = _config.Region,
            SecurityToken = _config.SecurityToken,
            TunnelEndpoint = _config.TunnelEndpoint,
            MaxRows = _config.MaxRows,
            UseV4Signature = _config.UseV4Signature,
            UseLocalTimeZone = _config.UseLocalTimeZone
        };

        var hints = new Dictionary<string, string>(StringComparer.Ordinal);
        if (_config.Hints != null)
        {
            foreach (var kv in _config.Hints)
                hints[kv.Key] = kv.Value;
        }
        foreach (var kv in Hints)
            hints[kv.Key] = kv.Value;

        merged.Hints = hints.Count > 0 ? hints : null;
        return merged;
    }

    /// <summary>
    /// 格式化参数值
    /// </summary>
    private string FormatParameterValue(object? value)
    {
        if (value == null || value == DBNull.Value)
            return "NULL";

        return value switch
        {
            string str => $"'{str.Replace("'", "''")}'",
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
            bool b => b ? "1" : "0",
            byte[] => "'<BINARY>'",
            _ => value.ToString() ?? "NULL"
        };
    }
}
