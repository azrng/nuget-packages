namespace Azrng.NmcWeather.Models;

/// <summary>
/// 风力信息。
/// </summary>
public class NmcWind
{
    /// <summary>
    /// 风向描述。
    /// </summary>
    public string? Direct { get; set; }

    /// <summary>
    /// 风力等级。
    /// </summary>
    public string? Power { get; set; }

    /// <summary>
    /// 风速（米/秒）。
    /// </summary>
    public string? Speed { get; set; }
}
