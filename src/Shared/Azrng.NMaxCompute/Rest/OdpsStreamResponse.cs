using System.Net.Http;

namespace Azrng.NMaxCompute.Rest;

/// <summary>
/// 流式响应：成功时持有可读取的 <see cref="Stream"/> 与响应头。
/// <para>dispose 时会连带释放底层 HttpResponseMessage / HttpContent。</para>
/// </summary>
public sealed class OdpsStreamResponse : IDisposable
{
    private readonly HttpResponseMessage _httpResponse;
    private readonly Stream _stream;
    private bool _disposed;

    internal OdpsStreamResponse(int statusCode, Dictionary<string, string> headers, Stream stream, HttpResponseMessage httpResponse)
    {
        StatusCode = statusCode;
        Headers = headers;
        _stream = stream;
        _httpResponse = httpResponse;
    }

    public int StatusCode { get; }

    public Dictionary<string, string> Headers { get; }

    /// <summary>
    /// 响应体流（已用 <see cref="HttpCompletionOption.ResponseHeadersRead"/> 拉起，未读完）。
    /// </summary>
    public Stream Stream => _stream;

    public void Dispose()
    {
        if (!_disposed)
        {
            _stream.Dispose();
            _httpResponse.Dispose();
            _disposed = true;
        }
    }
}
