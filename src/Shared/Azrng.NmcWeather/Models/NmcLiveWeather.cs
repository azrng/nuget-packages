namespace Azrng.NmcWeather.Models;

/// <summary>
/// 实时天气详情。
/// </summary>
public class NmcLiveWeather
{
    public string? Temperature { get; set; }

    public string? TemperatureDiff { get; set; }

    public string? Airpressure { get; set; }

    public string? Humidity { get; set; }

    public string? Rain { get; set; }

    public string? Rcomfort { get; set; }

    public string? Comfort { get; set; }

    public string? Info { get; set; }

    public string? Img { get; set; }

    public string? FeelsLike { get; set; }
}
