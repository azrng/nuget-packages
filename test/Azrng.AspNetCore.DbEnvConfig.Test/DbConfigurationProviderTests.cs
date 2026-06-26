using Azrng.AspNetCore.DbEnvConfig;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Azrng.AspNetCore.DbEnvConfig.Test;

public class DbConfigurationProviderTests
{
    [Fact]
    public void Normalize_WithSchemaTableName_SplitsSchemaAndBuildsFullTableName()
    {
        var options = new DbConfigOptions
        {
            CreateDbConnection = () => new FakeDbConnection(new FakeDatabaseState()),
            TableName = "config.system_config",
            ConfigKeyField = "code",
            ConfigValueField = "value"
        };

        options.Normalize();

        options.Schema.Should().Be("config");
        options.TableName.Should().Be("system_config");
        options.FullTableName.Should().Be("config.system_config");
    }

    [Fact]
    public void Normalize_WithInvalidArguments_ThrowsMeaningfulExceptions()
    {
        var missingConnection = new DbConfigOptions
        {
            TableName = "config.system_config",
            ConfigKeyField = "code",
            ConfigValueField = "value"
        };

        var invalidTableName = new DbConfigOptions
        {
            CreateDbConnection = () => new FakeDbConnection(new FakeDatabaseState()),
            TableName = "a.b.c",
            ConfigKeyField = "code",
            ConfigValueField = "value"
        };

        var blankFields = new DbConfigOptions
        {
            CreateDbConnection = () => new FakeDbConnection(new FakeDatabaseState()),
            TableName = "system_config",
            ConfigKeyField = "",
            ConfigValueField = ""
        };

        missingConnection.Invoking(x => x.Normalize())
            .Should().Throw<ArgumentNullException>()
            .WithParameterName(nameof(DbConfigOptions.CreateDbConnection));

        invalidTableName.Invoking(x => x.Normalize())
            .Should().Throw<ArgumentException>()
            .WithParameterName(nameof(DbConfigOptions.TableName));

        blankFields.Invoking(x => x.Normalize())
            .Should().Throw<ArgumentException>()
            .WithParameterName(nameof(DbConfigOptions.ConfigKeyField));
    }

    [Fact]
    public void AddDbConfiguration_WithNullArguments_ThrowsArgumentNullException()
    {
        IConfigurationBuilder? builder = null;
        Action<DbConfigOptions>? action = null;

        var nullBuilderAct = () => DbConfigurationProviderExtensions.AddDbConfiguration(builder!, _ => { });
        var nullActionAct = () => new ConfigurationBuilder().AddDbConfiguration(action!);

        nullBuilderAct.Should().Throw<ArgumentNullException>()
            .WithParameterName("builder");
        nullActionAct.Should().Throw<ArgumentNullException>()
            .WithParameterName("action");
    }

    [Fact]
    public void PostgreSqlScriptService_BuildsSchemaAwareInitScript()
    {
        var script = new PostgreSqlScriptService()
            .GetInitTableScript("system_config", "code", "value", "config");

        script.Should().Contain("CREATE SCHEMA IF NOT EXISTS config;");
        script.Should().Contain("CREATE TABLE IF NOT EXISTS config.system_config");
        script.Should().Contain("code VARCHAR(50) NOT NULL");
        script.Should().Contain("value VARCHAR(2000) NOT NULL");
    }

    [Fact]
    public void AddDbConfiguration_LoadsPlainValuesAndFlattensJsonObjects()
    {
        var state = new FakeDatabaseState
        {
            Rows =
            {
                ("plain", "  value  "),
                ("json", "{\"nested\":{\"flag\":true},\"list\":[1,\"two\"]}"),
                ("invalidJson", "{not-json}"),
                (null, "ignored"),
                ("nullValue", null)
            }
        };

        var configuration = new ConfigurationBuilder()
            .AddDbConfiguration(options =>
            {
                options.CreateDbConnection = () => new FakeDbConnection(state);
                options.TableName = "config.system_config";
                options.ConfigKeyField = "code";
                options.ConfigValueField = "value";
                options.FilterWhere = " AND enabled = true";
                options.ReloadOnChange = false;
                options.IsConsoleQueryLog = false;
            }, new TrackingScriptService())
            .Build();

        configuration["plain"].Should().Be("value");
        configuration["json:nested:flag"].Should().Be("true");
        configuration["json:list:0"].Should().Be("1");
        configuration["json:list:1"].Should().Be("two");
        configuration["invalidJson"].Should().Be("{not-json}");
        configuration["nullValue"].Should().BeNull();

        state.ExecutedCommands.Should().ContainSingle(command =>
            command.Contains("select code,value from config.system_config", StringComparison.OrdinalIgnoreCase) &&
            command.Contains("enabled = true", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DbConfigurationProvider_InitializesTableAndSeedsDataWhenTableIsEmpty()
    {
        var state = new FakeDatabaseState
        {
            TableCount = 0
        };
        var scriptService = new TrackingScriptService
        {
            InitTableScript = "CREATE TABLE custom_config (...);",
            InitDataScript = "INSERT INTO custom_config VALUES ('code','value');"
        };
        var options = new DbConfigOptions
        {
            CreateDbConnection = () => new FakeDbConnection(state),
            TableName = "config.custom_config",
            ConfigKeyField = "code",
            ConfigValueField = "value",
            ReloadOnChange = false,
            IsConsoleQueryLog = false
        };
        options.Normalize();

        using var provider = new DbConfigurationProvider(options, scriptService);

        state.ExecutedCommands.Should().Contain(scriptService.InitTableScript);
        state.ExecutedCommands.Should().Contain("select count(*) from config.custom_config");
        state.ExecutedCommands.Should().Contain(scriptService.InitDataScript);
    }

    [Fact]
    public void DbConfigurationProvider_RestoresPreviousDataWhenLoadThrowsDbException()
    {
        // 首次加载一条正常数据，后续 ExecuteReader 抛 DbException，验证 Data 被回滚而非清空
        var state = new FakeDatabaseState
        {
            Rows = { ("k1", "v1") }
        };

        var options = new DbConfigOptions
        {
            CreateDbConnection = () => new FakeDbConnection(state),
            TableName = "config.system_config",
            ConfigKeyField = "code",
            ConfigValueField = "value",
            ReloadOnChange = false,
            IsConsoleQueryLog = false
        };
        options.Normalize();

        // 构造时 InitTable 不触发 reader；首次 Load 读取 k1=v1
        using var provider = new DbConfigurationProvider(options, new TrackingScriptService());
        provider.Load();
        provider.TryGet("k1", out var first).Should().BeTrue();
        first.Should().Be("v1");

        // 配置 reader 抛异常后再次 Load，应回滚到上次成功的数据
        state.ThrowOnReader = true;
        provider.Load();

        provider.TryGet("k1", out var restored).Should().BeTrue();
        restored.Should().Be("v1");
    }

    [Fact]
    public async Task DbConfigurationProvider_BackgroundReloadStopsAfterDispose()
    {
        // 用极短轮询间隔启动后台线程，Dispose 后断言不再产生新的 select 查询
        var state = new FakeDatabaseState
        {
            Rows = { ("k1", "v1") }
        };

        var options = new DbConfigOptions
        {
            CreateDbConnection = () => new FakeDbConnection(state),
            TableName = "config.system_config",
            ConfigKeyField = "code",
            ConfigValueField = "value",
            ReloadOnChange = true,
            ReloadInterval = TimeSpan.FromMilliseconds(50),
            IsConsoleQueryLog = false
        };
        options.Normalize();

        var provider = new DbConfigurationProvider(options, new TrackingScriptService());

        // 让后台线程跑若干轮，确认 select 持续执行
        await Task.Delay(300);
        var countBeforeDispose = state.ExecutedCommands.Count;
        countBeforeDispose.Should().BeGreaterThan(0);

        provider.Dispose();

        // Dispose 后再等待一段足以触发多次轮询的时间
        await Task.Delay(400);

        // Dispose 之后不应再有任何新命令执行
        state.ExecutedCommands.Count.Should().Be(countBeforeDispose);
    }

    [Fact]
    public void AddDbConfiguration_CanBuildAgainAfterPreviousRootDisposed()
    {
        var state = new FakeDatabaseState
        {
            Rows = { ("k1", "v1") }
        };
        var builder = new ConfigurationBuilder()
            .AddDbConfiguration(options =>
            {
                options.CreateDbConnection = () => new FakeDbConnection(state);
                options.TableName = "config.system_config";
                options.ConfigKeyField = "code";
                options.ConfigValueField = "value";
                options.ReloadOnChange = false;
                options.IsConsoleQueryLog = false;
            }, new TrackingScriptService());

        var firstRoot = builder.Build();
        firstRoot["k1"].Should().Be("v1");
        (firstRoot as IDisposable)?.Dispose();

        state.Rows.Clear();
        state.Rows.Add(("k1", "v2"));

        var secondRoot = builder.Build();
        secondRoot["k1"].Should().Be("v2");
        (secondRoot as IDisposable)?.Dispose();
    }

    [Fact]
    public void DbConfigurationProvider_DisposeDoesNotDisposeLockWhileBackgroundLoadIsStillRunning()
    {
        var state = new FakeDatabaseState
        {
            Rows = { ("k1", "v1") },
            ReaderDelay = TimeSpan.FromSeconds(6)
        };
        var options = new DbConfigOptions
        {
            CreateDbConnection = () => new FakeDbConnection(state),
            TableName = "config.system_config",
            ConfigKeyField = "code",
            ConfigValueField = "value",
            ReloadOnChange = true,
            ReloadInterval = TimeSpan.FromMilliseconds(10),
            IsConsoleQueryLog = false
        };
        options.Normalize();

        var provider = new DbConfigurationProvider(options, new TrackingScriptService());

        state.ReaderStarted.Wait(TimeSpan.FromSeconds(2)).Should().BeTrue();

        var act = () => provider.Dispose();

        act.Should().NotThrow();
        state.ReaderCompleted.Wait(TimeSpan.FromSeconds(3)).Should().BeTrue();
        provider.Invoking(x => x.TryGet("k1", out _)).Should().NotThrow();
    }

    [Fact]
    public void DbConfigurationProvider_OnReloadOnlyFiresWhenDataActuallyChanges()
    {
        // 间接覆盖 Helper.IsChanged：连续两次加载相同数据，OnReload 不应改变 reload token
        var state = new FakeDatabaseState
        {
            Rows = { ("k1", "v1") }
        };
        var options = new DbConfigOptions
        {
            CreateDbConnection = () => new FakeDbConnection(state),
            TableName = "config.system_config",
            ConfigKeyField = "code",
            ConfigValueField = "value",
            ReloadOnChange = false,
            IsConsoleQueryLog = false
        };
        options.Normalize();

        using var provider = new DbConfigurationProvider(options, new TrackingScriptService());
        var configuration = new ConfigurationRoot(new[] { provider });

        var tokenBefore = configuration.GetReloadToken();
        tokenBefore.HasChanged.Should().BeFalse();

        provider.Load(); // 相同数据，不应触发 reload
        tokenBefore.HasChanged.Should().BeFalse();

        state.Rows.Clear();
        state.Rows.Add(("k1", "v2"));
        provider.Load(); // 数据变化，应触发 reload
        tokenBefore.HasChanged.Should().BeTrue();
    }
}

internal sealed class TrackingScriptService : IScriptService
{
    public string InitTableScript { get; set; } = string.Empty;

    public string InitDataScript { get; set; } = string.Empty;

    public string GetInitTableScript(string tableName, string field, string value, string? schema = null)
    {
        return InitTableScript;
    }

    public string GetInitTableDataScript()
    {
        return InitDataScript;
    }
}

internal sealed class FakeDatabaseState
{
    public List<(string? Key, string? Value)> Rows { get; } = [];

    public List<string> ExecutedCommands { get; } = [];

    public int TableCount { get; set; }

    /// <summary>
    /// 设置为 true 时，ExecuteReader 抛出 DbException，用于测试加载失败回滚
    /// </summary>
    public bool ThrowOnReader { get; set; }

    public TimeSpan ReaderDelay { get; set; }

    public ManualResetEventSlim ReaderStarted { get; } = new();

    public ManualResetEventSlim ReaderCompleted { get; } = new();
}

internal sealed class FakeDbConnection(FakeDatabaseState state) : IDbConnection
{
    [AllowNull]
    public string ConnectionString { get; set; } = string.Empty;

    public int ConnectionTimeout => 0;

    public string Database => "Fake";

    public ConnectionState State { get; private set; } = ConnectionState.Closed;

    public IDbTransaction BeginTransaction()
    {
        return new FakeDbTransaction(this);
    }

    public IDbTransaction BeginTransaction(IsolationLevel il)
    {
        return new FakeDbTransaction(this);
    }

    public void ChangeDatabase(string databaseName)
    {
    }

    public void Close()
    {
        State = ConnectionState.Closed;
    }

    public IDbCommand CreateCommand()
    {
        return new FakeDbCommand(state, this);
    }

    public void Open()
    {
        State = ConnectionState.Open;
    }

    public void Dispose()
    {
        Close();
    }
}

internal sealed class FakeDbCommand(FakeDatabaseState state, IDbConnection connection) : IDbCommand
{
    [AllowNull]
    public string CommandText { get; set; } = string.Empty;

    public int CommandTimeout { get; set; }

    public CommandType CommandType { get; set; } = CommandType.Text;

    public IDbConnection? Connection { get; set; } = connection;

    public IDataParameterCollection Parameters { get; } = new FakeParameterCollection();

    public IDbTransaction? Transaction { get; set; }

    public UpdateRowSource UpdatedRowSource { get; set; }

    public void Cancel()
    {
    }

    public IDbDataParameter CreateParameter()
    {
        return new FakeDbParameter();
    }

    public void Dispose()
    {
    }

    public int ExecuteNonQuery()
    {
        state.ExecutedCommands.Add(CommandText ?? string.Empty);
        return 1;
    }

    public IDataReader ExecuteReader()
    {
        state.ExecutedCommands.Add(CommandText ?? string.Empty);
        if (state.ThrowOnReader)
        {
            throw new TestDbException("模拟读取失败");
        }
        return state.ReaderDelay > TimeSpan.Zero ? CreateDelayedReader() : CreateReader(state.Rows);
    }

    public IDataReader ExecuteReader(CommandBehavior behavior)
    {
        state.ExecutedCommands.Add(CommandText ?? string.Empty);
        if (state.ThrowOnReader)
        {
            throw new TestDbException("模拟读取失败");
        }
        return state.ReaderDelay > TimeSpan.Zero ? CreateDelayedReader() : CreateReader(state.Rows);
    }

    public object ExecuteScalar()
    {
        state.ExecutedCommands.Add(CommandText ?? string.Empty);
        return state.TableCount;
    }

    public void Prepare()
    {
    }

    private static IDataReader CreateReader(IEnumerable<(string? Key, string? Value)> rows)
    {
        var table = new DataTable();
        table.Columns.Add("code", typeof(string));
        table.Columns.Add("value", typeof(string));

        foreach (var (key, value) in rows)
        {
            table.Rows.Add(key is null ? DBNull.Value : key, value is null ? DBNull.Value : value);
        }

        return table.CreateDataReader();
    }

    private IDataReader CreateDelayedReader()
    {
        state.ReaderStarted.Set();
        if (state.ReaderDelay > TimeSpan.Zero)
        {
            Thread.Sleep(state.ReaderDelay);
        }

        var reader = CreateReader(state.Rows);
        state.ReaderCompleted.Set();
        return reader;
    }
}

internal sealed class FakeDbTransaction(IDbConnection connection) : IDbTransaction
{
    public IDbConnection Connection => connection;

    public IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;

    public void Commit()
    {
    }

    public void Dispose()
    {
    }

    public void Rollback()
    {
    }
}

internal sealed class FakeParameterCollection : List<object>, IDataParameterCollection
{
    public object this[string parameterName]
    {
        get => this.FirstOrDefault(x => x is IDataParameter parameter && parameter.ParameterName == parameterName)!;
        set
        {
            var index = IndexOf(parameterName);
            if (index >= 0 && value != null)
            {
                this[index] = value;
            }
        }
    }

    public bool Contains(string parameterName)
    {
        return this.Any(x => x is IDataParameter parameter && parameter.ParameterName == parameterName);
    }

    public int IndexOf(string parameterName)
    {
        return FindIndex(x => x is IDataParameter parameter && parameter.ParameterName == parameterName);
    }

    public void RemoveAt(string parameterName)
    {
        var index = IndexOf(parameterName);
        if (index >= 0)
        {
            RemoveAt(index);
        }
    }
}

internal sealed class FakeDbParameter : IDbDataParameter
{
    public DbType DbType { get; set; }

    public ParameterDirection Direction { get; set; } = ParameterDirection.Input;

    public bool IsNullable => true;

    [AllowNull]
    public string ParameterName { get; set; } = string.Empty;

    [AllowNull]
    public string SourceColumn { get; set; } = string.Empty;

    public DataRowVersion SourceVersion { get; set; } = DataRowVersion.Current;

    public object? Value { get; set; }

    public byte Precision { get; set; }

    public byte Scale { get; set; }

    public int Size { get; set; }
}

/// <summary>
/// 用于测试的 DbException 派生类，确保能被 Load 的 catch(DbException) 捕获
/// </summary>
internal sealed class TestDbException(string? message) : DbException(message);
