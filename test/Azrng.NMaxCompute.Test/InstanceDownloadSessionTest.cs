using Azrng.NMaxCompute.Rest;
using Azrng.NMaxCompute.Tunnel;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class InstanceDownloadSessionTest
{
    [Theory]
    [InlineData(null, TunnelSessionStatus.Unknown)]
    [InlineData("", TunnelSessionStatus.Unknown)]
    [InlineData("normal", TunnelSessionStatus.Normal)]
    [InlineData("NORMAL", TunnelSessionStatus.Normal)]
    [InlineData("Closed", TunnelSessionStatus.Closed)]
    [InlineData("CLOSES", TunnelSessionStatus.Closed)]
    [InlineData("expired", TunnelSessionStatus.Expired)]
    [InlineData("failed", TunnelSessionStatus.Failed)]
    [InlineData("initiating", TunnelSessionStatus.Initiating)]
    [InlineData("garbage", TunnelSessionStatus.Unknown)]
    public void ParseStatus_HandlesKnownValues(string? raw, TunnelSessionStatus expected)
    {
        Assert.Equal(expected, InstanceDownloadSession.ParseStatus(raw));
    }

    [Fact]
    public void TunnelVersion_IsSix()
    {
        // PyODPS odps/tunnel/base.py::TUNNEL_VERSION
        Assert.Equal(6, InstanceDownloadSession.TunnelVersion);
    }

    [Fact]
    public void ParseResponse_PopulatesAllFields()
    {
        var body = @"{
            ""DownloadID"": ""20260619abc"",
            ""Status"": ""NORMAL"",
            ""RecordCount"": 42,
            ""QuotaName"": ""quota_a"",
            ""Schema"": {
                ""columns"": [
                    {""name"": ""id"", ""type"": ""bigint"", ""isNullable"": false},
                    {""name"": ""val"", ""type"": ""double""}
                ]
            }
        }";

        var session = Parse(body);

        Assert.Equal("20260619abc", session.Id);
        Assert.Equal(TunnelSessionStatus.Normal, session.Status);
        Assert.Equal(42, session.RecordCount);
        Assert.Equal("quota_a", session.QuotaName);
        Assert.Equal(2, session.Schema.Columns.Count);
        Assert.Equal("id", session.Schema.Columns[0].Name);
        Assert.Equal("bigint", session.Schema.Columns[0].Type);
        Assert.False(session.Schema.Columns[0].IsNullable);
    }

    [Fact]
    public void ParseResponse_EmptyBody_Throws()
    {
        Assert.Throws<OdpsException>(() => Parse(""));
    }

    [Fact]
    public void ParseResponse_InvalidJson_Throws()
    {
        Assert.Throws<OdpsException>(() => Parse("not-json"));
    }

    [Fact]
    public void ParseResponse_PartialJson_LeavesDefaults()
    {
        var session = Parse(@"{""DownloadID"": ""only_id""}");

        Assert.Equal("only_id", session.Id);
        Assert.Equal(TunnelSessionStatus.Unknown, session.Status);
        Assert.Equal(0, session.RecordCount);
        Assert.Null(session.QuotaName);
        Assert.Empty(session.Schema.Columns);
    }

    private static InstanceDownloadSession NewSession()
    {
        // 使用 internal 工厂，避免依赖网络
        return InstanceDownloadSession.FromResponseJsonForTest("proj", "inst", @"{""DownloadID"":""x""}");
    }

    private static InstanceDownloadSession Parse(string body)
    {
        return InstanceDownloadSession.FromResponseJsonForTest("proj", "inst", body);
    }
}
