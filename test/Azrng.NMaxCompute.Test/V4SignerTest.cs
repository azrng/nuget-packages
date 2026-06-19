using Azrng.NMaxCompute.Accounts.Signers;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class V4SignerTest
{
    [Fact]
    public void BuildAuthorization_Format()
    {
        var signer = new V4Signer("LTAI5tFakeAccessId", "FakeSecretKey123", "cn-hangzhou");
        var auth = signer.BuildAuthorization("GET\n\n\nFri, 13 Feb 2026 09:00:00 GMT\n/projects/p");

        // ODPS accessId/YYYYMMDD/region/odps/aliyun_v4_request:signature
        Assert.StartsWith("ODPS LTAI5tFakeAccessId/", auth);
        Assert.Contains("/cn-hangzhou/odps/aliyun_v4_request:", auth);
    }

    [Fact]
    public void BuildAuthorization_DateIsTodayUtc()
    {
        var signer = new V4Signer("id", "secret", "cn-hangzhou");
        var auth = signer.BuildAuthorization("canonical");

        var expectedDate = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        Assert.Contains($"/{expectedDate}/", auth);
    }

    [Fact]
    public void BuildAuthorization_Deterministic()
    {
        var signer = new V4Signer("id", "secret", "cn-hangzhou");
        var a = signer.BuildAuthorization("canonical");
        var b = signer.BuildAuthorization("canonical");
        Assert.Equal(a, b);
    }

    [Fact]
    public void Constructor_NullRegion_Throws()
    {
        Assert.Throws<ArgumentException>(() => new V4Signer("id", "secret", null!));
    }
}
