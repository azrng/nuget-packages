namespace Azrng.NmcWeather.Models;

/// <summary>
/// 城市站点信息。
/// </summary>
public class NmcCity
{
    /// <summary>
    /// 城市站点编码。
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 所属省份名称。
    /// </summary>
    public string Province { get; set; } = string.Empty;

    /// <summary>
    /// 城市名称。
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// 城市详情页 URL。
    /// </summary>
    public string Url { get; set; } = string.Empty;
}
