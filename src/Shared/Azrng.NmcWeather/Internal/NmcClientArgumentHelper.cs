namespace Azrng.NmcWeather.Internal;

/// <summary>
/// 客户端参数校验与标准化工具。
/// </summary>
internal static class NmcClientArgumentHelper
{
    /// <summary>
    /// 标准化接口基地址，去除首尾空白和末尾斜杠。
    /// </summary>
    /// <param name="baseUrl">基地址字符串。</param>
    /// <returns>标准化后的基地址。</returns>
    public static string NormalizeBaseUrl(string baseUrl)
    {
        var normalized = NormalizeRequiredText(baseUrl, nameof(baseUrl));
        return normalized.TrimEnd('/');
    }

    /// <summary>
    /// 标准化接口路径，确保以斜杠开头。
    /// </summary>
    /// <param name="path">路径字符串。</param>
    /// <returns>标准化后的路径。</returns>
    public static string NormalizePath(string path)
    {
        var normalized = NormalizeRequiredText(path, nameof(path));
        return normalized.StartsWith("/", StringComparison.Ordinal) ? normalized : $"/{normalized}";
    }

    /// <summary>
    /// 校验并标准化省份编码（转为大写）。
    /// </summary>
    /// <param name="value">省份编码。</param>
    /// <param name="paramName">参数名称，用于异常消息。</param>
    /// <returns>大写省份编码。</returns>
    public static string NormalizeRequiredProvinceCode(string value, string paramName)
    {
        return NormalizeRequiredText(value, paramName).ToUpperInvariant();
    }

    /// <summary>
    /// 校验并标准化城市编码。
    /// </summary>
    /// <param name="value">城市编码。</param>
    /// <param name="paramName">参数名称，用于异常消息。</param>
    /// <returns>标准化后的城市编码。</returns>
    public static string NormalizeRequiredCityCode(string value, string paramName)
    {
        return NormalizeRequiredText(value, paramName);
    }

    /// <summary>
    /// 校验文本参数非空并去除首尾空白。
    /// </summary>
    /// <param name="value">待校验文本。</param>
    /// <param name="paramName">参数名称，用于异常消息。</param>
    /// <returns>去除空白后的文本。</returns>
    /// <exception cref="ArgumentException">参数为空或仅包含空白时抛出。</exception>
    public static string NormalizeRequiredText(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("参数不能为空。", paramName);
        }

        return value.Trim();
    }

    /// <summary>
    /// NMC 站点编码固定长度（基于线上 2400+ 样本统计：100% 为 5 位 base62）。
    /// </summary>
    private const int CityCodeLength = 5;

    /// <summary>
    /// 判断输入值是否看起来像城市编码。
    /// NMC 站点编码恒为 5 位字母数字（base62），既无连字符/下划线，也非其他长度。
    /// 收紧为精确格式以避免拼音、英文短词等被误判为编码从而触发全量遍历。
    /// </summary>
    /// <param name="value">待判断的值。</param>
    /// <returns>符合 NMC 编码格式返回 <c>true</c>，否则返回 <c>false</c>。</returns>
    public static bool LooksLikeCityCode(string value)
    {
        return value.Length == CityCodeLength && value.All(IsAsciiLetterOrDigit);
    }

    /// <summary>
    /// 判断字符是否为 ASCII 字母或数字。
    /// </summary>
    private static bool IsAsciiLetterOrDigit(char value)
    {
        return value is >= '0' and <= '9'
            or >= 'A' and <= 'Z'
            or >= 'a' and <= 'z';
    }
}
