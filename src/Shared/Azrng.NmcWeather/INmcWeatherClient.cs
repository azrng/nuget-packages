namespace Azrng.NmcWeather;

/// <summary>
/// 中央气象台天气客户端。
/// </summary>
public interface INmcWeatherClient
{
    /// <summary>
    /// 根据城市编码获取天气。
    /// </summary>
    Task<Models.NmcWeatherEnvelope?> GetWeatherByCityCodeAsync(string cityCode, CancellationToken cancellationToken = default);
}
