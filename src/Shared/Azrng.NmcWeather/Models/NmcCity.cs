namespace Azrng.NmcWeather.Models;

/// <summary>
/// 城市站点信息。
/// </summary>
public class NmcCity
{
    public string Code { get; set; } = string.Empty;

    public string Province { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}
