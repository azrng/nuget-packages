namespace Azrng.NmcWeather.Internal;

/// <summary>
/// 地名标准化工具，去除行政后缀以实现宽松名称匹配。
/// </summary>
internal static class LocationNameNormalizer
{
    private static readonly string[] ProvinceSuffixes =
    [
        "维吾尔自治区",
        "壮族自治区",
        "回族自治区",
        "特别行政区",
        "自治区",
        "省",
        "市"
    ];

    private static readonly string[] CitySuffixes =
    [
        "自治州",
        "地区",
        "盟",
        "州",
        "市"
    ];

    /// <summary>
    /// 比较两个编码是否相同（忽略大小写和首尾空格）。
    /// </summary>
    /// <param name="source">源编码。</param>
    /// <param name="target">目标编码。</param>
    /// <returns>匹配返回 <c>true</c>，否则返回 <c>false</c>。</returns>
    public static bool IsCodeMatch(string? source, string? target)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
        {
            return false;
        }

        return string.Equals(source.Trim(), target.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 比较两个省份名称是否相同（去除行政后缀后忽略大小写比较）。
    /// </summary>
    /// <param name="source">源省份名称。</param>
    /// <param name="target">目标省份名称。</param>
    /// <returns>匹配返回 <c>true</c>，否则返回 <c>false</c>。</returns>
    public static bool IsProvinceNameMatch(string? source, string target)
    {
        return string.Equals(NormalizeProvinceName(source),
            NormalizeProvinceName(target),
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 比较两个城市名称是否相同（去除行政后缀后忽略大小写比较）。
    /// </summary>
    /// <param name="source">源城市名称。</param>
    /// <param name="target">目标城市名称。</param>
    /// <returns>匹配返回 <c>true</c>，否则返回 <c>false</c>。</returns>
    public static bool IsCityNameMatch(string? source, string target)
    {
        return string.Equals(NormalizeCityName(source),
            NormalizeCityName(target),
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 标准化省份名称，去除省市级行政后缀。
    /// </summary>
    private static string NormalizeProvinceName(string? value)
    {
        return Normalize(value, ProvinceSuffixes);
    }

    /// <summary>
    /// 标准化城市名称，去除地市级行政后缀。
    /// </summary>
    private static string NormalizeCityName(string? value)
    {
        return Normalize(value, CitySuffixes);
    }

    /// <summary>
    /// 去除名称中的行政后缀并移除空白字符。
    /// </summary>
    private static string Normalize(string? value, IEnumerable<string> suffixes)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = string.Concat(value.Trim().Where(static c => !char.IsWhiteSpace(c)));
        foreach (var suffix in suffixes.OrderByDescending(static item => item.Length))
        {
            if (normalized.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized[..^suffix.Length];
                break;
            }
        }

        return normalized;
    }
}
