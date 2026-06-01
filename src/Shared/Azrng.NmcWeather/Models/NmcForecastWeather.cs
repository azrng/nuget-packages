namespace Azrng.NmcWeather.Models;

/// <summary>
/// 预报天气详情。
/// </summary>
public class NmcForecastWeather
{
    /// <summary>
    /// 天气现象描述。
    /// </summary>
    public string? Info { get; set; }

    /// <summary>
    /// 天气现象图标编号。
    /// </summary>
    public string? Img { get; set; }

    /// <summary>
    /// 预报温度（摄氏度）。
    /// </summary>
    public string? Temperature { get; set; }
}
