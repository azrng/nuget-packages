using Azrng.NMaxCompute.Rest;
using Azrng.NMaxCompute.Tunnel;
using Xunit;

namespace Azrng.NMaxCompute.Test;

/// <summary>
/// TableDownloadSession 响应解析单测（不发起网络请求；读链路复用已测的 TunnelRecordReader / BufferedRecordReader）。
/// </summary>
public class TableDownloadSessionTest
{
    private const string SampleBody = /*lang=json,strict*/
        @"{""DownloadID"":""DL20260621abc"",""Status"":""OK"",""RecordCount"":42,""QuotaName"":""q1"",
          ""Schema"":{""columns"":[{""name"":""id"",""type"":""bigint""},{""name"":""ts"",""type"":""datetime""}]}}";

    [Fact]
    public void ParseResponse_ExtractsIdRecordCountSchema()
    {
        var s = TableDownloadSession.FromResponseJsonForTest("p", "t", null, null, SampleBody);

        Assert.Equal("DL20260621abc", s.Id);
        Assert.Equal(42, s.RecordCount);
        Assert.Equal("q1", s.QuotaName);
        Assert.Equal(2, s.Schema.Columns.Count);
        Assert.Equal("id", s.Schema.Columns[0].Name);
        Assert.Equal("bigint", s.Schema.Columns[0].Type);
        Assert.Equal("ts", s.Schema.Columns[1].Name);
        Assert.Equal("datetime", s.Schema.Columns[1].Type);
    }

    [Fact]
    public void UseLocalTimeZone_DefaultsTrue()
        => Assert.True(TableDownloadSession.FromResponseJsonForTest("p", "t", null, null, SampleBody).UseLocalTimeZone);

    [Fact]
    public void ParseResponse_EmptyBody_Throws()
        => Assert.Throws<OdpsException>(() =>
            TableDownloadSession.FromResponseJsonForTest("p", "t", null, null, ""));

    [Fact]
    public void ParseResponse_MalformedJson_Throws()
        => Assert.Throws<OdpsException>(() =>
            TableDownloadSession.FromResponseJsonForTest("p", "t", null, null, "{not json"));

    [Fact]
    public void ParseResponse_MissingFields_Defaults()
    {
        // 缺 DownloadID/RecordCount/Schema 时不崩，走默认值
        var s = TableDownloadSession.FromResponseJsonForTest("p", "t", "ds=2026", "mc2", @"{""Status"":""OK""}");
        Assert.Equal(string.Empty, s.Id);
        Assert.Equal(0, s.RecordCount);
        Assert.Empty(s.Schema.Columns);
    }
}
