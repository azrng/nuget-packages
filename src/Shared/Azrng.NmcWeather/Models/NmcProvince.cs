namespace Azrng.NmcWeather.Models;

/// <summary>
/// 省份信息。
/// </summary>
public class NmcProvince
{
    /// <summary>
    /// 省份编码。
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 省份名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 省份详情页 URL。
    /// </summary>
    public string Url { get; set; } = string.Empty;
}
