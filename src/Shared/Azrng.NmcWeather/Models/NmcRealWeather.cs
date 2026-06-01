namespace Azrng.NmcWeather.Models;

/// <summary>
/// 实时天气信息。
/// </summary>
public class NmcRealWeather
{
    /// <summary>
    /// 气象站点信息。
    /// </summary>
    public NmcStation? Station { get; set; }

    /// <summary>
    /// 数据发布时间。
    /// </summary>
    public string? PublishTime { get; set; }

    /// <summary>
    /// 实时天气详情。
    /// </summary>
    public NmcLiveWeather? Weather { get; set; }

    /// <summary>
    /// 风力信息。
    /// </summary>
    public NmcWind? Wind { get; set; }

    /// <summary>
    /// 预警信息。
    /// </summary>
    public NmcWarning? Warn { get; set; }
}
