namespace Azrng.NmcWeather.Models;

/// <summary>
/// 天气数据主体。
/// </summary>
public class NmcWeatherData
{
    public NmcRealWeather? Real { get; set; }

    public NmcPredictWeather? Predict { get; set; }

    public NmcRadar? Radar { get; set; }
}
