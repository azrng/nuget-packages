using Azrng.NmcWeather.Models;

namespace Azrng.NmcWeather;

/// <summary>
/// 中央气象台天气便捷查询客户端实现。
/// </summary>
public class NmcWeatherQueryClient : INmcWeatherQueryClient
{
    private readonly INmcLocationClient _locationClient;
    private readonly INmcWeatherClient _weatherClient;

    public NmcWeatherQueryClient(INmcLocationClient locationClient, INmcWeatherClient weatherClient)
    {
        _locationClient = locationClient ?? throw new ArgumentNullException(nameof(locationClient));
        _weatherClient = weatherClient ?? throw new ArgumentNullException(nameof(weatherClient));
    }

    public async Task<NmcWeatherEnvelope?> GetWeatherByCityNameAsync(
        string cityName,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default)
    {
        var cityCode = await _locationClient
            .GetCityCodeByNameAsync(cityName, provinceCode, provinceName, cancellationToken)
            .ConfigureAwait(false);

        return string.IsNullOrWhiteSpace(cityCode)
            ? null
            : await _weatherClient.GetWeatherByCityCodeAsync(cityCode, cancellationToken).ConfigureAwait(false);
    }

    public async Task<NmcWeatherEnvelope?> GetWeatherByCityAsync(
        string cityNameOrCode,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default)
    {
        var cityCode = await _locationClient
            .GetCityCodeAsync(cityNameOrCode, provinceCode, provinceName, cancellationToken)
            .ConfigureAwait(false);

        return string.IsNullOrWhiteSpace(cityCode)
            ? null
            : await _weatherClient.GetWeatherByCityCodeAsync(cityCode, cancellationToken).ConfigureAwait(false);
    }
}
