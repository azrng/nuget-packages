namespace Azrng.NmcWeather.Models;

/// <summary>
/// 天气数据主体。
/// </summary>
public class NmcWeatherData
{
    /// <summary>
    /// 实时天气信息。
    /// </summary>
    public NmcRealWeather? Real { get; set; }

    /// <summary>
    /// 预报天气信息。
    /// </summary>
    public NmcPredictWeather? Predict { get; set; }

    /// <summary>
    /// 雷达信息。
    /// </summary>
    public NmcRadar? Radar { get; set; }
}
