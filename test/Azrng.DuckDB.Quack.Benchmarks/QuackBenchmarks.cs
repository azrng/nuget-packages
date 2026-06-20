using System.Data;
using BenchmarkDotNet.Attributes;

namespace Azrng.DuckDB.Quack.Benchmarks;

/// <summary>
/// Handshake round-trip: the full cost of establishing a Quack session (TCP + auth + protocol
/// Connect). This is the pure client-side overhead paid once per fresh connection — the part the
/// connection pool exists to amortise.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 5)]
public class ConnectionBench
{
    [Benchmark(Description = "Open+Dispose handshake")]
    public async Task HandshakeRoundTrip()
    {
        await using var connection = new QuackConnection(Program.ConnectionString);
        await connection.OpenAsync();
    }
}

/// <summary>
/// Per-query overhead on a warm (reused) connection: trivial scalar, parameterised scalar, and a
/// small aggregate. All three are dominated by the single request/response round-trip, so the
/// spread between them isolates parameter rendering vs raw execution.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 5)]
public class QueryBench : IAsyncDisposable
{
    private QuackConnection _connection = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        _connection = new QuackConnection(Program.ConnectionString);
        await _connection.OpenAsync();
    }

    [Benchmark(Description = "SELECT 1 (1 round-trip)")]
    public async Task ScalarSelect1()
    {
        await using var command = _connection.CreateCommand();
        command.CommandText = "SELECT 1";
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
    }

    [Benchmark(Description = "SELECT @a + @b (parameterised)")]
    public async Task ParameterizedSelect()
    {
        await using var command = _connection.CreateCommand();
        command.CommandText = "SELECT @a + @b";
        var a = command.CreateParameter(); a.ParameterName = "@a"; a.Value = 17L;
        var b = command.CreateParameter(); b.ParameterName = "@b"; b.Value = 25L;
        command.Parameters.Add(a); command.Parameters.Add(b);
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
    }

    [Benchmark(Description = "COUNT/SUM over 10k rows")]
    public async Task AggregateSelect()
    {
        await using var command = _connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*), SUM(i) FROM range(0, 10000) t(i)";
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}

/// <summary>
/// Insert throughput: batch VALUES insert (one statement for N rows) vs one-parameterised-statement
/// per row. The ratio shows how much the per-round-trip overhead dominates small writes.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 3)]
public class InsertBench : IAsyncDisposable
{
    private QuackConnection _connection = null!;
    private string _table = "bench_insert_" + Guid.NewGuid().ToString("N")[..12];

    [Params(100, 1000, 10000)]
    public int Rows { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        _connection = new QuackConnection(Program.ConnectionString);
        await _connection.OpenAsync();
        await using var create = _connection.CreateCommand();
        create.CommandText = $"CREATE TABLE {_table} (id INTEGER, label VARCHAR)";
        await create.ExecuteNonQueryAsync();
    }

    [IterationSetup]
    public void ClearRows()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = $"DELETE FROM {_table}";
        command.ExecuteNonQuery();
    }

    [Benchmark(Description = "Batch VALUES insert")]
    public async Task BatchInsert()
    {
        var rows = Enumerable.Range(0, Rows)
            .Select(i => new object?[] { i, $"row{i}" });
        await _connection.ExecuteBatchInsertAsync(_table, new[] { "id", "label" }, rows);
    }

    [Benchmark(Description = "Per-row parameterised insert")]
    public async Task PerRowInsert()
    {
        for (var i = 0; i < Rows; i++)
        {
            await using var command = _connection.CreateCommand();
            command.CommandText = $"INSERT INTO {_table} (id, label) VALUES (@id, @label)";
            var id = command.CreateParameter(); id.ParameterName = "@id"; id.Value = i;
            var label = command.CreateParameter(); label.ParameterName = "@label"; label.Value = $"row{i}";
            command.Parameters.Add(id); command.Parameters.Add(label);
            await command.ExecuteNonQueryAsync();
        }
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await using var drop = _connection.CreateCommand();
        drop.CommandText = $"DROP TABLE IF EXISTS {_table}";
        await drop.ExecuteNonQueryAsync();
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}

/// <summary>
/// Result-set read throughput across sizes that force multi-chunk fetching (DuckDB vector size is
/// 2048). Rows/sec and bytes/sec here reveal client-side decoding cost vs server streaming.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 3)]
public class ResultSetBench : IAsyncDisposable
{
    private QuackConnection _connection = null!;

    [Params(10000, 100000)]
    public int Rows { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        _connection = new QuackConnection(Program.ConnectionString);
        await _connection.OpenAsync();
    }

    [Benchmark(Description = "Read N rows (2 cols) end-to-end")]
    public async Task ReadAllRows()
    {
        await using var command = _connection.CreateCommand();
        command.CommandText = $"SELECT i, CAST(i AS VARCHAR) FROM range(0, {Rows}) t(i)";
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            _ = reader.GetInt64(0);
            _ = reader.GetString(1);
        }
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}

/// <summary>
/// Connection pool acquire/release cost — the steady-state overhead of a pooled query in a real
/// app, where every logical query borrows and returns a connection.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 5)]
public class PoolBench : IAsyncDisposable
{
    private QuackConnectionPool _pool = null!;

    [GlobalSetup]
    public void Setup()
        => _pool = new QuackConnectionPool(Program.ConnectionString, maxPoolSize: 16);

    [Benchmark(Description = "Pool acquire + return")]
    public async Task AcquireRelease()
    {
        var connection = await _pool.GetConnectionAsync();
        _pool.ReturnConnection(connection);
    }

    public ValueTask DisposeAsync()
    {
        _pool.Dispose();
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Concurrent query throughput on independent connections. BenchmarkDotNet reports mean wall-clock
/// for a batch of <see cref="Degree"/> parallel queries; throughput (queries/sec) = Degree / mean.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 3)]
public class ConcurrencyBench
{
    [Params(4, 16, 64)]
    public int Degree { get; set; }

    [Benchmark(Description = "Degree parallel scalar queries")]
    public async Task ParallelQueries()
    {
        var tasks = new Task[Degree];
        for (var i = 0; i < Degree; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                await using var connection = new QuackConnection(Program.ConnectionString);
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                await using var reader = await command.ExecuteReaderAsync();
                await reader.ReadAsync();
            });
        }
        await Task.WhenAll(tasks);
    }
}
