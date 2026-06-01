namespace Azrng.NmcWeather.Models;

/// <summary>
/// 白天或夜间天气预报时段。
/// </summary>
public class NmcForecastPeriod
{
    /// <summary>
    /// 天气详情。
    /// </summary>
    public NmcForecastWeather? Weather { get; set; }

    /// <summary>
    /// 风力信息。
    /// </summary>
    public NmcWind? Wind { get; set; }
}
