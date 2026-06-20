using System.Data;

namespace Azrng.DuckDB.Quack.Tests;

/// <summary>
/// Integration coverage for protocol-level behaviors that are 0% covered by unit tests:
/// catalog switching, multi-chunk result fetching, post-disconnect rejection, and
/// transaction isolation level propagation.
/// </summary>
[Trait("Category", "Integration")]
[Collection(IntegrationTestCollection.Name)]
public sealed class ProtocolFeatureIntegrationTests
{
    private readonly TestOptions _options;

    public ProtocolFeatureIntegrationTests(TestOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// A connection string with Catalog set must auto-switch to that catalog on connect,
    /// so an unqualified table reference resolves inside it.
    /// </summary>
    [Fact]
    public async Task Catalog_ConnectionString_AutoSwitchesCatalog()
    {
        // Strip any existing Catalog and force memory — the default in-memory catalog.
        var baseCs = _options.ConnectionString.Split(';')
            .Where(p => !p.Trim().StartsWith("Catalog", StringComparison.OrdinalIgnoreCase));
        var catalogCs = string.Join(';', baseCs.Append("Catalog=memory"));

        await using var connection = new QuackConnection(catalogCs);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        // current_catalog should be "memory" after the implicit USE.
        command.CommandText = "SELECT current_catalog()";
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal("memory", reader.GetString(0));
    }

    /// <summary>
    /// A result set larger than DuckDB's vector size forces multiple FetchAsync round-trips.
    /// The reader must stitch them together and surface every row.
    /// </summary>
    [Fact]
    public async Task LargeResultSet_FetchesAcrossMultipleChunks()
    {
        await using var connection = new QuackConnection(_options.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        // 5000 rows > STANDARD_VECTOR_SIZE (2048), so the server must split into chunks
        // and the client must follow up with Fetch requests for the remaining rows.
        command.CommandText = "SELECT i FROM range(0, 5000) t(i) ORDER BY i";

        await using var reader = await command.ExecuteReaderAsync();
        long expected = 0;
        var lastSeen = -1L;
        while (await reader.ReadAsync())
        {
            var value = reader.GetInt64(0);
            Assert.Equal(expected, value);
            lastSeen = value;
            expected++;
        }

        Assert.Equal(5000, expected);
        Assert.Equal(4999, lastSeen);
    }

    /// <summary>
    /// After Dispose, the underlying connection is torn down; any subsequent command on it
    /// (or the server's view of the session) must reject rather than silently succeed.
    /// </summary>
    [Fact]
    public async Task Dispose_TearsDownConnectionAndRejectsReuse()
    {
        var connection = new QuackConnection(_options.ConnectionString);
        await connection.OpenAsync();

        await using var warmup = connection.CreateCommand();
        warmup.CommandText = "SELECT 1";
        await warmup.ExecuteReaderAsync();

        await connection.DisposeAsync();

        // Any attempt to use the torn-down connection — even creating a command — must fail
        // rather than silently proceed against a dead session.
        await Assert.ThrowsAnyAsync<Exception>(() =>
        {
            var afterDispose = connection.CreateCommand();
            afterDispose.CommandText = "SELECT 1";
            return afterDispose.ExecuteReaderAsync();
        });
    }

    /// <summary>
    /// BeginTransaction with an explicit IsolationLevel must propagate it onto the transaction
    /// object, and a committed transaction's writes must be visible to a fresh reader.
    /// </summary>
    [Fact]
    public async Task Transaction_CommitMakesWritesVisibleAndReportsIsolation()
    {
        await using var connection = new QuackConnection(_options.ConnectionString);
        await connection.OpenAsync();

        var tableName = $"iso_{Guid.NewGuid():N}".Substring(0, 30);
        await using (var create = connection.CreateCommand())
        {
            create.CommandText = $"CREATE TABLE {tableName} (id INTEGER)";
            await create.ExecuteNonQueryAsync();
        }

        try
        {
            await using (var tx = await connection.BeginTransactionAsync(IsolationLevel.Serializable))
            {
                Assert.Equal(IsolationLevel.Serializable, tx.IsolationLevel);

                await using var insert = connection.CreateCommand();
                insert.CommandText = $"INSERT INTO {tableName} VALUES (1)";
                await insert.ExecuteNonQueryAsync();

                await tx.CommitAsync();
            }

            await using var read = connection.CreateCommand();
            read.CommandText = $"SELECT COUNT(*) FROM {tableName}";
            await using var reader = await read.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.Equal(1L, Convert.ToInt64(reader.GetValue(0)));
        }
        finally
        {
            await using var cleanup = connection.CreateCommand();
            cleanup.CommandText = $"DROP TABLE IF EXISTS {tableName}";
            await cleanup.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// A rolled-back transaction must leave the table untouched — the canonical rollback contract.
    /// </summary>
    [Fact]
    public async Task Transaction_RollbackDiscardsWrites()
    {
        await using var connection = new QuackConnection(_options.ConnectionString);
        await connection.OpenAsync();

        var tableName = $"rb_{Guid.NewGuid():N}".Substring(0, 30);
        await using (var create = connection.CreateCommand())
        {
            create.CommandText = $"CREATE TABLE {tableName} (id INTEGER)";
            await create.ExecuteNonQueryAsync();
        }

        try
        {
            await using (var tx = await connection.BeginTransactionAsync())
            {
                Assert.Equal(IsolationLevel.ReadCommitted, tx.IsolationLevel);

                await using var insert = connection.CreateCommand();
                insert.CommandText = $"INSERT INTO {tableName} VALUES (1)";
                await insert.ExecuteNonQueryAsync();

                await tx.RollbackAsync();
            }

            await using var read = connection.CreateCommand();
            read.CommandText = $"SELECT COUNT(*) FROM {tableName}";
            await using var reader = await read.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.Equal(0L, Convert.ToInt64(reader.GetValue(0)));
        }
        finally
        {
            await using var cleanup = connection.CreateCommand();
            cleanup.CommandText = $"DROP TABLE IF EXISTS {tableName}";
            await cleanup.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// Calling BeginTransaction twice on the same connection without disposing the first
    /// must surface a clear error rather than silently nest.
    /// </summary>
    [Fact]
    public async Task Transaction_BeginTwice_Throws()
    {
        await using var connection = new QuackConnection(_options.ConnectionString);
        await connection.OpenAsync();

        await using var tx = await connection.BeginTransactionAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => connection.BeginTransactionAsync());
    }
}
