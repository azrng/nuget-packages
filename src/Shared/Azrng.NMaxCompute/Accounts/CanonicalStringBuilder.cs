using System.Globalization;
using System.Text;
using Azrng.NMaxCompute.Rest;

namespace Azrng.NMaxCompute.Accounts;

/// <summary>
/// ODPS 请求规范化字符串（StringToSign）构造器
/// <para>精确复刻 PyODPS <c>BaseAccount._build_canonical_str</c>。</para>
/// <para>
/// 规则：
/// 1. 第一行 HTTP method
/// 2. 后续每行：headers_to_sign 中按 key 字典序输出。x-odps-* 前缀输出 <c>key:value</c>，其它（content-type/content-md5/date）只输出 value
/// 3. 最后一行 canonical_resource = path[?sorted_query]
/// 4. 行之间用 <c>\n</c> 连接
/// </para>
/// </summary>
public static class CanonicalStringBuilder
{
    /// <summary>
    /// 构造 StringToSign；如果 headers 中没有 Date，会被填充并写回（PyODPS 行为）
    /// </summary>
    /// <returns>规范化签名串</returns>
    public static string Build(OdpsRequest request)
    {
        var lines = new List<string> { request.Method };

        var headersToSign = new Dictionary<string, string>(StringComparer.Ordinal);

        // 收集 x-odps-* / content-type / content-md5
        foreach (var (key, value) in request.Headers)
        {
            var lower = key.ToLowerInvariant();
            if (lower is "content-type" or "content-md5" || lower.StartsWith("x-odps", StringComparison.Ordinal))
            {
                headersToSign[lower] = value;
            }
        }

        // content-type / content-md5 缺失补空
        if (!headersToSign.ContainsKey("content-type"))
            headersToSign["content-type"] = string.Empty;
        if (!headersToSign.ContainsKey("content-md5"))
            headersToSign["content-md5"] = string.Empty;

        // Date header：必须存在，缺失时生成 GMT 时间塞回
        string? dateStr = null;
        foreach (var (key, value) in request.Headers)
        {
            if (key.Equals("Date", StringComparison.OrdinalIgnoreCase))
            {
                dateStr = value;
                break;
            }
        }
        if (string.IsNullOrEmpty(dateStr))
        {
            dateStr = FormatGmtDate(DateTimeOffset.UtcNow);
            request.Headers["Date"] = dateStr;
        }
        headersToSign["date"] = dateStr;

        // query 中 x-odps-* 参数也加入 headers_to_sign
        foreach (var (qKey, qValue) in request.Query)
        {
            if (qKey.StartsWith("x-odps-", StringComparison.Ordinal))
            {
                headersToSign[qKey] = qValue;
            }
        }

        // canonical_resource
        var canonicalResource = BuildCanonicalResource(request.Path, request.Query);

        // 输出排序后的 headers
        foreach (var key in headersToSign.Keys.OrderBy(static k => k, StringComparer.Ordinal))
        {
            var value = headersToSign[key];
            if (key.StartsWith("x-odps", StringComparison.Ordinal))
            {
                lines.Add($"{key}:{value}");
            }
            else
            {
                lines.Add(value);
            }
        }

        lines.Add(canonicalResource);

        return string.Join("\n", lines);
    }

    private static string BuildCanonicalResource(string path, List<KeyValuePair<string, string>> query)
    {
        var resource = new StringBuilder(path);

        if (query.Count > 0)
        {
            var sorted = query
                .OrderBy(static kv => kv.Key, StringComparer.Ordinal)
                .ToList();

            var parts = new List<string>(sorted.Count);
            foreach (var kv in sorted)
            {
                parts.Add(kv.Value.Length == 0 ? kv.Key : $"{kv.Key}={kv.Value}");
            }

            resource.Append('?').Append(string.Join("&", parts));
        }

        return resource.ToString();
    }

    /// <summary>
    /// RFC 2822 / RFC 1123 GMT 时间格式，与 Python <c>email.utils.formatdate(usegmt=True)</c> 一致
    /// </summary>
    public static string FormatGmtDate(DateTimeOffset time)
    {
        return time.UtcDateTime.ToString("R", CultureInfo.InvariantCulture);
    }
}
