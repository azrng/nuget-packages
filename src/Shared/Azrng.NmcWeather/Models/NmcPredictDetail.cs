namespace Azrng.NmcWeather.Models;

/// <summary>
/// 单日预报详情。
/// </summary>
public class NmcPredictDetail
{
    /// <summary>
    /// 预报日期。
    /// </summary>
    public string? Date { get; set; }

    /// <summary>
    /// 预报时段。
    /// </summary>
    public string? Pt { get; set; }

    /// <summary>
    /// 白天天气预报。
    /// </summary>
    public NmcForecastPeriod? Day { get; set; }

    /// <summary>
    /// 夜间天气预报。
    /// </summary>
    public NmcForecastPeriod? Night { get; set; }
}
