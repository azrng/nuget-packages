namespace Azrng.NmcWeather.Models;

/// <summary>
/// 实时天气详情。
/// </summary>
public class NmcLiveWeather
{
    /// <summary>
    /// 当前温度（摄氏度）。
    /// </summary>
    public string? Temperature { get; set; }

    /// <summary>
    /// 温度与昨日差值。
    /// </summary>
    public string? TemperatureDiff { get; set; }

    /// <summary>
    /// 气压（百帕）。
    /// </summary>
    public string? Airpressure { get; set; }

    /// <summary>
    /// 相对湿度（百分比）。
    /// </summary>
    public string? Humidity { get; set; }

    /// <summary>
    /// 降水量（毫米）。
    /// </summary>
    public string? Rain { get; set; }

    /// <summary>
    /// 体感指数代码。
    /// </summary>
    public string? Rcomfort { get; set; }

    /// <summary>
    /// 体感描述。
    /// </summary>
    public string? Comfort { get; set; }

    /// <summary>
    /// 天气现象描述。
    /// </summary>
    public string? Info { get; set; }

    /// <summary>
    /// 天气现象图标编号。
    /// </summary>
    public string? Img { get; set; }

    /// <summary>
    /// 体感温度（摄氏度）。
    /// </summary>
    public string? FeelsLike { get; set; }
}
