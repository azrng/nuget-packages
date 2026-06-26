using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;

namespace Azrng.AspNetCore.DbEnvConfig;

/// <summary>
/// 数据库配置提供程序
/// </summary>
public class DbConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly DbConfigOptions _options;
    private readonly IScriptService _scriptService;
    private readonly ILogger _logger;

    /// <summary>
    /// 允许多处读取 单个写入
    /// </summary>
    private readonly ReaderWriterLockSlim _lockObj = new();

    /// <summary>
    /// 是否被释放
    /// </summary>
    private volatile bool _isDisposed;

    /// <summary>
    /// 后台轮询取消控制
    /// </summary>
    private readonly CancellationTokenSource? _reloadCts;
    private readonly Task? _reloadTask;

    public DbConfigurationProvider(DbConfigOptions options, IScriptService scriptService, ILogger? logger = null)
    {
        _options = options;
        _scriptService = scriptService;
        _logger = logger ?? NullLogger.Instance;
        InitTable();

        if (options.ReloadOnChange)
        {
            _reloadCts = new CancellationTokenSource();
            _reloadTask = Task.Run(() => ReloadLoopAsync(_reloadCts.Token));
        }
    }

    public override void Load()
    {
        base.Load();
        IDictionary<string, string?> clonedData = new Dictionary<string, string?>();
        try
        {
            _lockObj.EnterWriteLock();
            clonedData = Data!.Clone();
            Data.Clear();
            DoLoad();
        }
        catch (DbException ex)
        {
            //if DbException is thrown, restore to the original data.
            this.Data = clonedData;

            _logger.LogError(ex, "{Provider} 加载配置发生异常,将回滚至上次成功的数据", nameof(DbConfigurationProvider));
        }
        finally
        {
            _lockObj.ExitWriteLock();
        }

        //OnReload cannot be between EnterWriteLock and ExitWriteLock, or "A read lock may not be acquired with the write lock held in this mode" will be thrown.
        if (Helper.IsChanged(clonedData, Data))
        {
            OnReload();
        }
    }

    public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        _lockObj.EnterReadLock();
        try
        {
            return base.GetChildKeys(earlierKeys, parentPath);
        }
        finally
        {
            _lockObj.ExitReadLock();
        }
    }

    public override bool TryGet(string key, out string? value)
    {
        _lockObj.EnterReadLock();
        try
        {
            return base.TryGet(key, out value);
        }
        finally
        {
            _lockObj.ExitReadLock();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing">为 true 表示由 Dispose 调用(释放托管资源);为 false 表示由终结器调用</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        if (disposing)
        {
            // 通知后台轮询停止并等待其退出，避免 Dispose 后仍执行一次 Load
            if (_reloadCts is not null)
            {
                _reloadCts.Cancel();
                try
                {
                    // 带超时等待，防止后台任务卡住导致 Dispose 永久阻塞
                    _reloadTask?.Wait(TimeSpan.FromSeconds(5));
                }
                catch (AggregateException ex)
                {
                    _logger.LogWarning(ex, "{Provider} 等待后台刷新任务退出时出现异常", nameof(DbConfigurationProvider));
                }
                _reloadCts.Dispose();
            }

            _lockObj.Dispose();
        }
    }

    private async Task ReloadLoopAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(_options.ReloadInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                if (_isDisposed)
                    return;
                Load();
            }
        }
        catch (OperationCanceledException)
        {
            // Dispose 触发的正常取消，忽略
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Provider} 后台刷新任务异常退出", nameof(DbConfigurationProvider));
        }
    }

    /// <summary>
    /// 创建数据库连接。<see cref="DbConfigOptions.CreateDbConnection"/> 在 <see cref="DbConfigOptions.Normalize"/> 中已校验非空。
    /// </summary>
    private IDbConnection CreateConnection()
    {
        var factory = _options.CreateDbConnection
            ?? throw new InvalidOperationException("CreateDbConnection 工厂未设置");
        return factory()
            ?? throw new InvalidOperationException("CreateDbConnection 工厂返回了 null 连接");
    }

    private void DoLoad()
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        var selectScript =
            $"select {_options.ConfigKeyField},{_options.ConfigValueField} from {_options.FullTableName} where 1=1 {_options.FilterWhere}";
        if (_options.IsConsoleQueryLog)
            _logger.LogInformation("{Provider} query: {Script} {Time}", nameof(DbConfigurationProvider), selectScript, DateTime.Now);
        cmd.CommandText = selectScript;

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var name = reader.IsDBNull(0) ? null : reader.GetString(0);
            if (name is null)
            {
                _logger.LogWarning("{Provider}:配置key不能为null", nameof(DbConfigurationProvider));
                continue;
            }

            var value = reader.IsDBNull(1) ? null : reader.GetString(1);
            if (value is null)
            {
                continue;
            }

            value = value.Trim();

            //if the value is like [...] or {} , it may be a json array value or json object value,
            //so try to parse it as json
            if (value.StartsWith('[') && value.EndsWith(']') || value.StartsWith('{') && value.EndsWith('}'))
            {
                TryLoadAsJson(name, value);
            }
            else
            {
                Data[name] = value;
            }
        }
    }

    private void InitTable()
    {
        try
        {
            using var conn = CreateConnection();
            conn.Open();

            var initTableScript = _scriptService.GetInitTableScript(_options.TableName, _options.ConfigKeyField, _options.ConfigValueField,
                _options.Schema);
            if (!string.IsNullOrWhiteSpace(initTableScript))
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = initTableScript;
                cmd.ExecuteNonQuery();
                _logger.LogInformation("{Provider} 初始化创建表脚本成功", nameof(DbConfigurationProvider));
            }

            var initDataScript = _scriptService.GetInitTableDataScript();
            if (!string.IsNullOrWhiteSpace(initDataScript))
            {
                using var countCmd = conn.CreateCommand();
                countCmd.CommandText = $"select count(*) from {_options.FullTableName}";
                var tableCount = Convert.ToInt32(countCmd.ExecuteScalar());
                if (tableCount == 0)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = initDataScript;
                    cmd.ExecuteNonQuery();
                    _logger.LogInformation("{Provider} 初始化数据脚本成功", nameof(DbConfigurationProvider));
                }
            }
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "{Provider} 初始化表/数据失败", nameof(DbConfigurationProvider));
        }
        catch (InvalidOperationException ex)
        {
            // 连接工厂配置错误、连接未正确打开等
            _logger.LogError(ex, "{Provider} 初始化表/数据失败(连接或配置错误)", nameof(DbConfigurationProvider));
        }
    }

    private void LoadJsonElement(string name, JsonElement jsonRoot)
    {
        switch (jsonRoot.ValueKind)
        {
            case JsonValueKind.Array:
                {
                    var index = 0;
                    foreach (var item in jsonRoot.EnumerateArray())
                    {
                        //https://andrewlock.net/creating-a-custom-iconfigurationprovider-in-asp-net-core-to-parse-yaml/
                        //parse as "a:b:0"="hello";"a:b:1"="world"
                        var path = name + ConfigurationPath.KeyDelimiter + index;
                        LoadJsonElement(path, item);
                        index++;
                    }

                    break;
                }
            case JsonValueKind.Object:
                {
                    foreach (var jsonObj in jsonRoot.EnumerateObject())
                    {
                        var pathOfObj = name + ConfigurationPath.KeyDelimiter + jsonObj.Name;
                        LoadJsonElement(pathOfObj, jsonObj.Value);
                    }

                    break;
                }
            default:
                //if it is not json array or object, parse it as plain string value
                Data[name] = jsonRoot.GetValueForConfig();
                break;
        }
    }

    private void TryLoadAsJson(string name, string value)
    {
        var jsonOptions = new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
        try
        {
            var jsonRoot = JsonDocument.Parse(value, jsonOptions).RootElement;
            LoadJsonElement(name, jsonRoot);
        }
        catch (JsonException ex)
        {
            //if it is not valid json, parse it as plain string value
            this.Data[name] = value;
            Debug.WriteLine($"When trying to parse {value} as json object, exception was thrown. {ex}");
        }
    }
}
