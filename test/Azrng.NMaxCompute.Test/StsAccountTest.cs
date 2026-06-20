using Azrng.NMaxCompute.Accounts;
using Azrng.NMaxCompute.Rest;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class StsAccountTest
{
    [Fact]
    public void Sign_InjectsAuthorizationStsTokenHeader()
    {
        var account = new StsAccount("id", "secret", "sts-token-value", region: null, useV4Signature: false);
        var request = new OdpsRequest { Method = "GET", Path = "/projects/p/instances/x" };

        account.Sign(request);

        // 必须注入 sts token 头（PyODPS: authorization-sts-token）
        Assert.True(request.Headers.TryGetValue("authorization-sts-token", out var token));
        Assert.Equal("sts-token-value", token);
        // Authorization 也必须存在（内层 CloudAccount 签名）
        Assert.True(request.Headers.ContainsKey("Authorization"));
    }

    [Fact]
    public void Constructor_RejectsEmptyToken()
    {
        Assert.Throws<ArgumentException>(() => new StsAccount("id", "secret", "", region: null));
        Assert.Throws<ArgumentException>(() => new StsAccount("id", "secret", "   ", region: null));
    }

    [Fact]
    public void Inner_ExposesCloudAccount()
    {
        var account = new StsAccount("id", "secret", "tok", region: "cn-hangzhou");
        Assert.IsType<CloudAccount>(account.Inner);
        Assert.Equal("id", account.AccessId);
    }

    [Fact]
    public void Sign_RoundTrip_TokenStableAcrossRequests()
    {
        var account = new StsAccount("id", "secret", "tok", region: null, useV4Signature: false);
        var r1 = new OdpsRequest { Method = "GET", Path = "/a" };
        var r2 = new OdpsRequest { Method = "GET", Path = "/b" };

        account.Sign(r1);
        account.Sign(r2);

        Assert.Equal(r1.Headers["authorization-sts-token"], r2.Headers["authorization-sts-token"]);
    }
}
