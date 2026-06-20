using Azrng.NMaxCompute.Adapter;
using Azrng.NMaxCompute.Models;
using Xunit;

namespace Azrng.NMaxCompute.Test;

/// <summary>
/// 记录最后一次接收到的 config，用于验证 MaxComputeCommand.Hints 的合并行为。
/// </summary>
internal sealed class RecordingExecutor : IQueryExecutor
{
    public MaxComputeConfig? LastConfig { get; private set; }

    public Task<QueryResult> ExecuteQueryAsync(MaxComputeConfig config, string sql, CancellationToken cancellationToken = default)
    {
        LastConfig = config;
        return Task.FromResult(new QueryResult
        {
            Columns = new[] { "c" },
            Rows = new[] { new object[] { 1 } },
            RowCount = 1
        });
    }

    public Task<bool> TestConnectionAsync(MaxComputeConfig config, CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}

public class CommandHintsOverrideTest
{
    private static MaxComputeConfig BaseConfig(IDictionary<string, string>? hints = null) => new()
    {
        Endpoint = "http://svc/api",
        AccessId = "id",
        SecretAccessKey = "key",
        Project = "p",
        Hints = hints
    };

    [Fact]
    public void CommandHints_OverrideConfigHints()
    {
        var executor = new RecordingExecutor();
        var config = BaseConfig(new Dictionary<string, string>
        {
            ["odps.sql.mapper.cpu"] = "50",
            ["odps.sql.mapper.split.size"] = "128"
        });

        var cmd = new MaxComputeCommand(config, executor)
        {
            CommandText = "SELECT 1",
            Hints = new Dictionary<string, string>
            {
                ["odps.sql.mapper.cpu"] = "100",          // 覆盖
                ["odps.sql.mapper.memory"] = "1024"       // 新增
            }
        };

        cmd.ExecuteNonQuery();

        var received = executor.LastConfig!;
        Assert.NotNull(received.Hints);
        Assert.Equal("100", received.Hints!["odps.sql.mapper.cpu"]);
        Assert.Equal("128", received.Hints["odps.sql.mapper.split.size"]);   // 保留 config 值
        Assert.Equal("1024", received.Hints["odps.sql.mapper.memory"]);
    }

    [Fact]
    public void CommandHints_Null_PassesThroughConfigHints()
    {
        var executor = new RecordingExecutor();
        var config = BaseConfig(new Dictionary<string, string> { ["a"] = "1" });

        var cmd = new MaxComputeCommand(config, executor) { CommandText = "SELECT 1" };
        cmd.ExecuteNonQuery();

        Assert.Same(config, executor.LastConfig);   // 无合并时直接复用原 config
        Assert.Equal("1", executor.LastConfig!.Hints!["a"]);
    }

    [Fact]
    public void CommandHints_Empty_DoesNotMerge()
    {
        var executor = new RecordingExecutor();
        var config = BaseConfig(new Dictionary<string, string> { ["a"] = "1" });

        var cmd = new MaxComputeCommand(config, executor)
        {
            CommandText = "SELECT 1",
            Hints = new Dictionary<string, string>()   // 空 → 不触发合并，直接复用 config
        };
        cmd.ExecuteNonQuery();

        Assert.Same(config, executor.LastConfig);
        Assert.Equal("1", executor.LastConfig!.Hints!["a"]);
    }
}
