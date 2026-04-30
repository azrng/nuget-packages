namespace Azrng.NmcWeather.Internal;

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

    public static bool IsCodeMatch(string? source, string? target)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
        {
            return false;
        }

        return string.Equals(source.Trim(), target.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsProvinceNameMatch(string? source, string target)
    {
        return string.Equals(NormalizeProvinceName(source),
            NormalizeProvinceName(target),
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsCityNameMatch(string? source, string target)
    {
        return string.Equals(NormalizeCityName(source),
            NormalizeCityName(target),
            StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeProvinceName(string? value)
    {
        return Normalize(value, ProvinceSuffixes);
    }

    private static string NormalizeCityName(string? value)
    {
        return Normalize(value, CitySuffixes);
    }

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