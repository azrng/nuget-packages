using Azrng.NMaxCompute;
using Azrng.NMaxCompute.Adapter;
using Azrng.NMaxCompute.Models;
using Xunit;

namespace Azrng.NMaxCompute.Test;

/// <summary>
/// MaxComputeCommand.MergeConfig 回归：命令带 Hints 时合并 config，必须保留所有非 Hints 字段
/// （曾漏 UseLocalTimeZone，导致命令级 Hints 场景下时区开关被静默重置为默认）。
/// </summary>
public class MaxComputeCommandMergeTest
{
    private sealed class StubExecutor : IQueryExecutor
    {
        public Task<QueryResult> ExecuteQueryAsync(MaxComputeConfig config, string sql, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task<bool> TestConnectionAsync(MaxComputeConfig config, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    [Fact]
    public void MergeConfig_PreservesUseLocalTimeZone_WhenHintsPresent()
    {
        var config = new MaxComputeConfig
        {
            Endpoint = "http://svc/api",
            AccessId = "id",
            SecretAccessKey = "k",
            Project = "p",
            UseLocalTimeZone = false
        };
        var cmd = new MaxComputeCommand(config, new StubExecutor())
        {
            Hints = new Dictionary<string, string> { ["odps.sql.mapper.split.size"] = "256" }
        };

        var merged = cmd.MergeConfig();

        Assert.False(merged.UseLocalTimeZone);   // 命令有 Hints 时合并不能丢 UseLocalTimeZone
        Assert.Equal("256", merged.Hints!["odps.sql.mapper.split.size"]);
    }

    [Fact]
    public void MergeConfig_NoHints_ReturnsOriginalConfig()
    {
        var config = new MaxComputeConfig
        {
            Endpoint = "http://svc/api",
            AccessId = "id",
            SecretAccessKey = "k",
            Project = "p",
            UseLocalTimeZone = false
        };
        var cmd = new MaxComputeCommand(config, new StubExecutor());   // 无 Hints

        Assert.Same(config, cmd.MergeConfig());   // 无 Hints 直接返回原 config（含 UseLocalTimeZone）
    }
}
