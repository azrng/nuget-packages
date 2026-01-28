using Azrng.NMaxCompute.Adapter;
using Azrng.NMaxCompute.Models;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;

namespace Azrng.NMaxCompute;

/// <summary>
/// MaxCompute 连接实现
/// </summary>
public class MaxComputeConnection : DbConnection
{
    private readonly string _connectionString;
    private readonly IQueryExecutor _queryExecutor;
    private readonly ILogger? _logger;
    private readonly MaxComputeConfig _config;
    private ConnectionState _state = ConnectionState.Closed;

    /// <summary>
    /// 使用配置对象创建连接
    /// </summary>
    public MaxComputeConnection(MaxComputeConfig config, IQueryExecutor queryExecutor, ILogger? logger = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _queryExecutor = queryExecutor ?? throw new ArgumentNullException(nameof(queryExecutor));
        _logger = logger;
        _connectionString = BuildConnectionString(config);

        if (!config.IsValid())
        {
            throw new ArgumentException("Invalid MaxCompute configuration.", nameof(config));
        }
    }

    /// <summary>
    /// 使用连接字符串创建连接
    /// </summary>
    public MaxComputeConnection(string connectionString, IQueryExecutor queryExecutor, ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        _queryExecutor = queryExecutor ?? throw new ArgumentNullException(nameof(queryExecutor));
        _logger = logger;
        _connectionString = connectionString;
        _config = ParseConnectionString(connectionString);

        if (!_config.IsValid())
        {
            throw new ArgumentException("Invalid connection string.", nameof(connectionString));
        }
    }

    public override string ConnectionString
    {
        get => _connectionString;
        set => throw new NotSupportedException("ConnectionString cannot be changed after initialization.");
    }

    public override int ConnectionTimeout => 30;

    public override string Database => _config.Project ?? string.Empty;

    public override string DataSource => _config.Url;

    public override string ServerVersion => "1.0";

    public override ConnectionState State => _state;

    /// <summary>
    /// 获取配置对象
    /// </summary>
    public MaxComputeConfig Config => _config;

    /// <summary>
    /// 获取查询执行器
    /// </summary>
    public IQueryExecutor QueryExecutor => _queryExecutor;

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        throw new NotSupportedException("Transactions are not supported by MaxCompute.");
    }

    public override void ChangeDatabase(string databaseName)
    {
        throw new NotSupportedException(
            "ChangeDatabase is not supported. Use a new connection with different project name.");
    }

    public override void Close()
    {
        if (_state != ConnectionState.Closed)
        {
            _state = ConnectionState.Closed;
        }
    }

    protected override DbCommand CreateDbCommand()
    {
        if (_state != ConnectionState.Open)
        {
            throw new InvalidOperationException("Connection must be open before creating a command.");
        }

        return new MaxComputeCommand(_config, _queryExecutor, _logger);
    }

    public override Task OpenAsync(CancellationToken cancellationToken)
    {
        if (_state == ConnectionState.Open)
        {
            return Task.CompletedTask;
        }

        return OpenAsyncInternal(cancellationToken);
    }

    private async Task OpenAsyncInternal(CancellationToken cancellationToken)
    {
        if (!await _queryExecutor.TestConnectionAsync(_config, cancellationToken))
        {
            throw new InvalidOperationException("Failed to open MaxCompute connection. Check your configuration.");
        }

        _state = ConnectionState.Open;
    }

    public override void Open()
    {
        OpenAsync().GetAwaiter().GetResult();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Close();
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// 构建连接字符串
    /// </summary>
    private static string BuildConnectionString(MaxComputeConfig config)
    {
        var parts = new List<string>
        {
            $"Url={config.Url}",
            $"AccessId={config.AccessId}",
            $"SecretKey={config.SecretKey}",
            $"JdbcUrl={config.JdbcUrl}"
        };

        if (!string.IsNullOrWhiteSpace(config.Project))
            parts.Add($"Project={config.Project}");

        if (config.MaxRows > 0)
            parts.Add($"MaxRows={config.MaxRows}");

        return string.Join(";", parts);
    }

    /// <summary>
    /// 解析连接字符串
    /// </summary>
    private static MaxComputeConfig ParseConnectionString(string connectionString)
    {
        var config = new MaxComputeConfig();
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var keyValue = part.Split('=', 2);
            if (keyValue.Length != 2)
                continue;

            var key = keyValue[0].Trim();
            var value = keyValue[1].Trim();

            switch (key)
            {
                case "Url":
                    config.Url = value;
                    break;
                case "AccessId":
                    config.AccessId = value;
                    break;
                case "SecretKey":
                    config.SecretKey = value;
                    break;
                case "JdbcUrl":
                    config.JdbcUrl = value;
                    break;
                case "Project":
                    config.Project = value;
                    break;
                case "MaxRows" when int.TryParse(value, out var maxRows):
                    config.MaxRows = maxRows;
                    break;
            }
        }

        return config;
    }
}
