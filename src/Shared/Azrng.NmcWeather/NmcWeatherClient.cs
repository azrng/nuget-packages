using Azrng.NmcWeather.Internal;
using Azrng.NmcWeather.Models;
using Common.HttpClients;
using Microsoft.Extensions.Options;

namespace Azrng.NmcWeather;

/// <summary>
/// 中央气象台天气客户端实现，只负责按站点编码查询天气。
/// </summary>
public class NmcWeatherClient : INmcWeatherClient
{
    private readonly IHttpHelper _httpHelper;
    private readonly NmcWeatherOptions _options;

    public NmcWeatherClient(IHttpHelper httpHelper, IOptions<NmcWeatherOptions> options)
    {
        _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<NmcWeatherEnvelope?> GetWeatherByCityCodeAsync(string cityCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = NmcClientArgumentHelper.NormalizeRequiredCityCode(cityCode, nameof(cityCode));
        return await _httpHelper
            .GetAsync<NmcWeatherEnvelope>(BuildWeatherUrl(normalizedCode), cancellation: cancellationToken)
            .ConfigureAwait(false);
    }

    private string BuildWeatherUrl(string cityCode)
    {
        var baseUrl = NmcClientArgumentHelper.NormalizeBaseUrl(_options.BaseUrl);
        var weatherPath = NmcClientArgumentHelper.NormalizePath(_options.WeatherPath);
        return $"{baseUrl}{weatherPath}?stationid={NmcClientArgumentHelper.NormalizeRequiredCityCode(cityCode, nameof(cityCode))}";
    }
}
