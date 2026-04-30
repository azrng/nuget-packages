namespace Azrng.NmcWeather;

/// <summary>
/// 中央气象台天气客户端配置。
/// </summary>
public class NmcWeatherOptions
{
    /// <summary>
    /// 接口基地址。
    /// </summary>
    public string BaseUrl { get; set; } = "http://www.nmc.cn";

    /// <summary>
    /// 省份接口路径。
    /// </summary>
    public string ProvincePath { get; set; } = "/rest/province";

    /// <summary>
    /// 天气接口路径。
    /// </summary>
    public string WeatherPath { get; set; } = "/rest/weather";
}
