namespace Azrng.NmcWeather.Models;

/// <summary>
/// 站点信息。
/// </summary>
public class NmcStation
{
    public string Code { get; set; } = string.Empty;

    public string Province { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}
