using System.Net.Http;

namespace Azrng.NMaxCompute.Test.Integration;

/// <summary>
/// 集成测试用的极简 IHttpClientFactory：每次返回共享的 HttpClient。
/// 生产代码应使用 Microsoft.Extensions.Http 的 IHttpClientFactory。
/// </summary>
internal sealed class SimpleHttpClientFactory : IHttpClientFactory
{
    private static readonly HttpClient Shared = new();

    public HttpClient CreateClient(string name) => Shared;
}
