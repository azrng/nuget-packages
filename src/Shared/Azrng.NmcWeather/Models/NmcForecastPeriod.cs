namespace Azrng.NmcWeather.Models;

/// <summary>
/// 白天或夜间天气。
/// </summary>
public class NmcForecastPeriod
{
    public NmcForecastWeather? Weather { get; set; }

    public NmcWind? Wind { get; set; }
}
