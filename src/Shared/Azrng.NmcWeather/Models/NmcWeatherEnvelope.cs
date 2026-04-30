namespace Azrng.NmcWeather.Models;

/// <summary>
/// 天气接口返回结果。
/// </summary>
public class NmcWeatherEnvelope
{
    public string? Msg { get; set; }

    public int Code { get; set; }

    public NmcWeatherData? Data { get; set; }
}
