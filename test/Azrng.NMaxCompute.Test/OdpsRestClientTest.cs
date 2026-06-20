using Azrng.NMaxCompute.Accounts;
using Azrng.NMaxCompute.Rest;
using Xunit;

namespace Azrng.NMaxCompute.Test;

/// <summary>
/// 回归 OdpsRestClient 的 User-Agent 注入：构造函数不得写共享 HttpClient 的 DefaultRequestHeaders，
/// 否则并发构造会腐蚀 HttpHeaders 内部 store（产生空 key / null value 条目），导致
/// SendAsync 在 HttpConnection.WriteAsciiString 抛 NullReferenceException。
/// </summary>
public class OdpsRestClientTest
{
    private static CloudAccount DummyAccount()
        => new("test-id", "test-secret", "cn-shanghai", useV4Signature: true);

    [Fact]
    public void Constructor_DoesNotWriteUserAgentToSharedClient()
    {
        var shared = new HttpClient();
        var account = DummyAccount();

        _ = new OdpsRestClient(shared, account, "https://example.com/api");

        Assert.Empty(shared.DefaultRequestHeaders);
        Assert.Empty(shared.DefaultRequestHeaders.UserAgent);
    }

    /// <summary>
    /// 回归：在同一个共享 HttpClient 上并发构造多个 OdpsRestClient，DefaultRequestHeaders
    /// 不应出现空 key 或 null value 条目（旧实现会腐蚀出 []=&lt;NULL&gt;）。
    /// </summary>
    [Fact]
    public void ConcurrentConstruction_DoesNotCorruptSharedHeaders()
    {
        var shared = new HttpClient();
        var account = DummyAccount();

        Parallel.For(0, 64, i =>
        {
            var rest = new OdpsRestClient(shared, account, "https://example.com/api");
            _ = rest; // 仅消费构造副作用
        });

        Assert.All(shared.DefaultRequestHeaders, h =>
        {
            Assert.False(string.IsNullOrEmpty(h.Key));
            Assert.All(h.Value, v => Assert.NotNull(v));
        });
    }
}
