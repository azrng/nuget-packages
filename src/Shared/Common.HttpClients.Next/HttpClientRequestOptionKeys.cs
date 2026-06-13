using System.Net.Http;

namespace Common.HttpClients
{
    internal static class HttpClientRequestOptionKeys
    {
        internal static readonly HttpRequestOptionsKey<bool> SkipResponseBodyAudit =
            new("Common.HttpClients.SkipResponseBodyAudit");
    }
}
