using Azrng.NmcWeather.Models;

namespace Azrng.NmcWeather;

/// <summary>
/// 中央气象台天气便捷查询客户端。
/// </summary>
public interface INmcWeatherQueryClient
{
    Task<NmcWeatherEnvelope?> GetWeatherByCityNameAsync(
        string cityName,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default);

    Task<NmcWeatherEnvelope?> GetWeatherByCityAsync(
        string cityNameOrCode,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default);
}
