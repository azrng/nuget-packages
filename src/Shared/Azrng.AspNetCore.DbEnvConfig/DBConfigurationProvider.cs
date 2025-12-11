using Microsoft.Extensions.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;

namespace Azrng.AspNetCore.DbEnvConfig;

/// <summary>
/// 数据库配置提供服务
/// </summary>
public class DbConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly DBConfigOptions _options;
    private readonly IScriptService _scriptService;

    /// <summary>
    /// 允许多处读取 单个写入
    /// </summary>
    private readonly ReaderWriterLockSlim _lockObj = new ReaderWriterLockSlim();

    /// <summary>
    /// 是否被释放
    /// </summary>
    private bool _isDisposed;

    public DbConfigurationProvider(DBConfigOptions options, IScriptService scriptService)
    {
        _options = options;
        _scriptService = scriptService;
        InitTable();

        if (options.ReloadOnChange)
        {
            ThreadPool.QueueUserWorkItem(obj =>
            {
                while (!_isDisposed)
                {
                    Load();
                    Thread.Sleep(options.ReloadInterval);
                }
            });
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

            Console.WriteLine("{0}发生异常, 时间:{1:yyyy-MM-dd HH:mm:ss}, 原因:{2} {3}", nameof(DbConfigurationProvider), DateTime.Now,
                ex.Message, ex.StackTrace);
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

    private void Dispose(bool disposing)
    {
        Console.WriteLine($"{nameof(DbConfigurationProvider)} {nameof(Dispose)}");
        if (_isDisposed)
            return;

        _isDisposed = true;
    }

    ~DbConfigurationProvider() => Dispose(false);

    private void DoLoad()
    {
        using var conn = _options.CreateDbConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        var selectScript =
            $"select {_options.ConfigKeyField},{_options.ConfigValueField} from {_options.FullTableName} where 1=1 {_options.FilterWhere}";
        if (_options.IsConsoleQueryLog)
            Console.WriteLine($"{nameof(DbConfigurationProvider)} query: {selectScript} {DateTime.Now}");
        cmd.CommandText = selectScript;

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var name = reader.IsDBNull(0) ? null : reader.GetString(0);
            if (name is null)
            {
                Console.WriteLine($"{nameof(DbConfigurationProvider)}:配置key不能为null");
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
            using var conn = _options.CreateDbConnection();
            conn.Open();

            var initTableScript = _scriptService.GetInitTableScript(_options.TableName, _options.ConfigKeyField, _options.ConfigValueField,
                _options.Schema);
            if (!string.IsNullOrWhiteSpace(initTableScript))
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = initTableScript;
                var i = cmd.ExecuteNonQuery();
                Console.WriteLine($"{nameof(DbConfigurationProvider)} 初始化创建表脚本成功");
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
                    var i = cmd.ExecuteNonQuery();
                    Console.WriteLine($"{nameof(DbConfigurationProvider)} 初始化数据脚本成功");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{nameof(DbConfigurationProvider)} Create data fail; message" + ex);
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