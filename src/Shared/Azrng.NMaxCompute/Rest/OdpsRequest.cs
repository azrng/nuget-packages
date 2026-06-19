using System.Text;

namespace Azrng.NMaxCompute.Rest;

/// <summary>
/// ODPS REST API 请求模型
/// </summary>
public sealed class OdpsRequest
{
    private static readonly string[] AllowedMethods = { "GET", "POST", "PUT", "DELETE", "HEAD" };

    private string _method = "GET";

    /// <summary>
    /// HTTP 方法（大写）
    /// </summary>
    public string Method
    {
        get => _method;
        init => _method = string.IsNullOrWhiteSpace(value)
            ? "GET"
            : value.ToUpperInvariant();
    }

    /// <summary>
    /// 相对 endpoint 的路径，如 <c>/projects/my_proj/instances</c>
    /// </summary>
    public string Path { get; init; } = "/";

    /// <summary>
    /// 查询参数（按加入顺序保留；签名时按 key 排序）
    /// </summary>
    public List<KeyValuePair<string, string>> Query { get; init; } = new();

    /// <summary>
    /// 请求头（key 不区分大小写）
    /// </summary>
    public Dictionary<string, string> Headers { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 请求体（POST/PUT 用），可为 null
    /// </summary>
    public byte[]? Body { get; set; }

    /// <summary>
    /// 添加查询参数
    /// </summary>
    public OdpsRequest WithQuery(string key, string? value = null)
    {
        Query.Add(new KeyValuePair<string, string>(key, value ?? string.Empty));
        return this;
    }

    /// <summary>
    /// 添加请求头
    /// </summary>
    public OdpsRequest WithHeader(string key, string value)
    {
        Headers[key] = value;
        return this;
    }

    /// <summary>
    /// 设置字符串 body（UTF-8 编码）
    /// </summary>
    public OdpsRequest WithStringBody(string body)
    {
        Body = Encoding.UTF8.GetBytes(body ?? string.Empty);
        return this;
    }
}
