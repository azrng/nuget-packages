namespace Azrng.NMaxCompute.Rest;

/// <summary>
/// ODPS REST 响应
/// </summary>
public sealed class OdpsResponse
{
    public int StatusCode { get; init; }

    public Dictionary<string, string> Headers { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public byte[]? BodyBytes { get; init; }

    public string? BodyText { get; init; }

    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

    /// <summary>
    /// 从响应头 Location 中提取 instance id（提交 SQL 任务后服务端返回）
    /// </summary>
    public string? TryGetInstanceId()
    {
        if (!Headers.TryGetValue("Location", out var location) || string.IsNullOrEmpty(location))
            return null;

        var trimmed = location.TrimEnd('/');
        var lastSlash = trimmed.LastIndexOf('/');
        return lastSlash < 0 ? trimmed : trimmed[(lastSlash + 1)..];
    }
}
