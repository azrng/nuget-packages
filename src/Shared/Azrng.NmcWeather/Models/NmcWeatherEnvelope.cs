namespace Azrng.NmcWeather.Models;

/// <summary>
/// 天气接口返回结果信封。
/// </summary>
public class NmcWeatherEnvelope
{
    /// <summary>
    /// 响应消息。
    /// </summary>
    public string? Msg { get; set; }

    /// <summary>
    /// 响应状态码。
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 天气数据主体。
    /// </summary>
    public NmcWeatherData? Data { get; set; }
}
