using Azrng.AspNetCore.DbEnvConfig;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Data;
using Xunit;

namespace Azrng.AspNetCore.DbEnvConfig.Test;

public class DbConfigurationProviderTests
{
    [Fact]
    public void ParamVerify_WithSchemaTableName_SplitsSchemaAndBuildsFullTableName()
    {
        var options = new DBConfigOptions
        {
            CreateDbConnection = () => new FakeDbConnection(new FakeDatabaseState()),
            TableName = "config.system_config",
            ConfigKeyField = "code",
            ConfigValueField = "value"
        };

        options.ParamVerify();

        options.Schema.Should().Be("config");
        options.TableName.Should().Be("system_config");
        options.FullTableName.Should().Be("config.system_config");
    }

    [Fact]
    public void ParamVerify_WithInvalidArguments_ThrowsMeaningfulExceptions()
    {
        var missingConnection = new DBConfigOptions
        {
            TableName = "config.system_config",
            ConfigKeyField = "code",
            ConfigValueField = "value"
        };

        var invalidTableName = new DBConfigOptions
        {
            CreateDbConnection = () => new FakeDbConnection(new FakeDatabaseState()),
            TableName = "a.b.c",
            ConfigKeyField = "code",
            ConfigValueField = "value"
        };

        var blankFields = new DBConfigOptions
        {
            CreateDbConnection = () => new FakeDbConnection(new FakeDatabaseState()),
            TableName = "system_config",
            ConfigKeyField = "",
            ConfigValueField = ""
        };

        missingConnection.Invoking(x => x.ParamVerify())
            .Should().Throw<ArgumentNullException>()
            .WithParameterName(nameof(DBConfigOptions.CreateDbConnection));

        invalidTableName.Invoking(x => x.ParamVerify())
            .Should().Throw<ArgumentException>()
            .WithParameterName(nameof(DBConfigOptions.TableName));

        blankFields.Invoking(x => x.ParamVerify())
            .Should().Throw<ArgumentException>()
            .WithParameterName(nameof(DBConfigOptions.ConfigKeyField));
    }

    [Fact]
    public void AddDbConfiguration_WithNullArguments_ThrowsArgumentNullException()
    {
        IConfigurationBuilder? builder = null;
        Action<DBConfigOptions>? action = null;

        var nullBuilderAct = () => DbConfigurationProviderExtensions.AddDbConfiguration(builder!, _ => { });
        var nullActionAct = () => new ConfigurationBuilder().AddDbConfiguration(action!);

        nullBuilderAct.Should().Throw<ArgumentNullException>()
            .WithParameterName("builder");
        nullActionAct.Should().Throw<ArgumentNullException>()
            .WithParameterName("action");
    }

    [Fact]
    public void DefaultScriptService_BuildsSchemaAwareInitScript()
    {
        var script = new DefaultScriptService()
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
        var options = new DBConfigOptions
        {
            CreateDbConnection = () => new FakeDbConnection(state),
            TableName = "config.custom_config",
            ConfigKeyField = "code",
            ConfigValueField = "value",
            ReloadOnChange = false,
            IsConsoleQueryLog = false
        };
        options.ParamVerify();

        using var provider = new DbConfigurationProvider(options, scriptService);

        state.ExecutedCommands.Should().Contain(scriptService.InitTableScript);
        state.ExecutedCommands.Should().Contain("select count(*) from config.custom_config");
        state.ExecutedCommands.Should().Contain(scriptService.InitDataScript);
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
}

internal sealed class FakeDbConnection(FakeDatabaseState state) : IDbConnection
{
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
        state.ExecutedCommands.Add(CommandText);
        return 1;
    }

    public IDataReader ExecuteReader()
    {
        state.ExecutedCommands.Add(CommandText);
        return CreateReader(state.Rows);
    }

    public IDataReader ExecuteReader(CommandBehavior behavior)
    {
        state.ExecutedCommands.Add(CommandText);
        return CreateReader(state.Rows);
    }

    public object ExecuteScalar()
    {
        state.ExecutedCommands.Add(CommandText);
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
    public object? this[string parameterName]
    {
        get => this.FirstOrDefault(x => x is IDataParameter parameter && parameter.ParameterName == parameterName);
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

    public string ParameterName { get; set; } = string.Empty;

    public string SourceColumn { get; set; } = string.Empty;

    public DataRowVersion SourceVersion { get; set; } = DataRowVersion.Current;

    public object? Value { get; set; }

    public byte Precision { get; set; }

    public byte Scale { get; set; }

    public int Size { get; set; }
}
