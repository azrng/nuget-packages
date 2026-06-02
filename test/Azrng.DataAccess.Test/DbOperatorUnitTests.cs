using Azrng.Core.Model;
using Azrng.DataAccess;
using Azrng.DataAccess.DbBridge;
using Azrng.DataAccess.Helper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;

namespace Azrng.DataAccess.Test;

public class DbOperatorUnitTests
{
    private static readonly string[] RequiredSqlKeys =
    {
        "SchemaName",
        "SchemaInfo",
        "SchemaTableName",
        "SchemaTable",
        "TableInfo",
        "TableColumn",
        "SchemaColumn",
        "TablePrimary",
        "SchemaPrimary",
        "TableForeign",
        "SchemaForeign",
        "TableIndex",
        "SchemaIndex"
    };

    [Fact]
    public void Factory_ShouldCreateExpectedBridgeTypes_FromConnectionString()
    {
        var cases = new (DatabaseType Type, Type BridgeType, Type HelperType, string ConnectionString)[]
        {
            (DatabaseType.MySql, typeof(MySqlBasicDbBridge), typeof(MySqlDbHelper), "Server=localhost;Database=test;User Id=root;Password=pwd;"),
            (DatabaseType.SqlServer, typeof(SqlServerBasicDbBridge), typeof(SqlServerDbHelper), "Server=localhost,1433;Database=test;Uid=sa;Pwd=pwd;Encrypt=no;"),
            (DatabaseType.Oracle, typeof(OracleBasicDbBridge), typeof(OracleDbHelper), "Data Source=localhost;User Id=user;Password=pwd;"),
            (DatabaseType.PostgresSql, typeof(PostgreBasicDbBridge), typeof(PostgresSqlDbHelper), "Host=localhost;Port=5432;Database=test;Username=postgres;Password=pwd;"),
            (DatabaseType.Sqlite, typeof(SqliteBasicDbBridge), typeof(SqliteDbHelper), "Data Source=:memory:;"),
            (DatabaseType.ClickHouse, typeof(ClickHouseBasicDbBridge), typeof(ClickHouseDbHelper), "Host=localhost;Port=8123;Database=default;User=default;Password=;")
        };

        foreach (var testCase in cases)
        {
            var bridge = DbBridgeFactory.CreateDbBridge(testCase.Type, testCase.ConnectionString);

            Assert.IsType(testCase.BridgeType, bridge);
            Assert.IsType(testCase.HelperType, bridge.DbHelper);
            Assert.Equal(testCase.ConnectionString, bridge.DbHelper.ConnectionString);
        }
    }

    [Fact]
    public void Factory_ShouldCreateExpectedBridgeTypes_FromConfig()
    {
        var cases = new (DatabaseType Type, Type BridgeType)[]
        {
            (DatabaseType.MySql, typeof(MySqlBasicDbBridge)),
            (DatabaseType.SqlServer, typeof(SqlServerBasicDbBridge)),
            (DatabaseType.Oracle, typeof(OracleBasicDbBridge)),
            (DatabaseType.PostgresSql, typeof(PostgreBasicDbBridge)),
            (DatabaseType.Sqlite, typeof(SqliteBasicDbBridge)),
            (DatabaseType.ClickHouse, typeof(ClickHouseBasicDbBridge))
        };

        foreach (var testCase in cases)
        {
            var config = CreateConfig(testCase.Type);
            var bridge = DbBridgeFactory.CreateDbBridge(config);

            Assert.IsType(testCase.BridgeType, bridge);
        }
    }

    [Fact]
    public void Bridges_ShouldContainRequiredQuerySqlKeys()
    {
        var bridges = new BasicDbBridge[]
        {
            new MySqlBasicDbBridge("Server=localhost;Database=test;User Id=root;Password=pwd;"),
            new SqlServerBasicDbBridge("Server=localhost,1433;Database=test;Uid=sa;Pwd=pwd;Encrypt=no;"),
            new OracleBasicDbBridge("Data Source=localhost;User Id=user;Password=pwd;"),
            new PostgreBasicDbBridge("Host=localhost;Port=5432;Database=test;Username=postgres;Password=pwd;"),
            new SqliteBasicDbBridge("Data Source=:memory:;"),
            new ClickHouseBasicDbBridge("Host=localhost;Port=8123;Database=default;User=default;Password=;")
        };

        foreach (var bridge in bridges)
        {
            foreach (var key in RequiredSqlKeys)
            {
                Assert.True(
                    bridge.QuerySqlMap.TryGetValue(key, out var sql) && !string.IsNullOrWhiteSpace(sql),
                    $"{bridge.GetType().Name} is missing required SQL key '{key}'.");
            }
        }
    }

    [Fact]
    public void PaginationBuilders_ShouldRejectUnsafeOrderColumn()
    {
        foreach (var helper in CreateHelpersForPaging())
        {
            Assert.Throws<ArgumentException>(() =>
                helper.BuildSplitPageSql("select * from users", 1, 10, "id;drop table users", "DESC"));
        }
    }

    [Fact]
    public void PaginationBuilders_ShouldRejectUnsafeOrderDirection()
    {
        foreach (var helper in CreateHelpersForPaging())
        {
            Assert.Throws<ArgumentException>(() =>
                helper.BuildSplitPageSql("select * from users", 1, 10, "id", "DESC;DROP"));
        }
    }

    [Fact]
    public void PaginationBuilders_ShouldRejectNonPositivePageArguments()
    {
        foreach (var helper in CreateHelpersForPaging())
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                helper.BuildSplitPageSql("select * from users", 0, 10, "id", "DESC"));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                helper.BuildSplitPageSql("select * from users", 1, 0, "id", "DESC"));
        }
    }

    [Fact]
    public void MySqlHelper_ShouldRespectPoolingConfiguration()
    {
        var helper = new MySqlDbHelper(new DataSourceConfig
        {
            Host = "localhost",
            Port = 3306,
            DbName = "sample",
            User = "root",
            Password = "pwd",
            Pooling = true,
            MinPoolSize = 3,
            MaxPoolSize = 9
        });

        var builder = new MySqlConnectionStringBuilder(helper.ConnectionString);
        Assert.True(builder.Pooling);
        Assert.Equal((uint)3, builder.MinimumPoolSize);
        Assert.Equal((uint)9, builder.MaximumPoolSize);
    }

    [Fact]
    public void PostgresHelper_ShouldRespectPoolingConfiguration()
    {
        var helper = new PostgresSqlDbHelper(new DataSourceConfig
        {
            Host = "localhost",
            Port = 5432,
            DbName = "sample",
            User = "postgres",
            Password = "pwd",
            Pooling = true,
            MinPoolSize = 4,
            MaxPoolSize = 11
        });

        var builder = new NpgsqlConnectionStringBuilder(helper.ConnectionString);
        Assert.True(builder.Pooling);
        Assert.Equal(4, builder.MinPoolSize);
        Assert.Equal(11, builder.MaxPoolSize);
        Assert.False(builder.PersistSecurityInfo);
    }

    [Fact]
    public void DataSourceConnectionStringBuilder_ShouldBuildProviderConnectionStrings()
    {
        var mysql = new MySqlConnectionStringBuilder(
            DataSourceConnectionStringBuilder.Build(CreateConfig(DatabaseType.MySql)));
        Assert.Equal("localhost", mysql.Server);
        Assert.Equal("sample", mysql.Database);
        Assert.Equal("pwd", mysql.Password);

        var sqlServer = new SqlConnectionStringBuilder(
            DataSourceConnectionStringBuilder.Build(CreateConfig(DatabaseType.SqlServer)));
        Assert.Equal("localhost,1433", sqlServer.DataSource);
        Assert.Equal("sample", sqlServer.InitialCatalog);
        Assert.Equal("pwd", sqlServer.Password);
        Assert.False(sqlServer.Encrypt);

        var oracle = new OracleConnectionStringBuilder(
            DataSourceConnectionStringBuilder.Build(CreateConfig(DatabaseType.Oracle)));
        Assert.Equal("user", oracle.UserID);
        Assert.Equal("pwd", oracle.Password);

        var postgres = new NpgsqlConnectionStringBuilder(
            DataSourceConnectionStringBuilder.Build(CreateConfig(DatabaseType.PostgresSql)));
        Assert.Equal("localhost", postgres.Host);
        Assert.Equal("sample", postgres.Database);
        Assert.Equal("pwd", postgres.Password);

        var sqlite = new SqliteConnectionStringBuilder(
            DataSourceConnectionStringBuilder.Build(CreateConfig(DatabaseType.Sqlite)));
        Assert.Equal(":memory:", sqlite.DataSource);
    }

    [Fact]
    public void MaskConnectionString_ShouldRedactSensitiveKeys()
    {
        var connectionString =
            "Server=localhost;Database=sample;User Id=sa;Username=postgres;User=click;Password=secret;Pwd=secret2;";

        var masked = DataSourceConnectionStringBuilder.MaskConnectionString(connectionString);

        var maskedBuilder = new DbConnectionStringBuilder { ConnectionString = masked };
        Assert.Equal("***", maskedBuilder["User Id"]);
        Assert.Equal("***", maskedBuilder["Username"]);
        Assert.Equal("***", maskedBuilder["User"]);
        Assert.Equal("***", maskedBuilder["Password"]);
        Assert.Equal("***", maskedBuilder["Pwd"]);
        Assert.DoesNotContain("secret", masked);
        Assert.DoesNotContain("secret2", masked);
        Assert.Contains("***", masked);
    }

    [Fact]
    public void DbHelper_ShouldExposeMaskedConnectionString()
    {
        var helper = new SqlServerDbHelper(new DataSourceConfig
        {
            Type = DatabaseType.SqlServer,
            Host = "localhost",
            Port = 1433,
            DbName = "sample",
            User = "sa",
            Password = "secret"
        });

        Assert.Contains("Password=secret", helper.ConnectionString);
        IDbHelper dbHelper = helper;
        var maskedConnectionString = dbHelper.GetMaskedConnectionString();
        var maskedBuilder = new DbConnectionStringBuilder { ConnectionString = maskedConnectionString };
        Assert.Equal("***", maskedBuilder["User ID"]);
        Assert.Equal("***", maskedBuilder["Password"]);
        Assert.DoesNotContain("secret", helper.MaskedConnectionString);
        Assert.DoesNotContain("secret", maskedConnectionString);
    }

    [Fact]
    public async Task OptionalUnsupportedOperations_ShouldThrowNotSupportedException()
    {
        var bridge = new UnsupportedOptionalBridge();

        await Assert.ThrowsAsync<NotSupportedException>(() => bridge.GetViewListAsync());
        await Assert.ThrowsAsync<NotSupportedException>(() => bridge.GetSchemaViewListAsync("main"));
        await Assert.ThrowsAsync<NotSupportedException>(() => bridge.GetProcListAsync());
        await Assert.ThrowsAsync<NotSupportedException>(() => bridge.GetSchemaProcListAsync("main"));
    }

    private static IEnumerable<IDbHelper> CreateHelpersForPaging()
    {
        yield return new MySqlDbHelper("Server=localhost;Database=test;User Id=root;Password=pwd;");
        yield return new SqlServerDbHelper("Server=localhost,1433;Database=test;Uid=sa;Pwd=pwd;Encrypt=no;");
        yield return new OracleDbHelper("Data Source=localhost;User Id=user;Password=pwd;");
        yield return new PostgresSqlDbHelper("Host=localhost;Port=5432;Database=test;Username=postgres;Password=pwd;");
        yield return new SqliteDbHelper("Data Source=:memory:;");
    }

    private static DataSourceConfig CreateConfig(DatabaseType type)
    {
        return new DataSourceConfig
        {
            Type = type,
            Host = "localhost",
            Port = type switch
            {
                DatabaseType.MySql => 3306,
                DatabaseType.SqlServer => 1433,
                DatabaseType.Oracle => 1521,
                DatabaseType.PostgresSql => 5432,
                DatabaseType.ClickHouse => 8123,
                _ => 0
            },
            DbName = type == DatabaseType.Sqlite ? ":memory:" : "sample",
            User = "user",
            Password = "pwd"
        };
    }

    private sealed class UnsupportedOptionalBridge : BasicDbBridge
    {
        public UnsupportedOptionalBridge() : base("Data Source=:memory:;") { }

        public override Dictionary<string, string> QuerySqlMap =>
            RequiredSqlKeys.ToDictionary(key => key, _ => "select 1");

        public override DatabaseType DatabaseType => DatabaseType.Sqlite;

        public override IDbHelper DbHelper { get; } = new ThrowingDbHelper();
    }

    private sealed class ThrowingDbHelper : DbHelperBase
    {
        public ThrowingDbHelper() : base("Data Source=:memory:;") { }

        protected override DbConnection GetConnection() => throw new NotSupportedException();

        public override Task<object[][]> QueryArrayAsync(string sql, object? parameters = null, bool header = true) =>
            throw new NotSupportedException();

        public override DbParameter SetParameter(string key, object value) => throw new NotSupportedException();
    }
}
