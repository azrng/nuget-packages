using Azrng.SqlMigration;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Data;
using System.Data.Common;
using Xunit;

namespace Azrng.SqlMigration.Test;

public class SqlMigrationTests : IDisposable
{
    public SqlMigrationTests()
    {
        SqlMigrationServiceExtension.DbNames.Clear();
    }

    public void Dispose()
    {
        SqlMigrationServiceExtension.DbNames.Clear();
    }

    [Fact]
    public void AddSqlMigrationService_ThrowsForBlankAndDuplicateMigrationNames()
    {
        var services = new ServiceCollection();

        var blankAct = () => services.AddSqlMigrationService(" ", options =>
        {
            options.ConnectionBuilder = _ => new FakeDbConnection(new FakeDbState());
        });

        blankAct.Should().Throw<ArgumentException>();

        services.AddSqlMigrationService("main", options =>
        {
            options.ConnectionBuilder = _ => new FakeDbConnection(new FakeDbState());
        });

        var duplicateAct = () => services.AddSqlMigrationService("main", options =>
        {
            options.ConnectionBuilder = _ => new FakeDbConnection(new FakeDbState());
        });

        duplicateAct.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddAutoMigration_WithoutMigrationRegistrations_ThrowsArgumentException()
    {
        var services = new ServiceCollection();

        var act = () => services.AddAutoMigration();

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddSqlMigrationService_RegistersNamedOptionsHandlerAndInitVersionSetter()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddSqlMigrationService<TestMigrationHandler>("main", options =>
        {
            options.ConnectionBuilder = _ => new FakeDbConnection(new FakeDbState());
            options.SqlRootPath = "sql";
            options.VersionPrefix = "version";
            options.Schema = "custom";
            options.SetInitVersionSetter<TestInitVersionSetter>();
            options.VersionLog.TableName = "migration_log";
        }).AddAutoMigration();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsSnapshot<SqlMigrationOption>>().Get("main");

        provider.GetRequiredKeyedService<IMigrationHandler>("main").Should().BeOfType<TestMigrationHandler>();
        provider.GetRequiredKeyedService<IInitVersionSetter>("main").Should().BeOfType<TestInitVersionSetter>();
        provider.GetServices<IStartupFilter>().Should().ContainSingle(x => x is SqlMigrationStartupFilter);
        options.Schema.Should().Be("custom");
        options.VersionPrefix.Should().Be("version");
        options.SqlRootPath.Should().Be("sql");
        options.InitVersionSetterType.Should().Be(typeof(TestInitVersionSetter));
        options.VersionLog.TableName.Should().Be("migration_log");
    }

    [Fact]
    public void SqlVersionLogOption_OrderByColumn_FallsBackToIdColumn()
    {
        var option = new SqlVersionLogOption
        {
            IdColumn = "log_id"
        };

        option.OrderByColumn.Should().Be("log_id");

        option.OrderByColumn = "custom_order";

        option.OrderByColumn.Should().Be("custom_order");
    }

    [Fact]
    public async Task SqlMigrationStartupFilter_RunsNewerScriptsInOrderAndUsesLockProvider()
    {
        var state = new FakeDbState
        {
            CurrentVersion = "1.0.0",
            VersionTableExists = true
        };
        var lockTracker = new LockTracker();
        var handler = new TestMigrationHandler();
        var sqlRoot = CreateSqlDirectory(
            ("version0.9.9.sql", "select 'skip older';"),
            ("version1.0.1.sql", "select 'apply 101';"),
            ("version1.0.2.txt", "select 'apply 102';"),
            ("ignored.md", "skip"));

        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddKeyedSingleton<IMigrationHandler>("main", handler);

            services.AddSqlMigrationService("main", options =>
            {
                options.ConnectionBuilder = _ => new FakeDbConnection(state);
                options.SqlRootPath = sqlRoot;
                options.VersionPrefix = "version";
                options.LockProvider = _ => Task.FromResult<IAsyncDisposable?>(new TrackableAsyncDisposable(lockTracker));
            }).AddAutoMigration();

            using var provider = services.BuildServiceProvider();
            var filter = provider.GetRequiredService<IStartupFilter>();
            var appBuilder = new ApplicationBuilder(provider);
            var nextCalled = false;

            filter.Configure(_ => { nextCalled = true; })(appBuilder);

            nextCalled.Should().BeTrue();
            lockTracker.EnterCount.Should().Be(1);
            lockTracker.DisposeCount.Should().Be(1);
            state.ExecutedCommands.Should().ContainInOrder("select 'apply 101';", "select 'apply 102';");
            state.InsertedVersions.Should().Equal("1.0.1", "1.0.2");
            state.CommitCount.Should().Be(2);
            state.RollbackCount.Should().Be(0);
            handler.Events.Should().ContainInOrder(
                "before:1.0.0",
                "version-before:1.0.1",
                "version-success:1.0.1",
                "version-before:1.0.2",
                "version-success:1.0.2",
                "success:1.0.0->1.0.2");
        }
        finally
        {
            Directory.Delete(sqlRoot, recursive: true);
        }
    }

    [Fact]
    public async Task PgSqlDbVersionService_ValidatesCustomOrderByColumnWithoutInitSql()
    {
        var state = new FakeDbState();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IOptionsSnapshot<SqlMigrationOption>>(new NamedOptionsSnapshot<SqlMigrationOption>(
            new Dictionary<string, SqlMigrationOption>
            {
                ["main"] = new()
                {
                    ConnectionBuilder = _ => new FakeDbConnection(state),
                    Schema = "public",
                    VersionLog =
                    {
                        TableName = "app_version_log",
                        IdColumn = "id",
                        VersionColumn = "version",
                        OrderByColumn = "sort_no"
                    }
                }
            }));
        services.AddKeyedSingleton<IDbConnection>("main", new FakeDbConnection(state));

        await using var provider = services.BuildServiceProvider();
        var service = new Azrng.SqlMigration.Service.PgSqlDbVersionService(
            provider,
            NullLogger<Azrng.SqlMigration.Service.PgSqlDbVersionService>.Instance);

        var act = () => service.GetCurrentVersionAsync("main");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SqlMigrationStartupFilter_WhenScriptFails_RollsBackAndInvokesFailureCallbacks()
    {
        var state = new FakeDbState
        {
            CurrentVersion = "1.0.0",
            VersionTableExists = true,
            ThrowOnCommand = sql => sql.Contains("broken", StringComparison.OrdinalIgnoreCase)
        };
        var handler = new TestMigrationHandler();
        var sqlRoot = CreateSqlDirectory(("version1.0.1.sql", "select 'broken';"));

        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddKeyedSingleton<IMigrationHandler>("main", handler);

            services.AddSqlMigrationService("main", options =>
            {
                options.ConnectionBuilder = _ => new FakeDbConnection(state);
                options.SqlRootPath = sqlRoot;
                options.VersionPrefix = "version";
            }).AddAutoMigration();

            using var provider = services.BuildServiceProvider();
            var filter = provider.GetRequiredService<IStartupFilter>();
            var appBuilder = new ApplicationBuilder(provider);

            var act = () => Task.Run(() => filter.Configure(_ => { })(appBuilder));

            await act.Should().ThrowAsync<InvalidOperationException>();
            state.CommitCount.Should().Be(0);
            state.RollbackCount.Should().Be(1);
            handler.Events.Should().Contain("version-failed:1.0.1");
            handler.Events.Should().Contain("failed:1.0.0->1.0.0");
        }
        finally
        {
            Directory.Delete(sqlRoot, recursive: true);
        }
    }

    private static string CreateSqlDirectory(params (string FileName, string Content)[] files)
    {
        var root = Path.Combine(Path.GetTempPath(), "Azrng.SqlMigration.Test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        foreach (var (fileName, content) in files)
        {
            File.WriteAllText(Path.Combine(root, fileName), content);
        }

        return root;
    }
}

public sealed class TestInitVersionSetter : IInitVersionSetter
{
    public Task<string> GetCurrentVersionAsync()
    {
        return Task.FromResult("1.0.0");
    }
}

public sealed class TestMigrationHandler : IMigrationHandler
{
    public List<string> Events { get; } = [];

    public Task<bool> BeforeMigrateAsync(string oloVersion)
    {
        Events.Add($"before:{oloVersion}");
        return Task.FromResult(true);
    }

    public Task<bool> VersionUpdateBeforeMigrateAsync(string version)
    {
        Events.Add($"version-before:{version}");
        return Task.FromResult(true);
    }

    public Task VersionUpdateMigratedAsync(string version)
    {
        Events.Add($"version-success:{version}");
        return Task.CompletedTask;
    }

    public Task VersionUpdateMigrateFailedAsync(string version)
    {
        Events.Add($"version-failed:{version}");
        return Task.CompletedTask;
    }

    public Task MigratedAsync(string oldVersion, string version)
    {
        Events.Add($"success:{oldVersion}->{version}");
        return Task.CompletedTask;
    }

    public Task MigrateFailedAsync(string oldVersion, string version)
    {
        Events.Add($"failed:{oldVersion}->{version}");
        return Task.CompletedTask;
    }
}

internal sealed class NamedOptionsSnapshot<TOptions>(IReadOnlyDictionary<string, TOptions> options) : IOptionsSnapshot<TOptions>
    where TOptions : class
{
    public TOptions Value => Get(Options.DefaultName);

    public TOptions Get(string? name)
    {
        var key = name ?? Options.DefaultName;
        if (options.TryGetValue(key, out var value))
        {
            return value;
        }

        throw new InvalidOperationException($"Missing named options: {key}");
    }
}

internal sealed class LockTracker
{
    public int EnterCount { get; set; }

    public int DisposeCount { get; set; }
}

internal sealed class TrackableAsyncDisposable : IAsyncDisposable
{
    private readonly LockTracker _tracker;

    public TrackableAsyncDisposable(LockTracker tracker)
    {
        _tracker = tracker;
        _tracker.EnterCount++;
    }

    public ValueTask DisposeAsync()
    {
        _tracker.DisposeCount++;
        return ValueTask.CompletedTask;
    }
}

internal sealed class FakeDbState
{
    public string CurrentVersion { get; set; } = "0.0.0";

    public bool VersionTableExists { get; set; }

    public bool HasLegacyCreatedTime { get; set; }

    public List<string> ExecutedCommands { get; } = [];

    public List<string> InsertedVersions { get; } = [];

    public int CommitCount { get; set; }

    public int RollbackCount { get; set; }

    public Func<string, bool>? ThrowOnCommand { get; set; }
}

internal sealed class FakeDbConnection(FakeDbState state) : DbConnection
{
    private ConnectionState _state = ConnectionState.Closed;
    private string _connectionString = string.Empty;

    public override string ConnectionString
    {
        get => _connectionString;
        set => _connectionString = value ?? string.Empty;
    }

    public override string Database => "Fake";

    public override string DataSource => "Fake";

    public override string ServerVersion => "1.0";

    public override ConnectionState State => _state;

    public override void ChangeDatabase(string databaseName)
    {
    }

    public override void Close()
    {
        _state = ConnectionState.Closed;
    }

    public override void Open()
    {
        _state = ConnectionState.Open;
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        return new FakeDbTransaction(this, state);
    }

    protected override DbCommand CreateDbCommand()
    {
        return new FakeDbCommand(this, state);
    }
}

internal sealed class FakeDbTransaction(FakeDbConnection connection, FakeDbState state) : DbTransaction
{
    public override IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;

    protected override DbConnection DbConnection => connection;

    public override void Commit()
    {
        state.CommitCount++;
    }

    public override void Rollback()
    {
        state.RollbackCount++;
    }
}

internal sealed class FakeDbCommand(FakeDbConnection connection, FakeDbState state) : DbCommand
{
    private readonly FakeDbParameterCollection _parameters = new();
    private string _commandText = string.Empty;

    public override string CommandText
    {
        get => _commandText;
        set => _commandText = value ?? string.Empty;
    }

    public override int CommandTimeout { get; set; }

    public override CommandType CommandType { get; set; } = CommandType.Text;

    protected override DbConnection DbConnection { get; set; } = connection;

    protected override DbParameterCollection DbParameterCollection => _parameters;

    protected override DbTransaction? DbTransaction { get; set; }

    public override bool DesignTimeVisible { get; set; }

    public override UpdateRowSource UpdatedRowSource { get; set; }

    public override void Cancel()
    {
    }

    public override int ExecuteNonQuery()
    {
        TrackCommand();
        return 1;
    }

    public override object? ExecuteScalar()
    {
        TrackCommand();

        if (CommandText.Contains("FROM pg_class", StringComparison.OrdinalIgnoreCase))
        {
            return state.VersionTableExists ? 1 : 0;
        }

        if (CommandText.Contains("information_schema.columns", StringComparison.OrdinalIgnoreCase))
        {
            return state.HasLegacyCreatedTime ? 1 : 0;
        }

        if (CommandText.Contains("order by", StringComparison.OrdinalIgnoreCase))
        {
            return state.CurrentVersion;
        }

        return null;
    }

    public override void Prepare()
    {
    }

    protected override DbParameter CreateDbParameter()
    {
        return new FakeDbParameter();
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        TrackCommand();
        return new EmptyDbDataReader();
    }

    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(ExecuteNonQuery());
    }

    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(ExecuteScalar());
    }

    protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
    {
        return Task.FromResult<DbDataReader>(new EmptyDbDataReader());
    }

    private void TrackCommand()
    {
        state.ExecutedCommands.Add(CommandText);

        if (state.ThrowOnCommand?.Invoke(CommandText) == true)
        {
            throw new InvalidOperationException("Command execution failed");
        }

        if (CommandText.Contains("insert into", StringComparison.OrdinalIgnoreCase))
        {
            var version = _parameters
                .OfType<FakeDbParameter>()
                .FirstOrDefault(x => x.ParameterName.Contains("version", StringComparison.OrdinalIgnoreCase))
                ?.Value?.ToString();

            if (!string.IsNullOrWhiteSpace(version))
            {
                state.InsertedVersions.Add(version);
            }
        }
    }
}

internal sealed class FakeDbParameterCollection : DbParameterCollection
{
    private readonly List<DbParameter> _items = [];

    public override int Count => _items.Count;

    public override object SyncRoot => ((ICollection)_items).SyncRoot!;

    public override int Add(object value)
    {
        _items.Add((DbParameter)value);
        return _items.Count - 1;
    }

    public override void AddRange(Array values)
    {
        foreach (var value in values)
        {
            Add(value!);
        }
    }

    public override void Clear()
    {
        _items.Clear();
    }

    public override bool Contains(object value)
    {
        return _items.Contains((DbParameter)value);
    }

    public override bool Contains(string value)
    {
        return _items.Any(x => x.ParameterName == value);
    }

    public override void CopyTo(Array array, int index)
    {
        _items.ToArray().CopyTo(array, index);
    }

    public override IEnumerator GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    public override int IndexOf(object value)
    {
        return _items.IndexOf((DbParameter)value);
    }

    public override int IndexOf(string parameterName)
    {
        return _items.FindIndex(x => x.ParameterName == parameterName);
    }

    public override void Insert(int index, object value)
    {
        _items.Insert(index, (DbParameter)value);
    }

    public override void Remove(object value)
    {
        _items.Remove((DbParameter)value);
    }

    public override void RemoveAt(int index)
    {
        _items.RemoveAt(index);
    }

    public override void RemoveAt(string parameterName)
    {
        var index = IndexOf(parameterName);
        if (index >= 0)
        {
            RemoveAt(index);
        }
    }

    protected override DbParameter GetParameter(int index)
    {
        return _items[index];
    }

    protected override DbParameter GetParameter(string parameterName)
    {
        return _items[IndexOf(parameterName)];
    }

    protected override void SetParameter(int index, DbParameter value)
    {
        _items[index] = value;
    }

    protected override void SetParameter(string parameterName, DbParameter value)
    {
        var index = IndexOf(parameterName);
        if (index >= 0)
        {
            _items[index] = value;
        }
        else
        {
            _items.Add(value);
        }
    }
}

internal sealed class FakeDbParameter : DbParameter
{
    private string _parameterName = string.Empty;
    private string _sourceColumn = string.Empty;

    public override DbType DbType { get; set; }

    public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;

    public override bool IsNullable { get; set; }

    public override string ParameterName
    {
        get => _parameterName;
        set => _parameterName = value ?? string.Empty;
    }

    public override string SourceColumn
    {
        get => _sourceColumn;
        set => _sourceColumn = value ?? string.Empty;
    }

    public override object? Value { get; set; }

    public override bool SourceColumnNullMapping { get; set; }

    public override int Size { get; set; }

    public override void ResetDbType()
    {
    }
}

internal sealed class EmptyDbDataReader : DbDataReader
{
    public override int FieldCount => 0;

    public override bool HasRows => false;

    public override bool IsClosed => false;

    public override int RecordsAffected => 0;

    public override int Depth => 0;

    public override object this[int ordinal] => throw new IndexOutOfRangeException();

    public override object this[string name] => throw new IndexOutOfRangeException();

    public override bool GetBoolean(int ordinal) => throw new IndexOutOfRangeException();

    public override byte GetByte(int ordinal) => throw new IndexOutOfRangeException();

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => 0;

    public override char GetChar(int ordinal) => throw new IndexOutOfRangeException();

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => 0;

    public override string GetDataTypeName(int ordinal) => string.Empty;

    public override DateTime GetDateTime(int ordinal) => throw new IndexOutOfRangeException();

    public override decimal GetDecimal(int ordinal) => throw new IndexOutOfRangeException();

    public override double GetDouble(int ordinal) => throw new IndexOutOfRangeException();

    public override IEnumerator GetEnumerator()
    {
        yield break;
    }

    public override Type GetFieldType(int ordinal) => typeof(object);

    public override float GetFloat(int ordinal) => throw new IndexOutOfRangeException();

    public override Guid GetGuid(int ordinal) => throw new IndexOutOfRangeException();

    public override short GetInt16(int ordinal) => throw new IndexOutOfRangeException();

    public override int GetInt32(int ordinal) => throw new IndexOutOfRangeException();

    public override long GetInt64(int ordinal) => throw new IndexOutOfRangeException();

    public override string GetName(int ordinal) => string.Empty;

    public override int GetOrdinal(string name) => -1;

    public override string GetString(int ordinal) => throw new IndexOutOfRangeException();

    public override object GetValue(int ordinal) => throw new IndexOutOfRangeException();

    public override int GetValues(object[] values) => 0;

    public override bool IsDBNull(int ordinal) => true;

    public override bool NextResult() => false;

    public override bool Read() => false;
}
