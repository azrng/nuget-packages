namespace Azrng.NmcWeather.Models;

/// <summary>
/// 气象站点信息。
/// </summary>
public class NmcStation
{
    /// <summary>
    /// 站点编码。
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 所属省份名称。
    /// </summary>
    public string Province { get; set; } = string.Empty;

    /// <summary>
    /// 所属城市名称。
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// 站点详情页 URL。
    /// </summary>
    public string Url { get; set; } = string.Empty;
}
