using System.Diagnostics;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Util;
using Xunit.Abstractions;

namespace Azrng.JSqlParser.Test.Performance;

/// <summary>
/// 性能基准测试 — 记录 ANTLR4 原生解析性能
/// </summary>
public class PerformanceBenchmarkTest
{
    private const int WarmupIterations = 10;
    private const int MeasureIterations = 100;
    private readonly ITestOutputHelper _output;

    public PerformanceBenchmarkTest(ITestOutputHelper output)
    {
        _output = output;
    }

    private static readonly string[] SqlStatements =
    [
        // 简单 SELECT
        "SELECT id, name, email FROM users WHERE status = 'active'",
        // JOIN 查询
        "SELECT u.id, u.name, o.total FROM users u INNER JOIN orders o ON u.id = o.user_id WHERE o.amount > 100",
        // 子查询
        "SELECT id FROM users WHERE id IN (SELECT user_id FROM orders WHERE amount > 100)",
        // CTE
        "WITH active_users AS (SELECT id, name FROM users WHERE status = 'active') SELECT * FROM active_users",
        // UNION
        "SELECT id FROM users UNION SELECT id FROM admins UNION SELECT id FROM guests",
        // 复杂查询
        "SELECT u.id, u.name, COUNT(o.id) as order_count, SUM(o.amount) as total " +
        "FROM users u LEFT JOIN orders o ON u.id = o.user_id " +
        "WHERE u.status = 'active' AND u.created_at > '2024-01-01' " +
        "GROUP BY u.id, u.name HAVING COUNT(o.id) > 5 ORDER BY total DESC LIMIT 100",
        // INSERT
        "INSERT INTO users (id, name, email, status) VALUES (1, 'test', 'test@example.com', 'active')",
        // UPDATE
        "UPDATE users SET name = 'updated', status = 'inactive' WHERE id = 1",
        // DELETE
        "DELETE FROM users WHERE status = 'inactive' AND created_at < '2023-01-01'",
        // CREATE TABLE
        "CREATE TABLE orders (id INT PRIMARY KEY, user_id INT NOT NULL, amount DECIMAL(10,2), status VARCHAR(20))"
    ];

    [Fact]
    public void Benchmark_ParseSimpleSelect()
    {
        var sql = SqlStatements[0];
        var result = RunBenchmark("Simple SELECT", sql);
        OutputResult(result);
        Assert.True(result.AverageMs < 100, $"Simple SELECT too slow: {result.AverageMs:F2}ms");
    }

    [Fact]
    public void Benchmark_ParseJoinQuery()
    {
        var sql = SqlStatements[1];
        var result = RunBenchmark("JOIN Query", sql);
        OutputResult(result);
        Assert.True(result.AverageMs < 100, $"JOIN query too slow: {result.AverageMs:F2}ms");
    }

    [Fact]
    public void Benchmark_ParseSubquery()
    {
        var sql = SqlStatements[2];
        var result = RunBenchmark("Subquery", sql);
        OutputResult(result);
        Assert.True(result.AverageMs < 100, $"Subquery too slow: {result.AverageMs:F2}ms");
    }

    [Fact]
    public void Benchmark_ParseCte()
    {
        var sql = SqlStatements[3];
        var result = RunBenchmark("CTE", sql);
        OutputResult(result);
        Assert.True(result.AverageMs < 100, $"CTE too slow: {result.AverageMs:F2}ms");
    }

    [Fact]
    public void Benchmark_ParseUnion()
    {
        var sql = SqlStatements[4];
        var result = RunBenchmark("UNION", sql);
        OutputResult(result);
        Assert.True(result.AverageMs < 100, $"UNION too slow: {result.AverageMs:F2}ms");
    }

    [Fact]
    public void Benchmark_ParseComplexQuery()
    {
        var sql = SqlStatements[5];
        var result = RunBenchmark("Complex Query", sql);
        OutputResult(result);
        Assert.True(result.AverageMs < 200, $"Complex query too slow: {result.AverageMs:F2}ms");
    }

    [Fact]
    public void Benchmark_ParseInsert()
    {
        var sql = SqlStatements[6];
        var result = RunBenchmark("INSERT", sql);
        OutputResult(result);
        Assert.True(result.AverageMs < 100, $"INSERT too slow: {result.AverageMs:F2}ms");
    }

    [Fact]
    public void Benchmark_ParseUpdate()
    {
        var sql = SqlStatements[7];
        var result = RunBenchmark("UPDATE", sql);
        OutputResult(result);
        Assert.True(result.AverageMs < 100, $"UPDATE too slow: {result.AverageMs:F2}ms");
    }

    [Fact]
    public void Benchmark_ParseDelete()
    {
        var sql = SqlStatements[8];
        var result = RunBenchmark("DELETE", sql);
        OutputResult(result);
        Assert.True(result.AverageMs < 100, $"DELETE too slow: {result.AverageMs:F2}ms");
    }

    [Fact]
    public void Benchmark_ParseCreateTable()
    {
        var sql = SqlStatements[9];
        var result = RunBenchmark("CREATE TABLE", sql);
        OutputResult(result);
        Assert.True(result.AverageMs < 100, $"CREATE TABLE too slow: {result.AverageMs:F2}ms");
    }

    [Fact]
    public void Benchmark_TablesNamesFinder()
    {
        var sql = "SELECT u.id, o.total, p.name FROM users u " +
                  "INNER JOIN orders o ON u.id = o.user_id " +
                  "INNER JOIN products p ON o.product_id = p.id " +
                  "WHERE u.id IN (SELECT user_id FROM reviews WHERE rating > 4)";

        // Warmup
        for (var i = 0; i < WarmupIterations; i++)
        {
            var stmt = CCJSqlParserUtil.Parse(sql)!;
            var finder = new TablesNamesFinder();
            finder.GetTables(stmt);
        }

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < MeasureIterations; i++)
        {
            var stmt = CCJSqlParserUtil.Parse(sql)!;
            var finder = new TablesNamesFinder();
            finder.GetTables(stmt);
        }
        sw.Stop();

        var avgMs = sw.Elapsed.TotalMilliseconds / MeasureIterations;
        _output.WriteLine($"[TablesNamesFinder] Avg: {avgMs:F3}ms over {MeasureIterations} iterations");
        Assert.True(avgMs < 200, $"TablesNamesFinder too slow: {avgMs:F2}ms");
    }

    [Fact]
    public void Benchmark_ToString()
    {
        var sql = "SELECT u.id, u.name, o.total FROM users u " +
                  "INNER JOIN orders o ON u.id = o.user_id WHERE u.status = 'active'";

        var stmt = CCJSqlParserUtil.Parse(sql)!;

        // Warmup
        for (var i = 0; i < WarmupIterations; i++)
            _ = stmt.ToString();

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < MeasureIterations; i++)
            _ = stmt.ToString();
        sw.Stop();

        var avgMs = sw.Elapsed.TotalMilliseconds / MeasureIterations;
        _output.WriteLine($"[ToString] Avg: {avgMs:F3}ms over {MeasureIterations} iterations");
        Assert.True(avgMs < 50, $"ToString too slow: {avgMs:F2}ms");
    }

    [Fact]
    public void Benchmark_BatchParse()
    {
        // Warmup
        for (var i = 0; i < WarmupIterations; i++)
            foreach (var sql in SqlStatements)
                CCJSqlParserUtil.Parse(sql);

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < MeasureIterations; i++)
            foreach (var sql in SqlStatements)
                CCJSqlParserUtil.Parse(sql);
        sw.Stop();

        var totalOps = MeasureIterations * SqlStatements.Length;
        var avgMs = sw.Elapsed.TotalMilliseconds / totalOps;
        _output.WriteLine($"[Batch Parse] {totalOps} ops in {sw.Elapsed.TotalMilliseconds:F0}ms, Avg: {avgMs:F3}ms/op");
        Assert.True(avgMs < 50, $"Batch parse too slow: {avgMs:F2}ms/op");
    }

    private BenchmarkResult RunBenchmark(string name, string sql)
    {
        // Warmup
        for (var i = 0; i < WarmupIterations; i++)
            CCJSqlParserUtil.Parse(sql);

        var timings = new double[MeasureIterations];
        var sw = new Stopwatch();

        for (var i = 0; i < MeasureIterations; i++)
        {
            sw.Restart();
            CCJSqlParserUtil.Parse(sql);
            sw.Stop();
            timings[i] = sw.Elapsed.TotalMilliseconds;
        }

        return new BenchmarkResult
        {
            Name = name,
            Iterations = MeasureIterations,
            AverageMs = timings.Average(),
            MinMs = timings.Min(),
            MaxMs = timings.Max(),
            MedianMs = timings.OrderBy(x => x).ElementAt(MeasureIterations / 2)
        };
    }

    private void OutputResult(BenchmarkResult result)
    {
        _output.WriteLine($"[{result.Name}] Avg: {result.AverageMs:F3}ms, " +
                         $"Min: {result.MinMs:F3}ms, Max: {result.MaxMs:F3}ms, " +
                         $"Median: {result.MedianMs:F3}ms ({result.Iterations} iterations)");
    }

    private class BenchmarkResult
    {
        public string Name { get; set; } = "";
        public int Iterations { get; set; }
        public double AverageMs { get; set; }
        public double MinMs { get; set; }
        public double MaxMs { get; set; }
        public double MedianMs { get; set; }
    }
}
