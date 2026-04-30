namespace Azrng.NmcWeather.Internal;

internal static class NmcClientArgumentHelper
{
    public static string NormalizeBaseUrl(string baseUrl)
    {
        var normalized = NormalizeRequiredText(baseUrl, nameof(baseUrl));
        return normalized.TrimEnd('/');
    }

    public static string NormalizePath(string path)
    {
        var normalized = NormalizeRequiredText(path, nameof(path));
        return normalized.StartsWith("/", StringComparison.Ordinal) ? normalized : $"/{normalized}";
    }

    public static string NormalizeRequiredProvinceCode(string value, string paramName)
    {
        return NormalizeRequiredText(value, paramName).ToUpperInvariant();
    }

    public static string NormalizeRequiredCityCode(string value, string paramName)
    {
        return NormalizeRequiredText(value, paramName);
    }

    public static string NormalizeRequiredText(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("参数不能为空。", paramName);
        }

        return value.Trim();
    }

    public static bool LooksLikeCityCode(string value)
    {
        return value.Length >= 4 && value.All(static c => IsAsciiLetterOrDigit(c) || c == '-' || c == '_');
    }

    private static bool IsAsciiLetterOrDigit(char value)
    {
        return value is >= '0' and <= '9'
            or >= 'A' and <= 'Z'
            or >= 'a' and <= 'z';
    }
}
