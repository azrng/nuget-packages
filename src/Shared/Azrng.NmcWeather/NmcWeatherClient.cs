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

    /// <summary>
    /// 初始化天气客户端实例。
    /// </summary>
    /// <param name="httpHelper">HTTP 请求辅助工具。</param>
    /// <param name="options">天气客户端配置。</param>
    public NmcWeatherClient(IHttpHelper httpHelper, IOptions<NmcWeatherOptions> options)
    {
        _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public async Task<NmcWeatherEnvelope?> GetWeatherByCityCodeAsync(string cityCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = NmcClientArgumentHelper.NormalizeRequiredCityCode(cityCode, nameof(cityCode));
        return await _httpHelper
            .GetAsync<NmcWeatherEnvelope>(BuildWeatherUrl(normalizedCode), cancellation: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 构建天气查询接口的完整 URL。
    /// </summary>
    /// <param name="cityCode">城市站点编码。</param>
    /// <returns>完整的天气查询 URL。</returns>
    private string BuildWeatherUrl(string cityCode)
    {
        var baseUrl = NmcClientArgumentHelper.NormalizeBaseUrl(_options.BaseUrl);
        var weatherPath = NmcClientArgumentHelper.NormalizePath(_options.WeatherPath);
        return $"{baseUrl}{weatherPath}?stationid={NmcClientArgumentHelper.NormalizeRequiredCityCode(cityCode, nameof(cityCode))}";
    }
}
