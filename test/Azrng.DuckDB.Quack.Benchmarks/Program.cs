using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace Azrng.DuckDB.Quack.Benchmarks;

/// <summary>
/// End-to-end performance benchmarks for the Azrng.DuckDB.Quack ADO.NET client against a live Quack
/// server. Results characterise client-side overhead on top of DuckDB engine performance.
///
/// Connection string comes from QUACK_PROTOCOL_CONNECTION_STRING (same env var as the test suite),
/// defaulting to the local container.
/// </summary>
internal static class Program
{
    public static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("QUACK_PROTOCOL_CONNECTION_STRING")
        ?? "Host=localhost;Port=9494;Token=E7231CE2CE78902BA280F3B9158BEB30;DisableSsl=true";

    private static void Main()
    {
        // Each benchmark class carries its own [SimpleJob(...)] for warmup/iteration counts; the
        // config only contributes exporters, ordering, and invariant culture. BenchmarkRunner.Run
        // is used directly (not the interactive BenchmarkSwitcher) so the suite runs headless.
        var config = ManualConfig.CreateMinimumViable()
            .AddLogger(BenchmarkDotNet.Loggers.ConsoleLogger.Default)
            .AddExporter(MarkdownExporter.GitHub)
            .AddExporter(JsonExporter.Default)
            .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical))
            .WithCultureInfo(System.Globalization.CultureInfo.InvariantCulture);

        Console.WriteLine($"Benchmark target: {ConnectionString}");

        BenchmarkRunner.Run(new[]
        {
            typeof(ConnectionBench),
            typeof(QueryBench),
            typeof(InsertBench),
            typeof(ResultSetBench),
            typeof(PoolBench),
            typeof(ConcurrencyBench),
        }, config);
    }
}
