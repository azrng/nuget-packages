using Azrng.NMaxCompute.Models;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class ConnectionStringHintsTest
{
    [Fact]
    public void Hints_ParseRoundTrip()
    {
        var cs = new MaxComputeConnectionStringBuilder
        {
            Endpoint = "http://svc/api",
            AccessId = "id",
            SecretAccessKey = "key",
            Project = "p",
            Hints = "odps.sql.mapper.split.size=256,odps.sql.mapper.cpu=100"
        };

        var dict = cs.GetHintsDictionary();
        Assert.NotNull(dict);
        Assert.Equal("256", dict!["odps.sql.mapper.split.size"]);
        Assert.Equal("100", dict["odps.sql.mapper.cpu"]);
    }

    [Fact]
    public void ToConfig_CarriesHints()
    {
        var cs = new MaxComputeConnectionStringBuilder
        {
            Endpoint = "http://svc/api",
            AccessId = "id",
            SecretAccessKey = "key",
            Project = "p",
            Hints = "a=1,b=2"
        };

        var config = cs.ToConfig();
        Assert.NotNull(config.Hints);
        Assert.Equal("1", config.Hints!["a"]);
        Assert.Equal("2", config.Hints["b"]);
    }

    [Fact]
    public void Hints_Empty_ReturnsNullDict()
    {
        var cs = new MaxComputeConnectionStringBuilder
        {
            Endpoint = "http://svc/api",
            AccessId = "id",
            SecretAccessKey = "key",
            Project = "p"
        };
        Assert.Null(cs.GetHintsDictionary());
    }

    [Fact]
    public void Hints_TolerantWhitespaceAndGarbage()
    {
        var cs = new MaxComputeConnectionStringBuilder { Hints = "  a = 1 , ,badnoeq, b = 2 " };
        var dict = cs.GetHintsDictionary();
        Assert.NotNull(dict);
        Assert.Equal("1", dict!["a"].Trim());
        Assert.Equal("2", dict["b"].Trim());
    }

    [Fact]
    public void ToString_IncludesHints()
    {
        var cs = new MaxComputeConnectionStringBuilder
        {
            Endpoint = "http://svc/api",
            AccessId = "id",
            SecretAccessKey = "key",
            Project = "p",
            Hints = "a=1"
        };
        Assert.Contains("Hints=a=1", cs.ToString());
    }

    [Fact]
    public void UseLocalTimeZone_RoundTrip()
    {
        var cs = new MaxComputeConnectionStringBuilder
        {
            Endpoint = "http://svc/api",
            AccessId = "id",
            SecretAccessKey = "key",
            Project = "p",
            UseLocalTimeZone = false
        };

        // 设 false 后读回 false，ToConfig 带过去，连接串写出
        Assert.False(cs.UseLocalTimeZone);
        Assert.False(cs.ToConfig().UseLocalTimeZone);
        Assert.Contains("UseLocalTimeZone=false", cs.ToString());

        // 默认 true：不写出，ToConfig 也是 true
        cs.UseLocalTimeZone = true;
        Assert.DoesNotContain("UseLocalTimeZone", cs.ToString());
        Assert.True(cs.ToConfig().UseLocalTimeZone);
    }
}
