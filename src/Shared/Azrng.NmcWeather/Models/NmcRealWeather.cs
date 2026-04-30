namespace Azrng.NmcWeather.Models;

/// <summary>
/// 实时天气。
/// </summary>
public class NmcRealWeather
{
    public NmcStation? Station { get; set; }

    public string? PublishTime { get; set; }

    public NmcLiveWeather? Weather { get; set; }

    public NmcWind? Wind { get; set; }

    public NmcWarning? Warn { get; set; }
}
