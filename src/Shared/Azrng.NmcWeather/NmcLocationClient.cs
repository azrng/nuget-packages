using Azrng.NmcWeather.Internal;
using Azrng.NmcWeather.Models;
using Common.HttpClients;
using Microsoft.Extensions.Options;

namespace Azrng.NmcWeather;

/// <summary>
/// 中央气象台地区与编码查询客户端实现。
/// </summary>
public class NmcLocationClient : INmcLocationClient
{
    private readonly IHttpHelper _httpHelper;
    private readonly NmcWeatherOptions _options;

    public NmcLocationClient(IHttpHelper httpHelper, IOptions<NmcWeatherOptions> options)
    {
        _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<IReadOnlyList<NmcProvince>> GetProvincesAsync(CancellationToken cancellationToken = default)
    {
        var provinces = await _httpHelper
            .GetAsync<List<NmcProvince>>(BuildProvinceUrl(), cancellation: cancellationToken)
            .ConfigureAwait(false);

        return provinces ?? new List<NmcProvince>();
    }

    public async Task<NmcProvince?> GetProvinceByCodeAsync(string provinceCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = NmcClientArgumentHelper.NormalizeRequiredProvinceCode(provinceCode, nameof(provinceCode));
        var provinces = await GetProvincesAsync(cancellationToken).ConfigureAwait(false);
        return provinces.FirstOrDefault(province =>
            LocationNameNormalizer.IsCodeMatch(province.Code, normalizedCode));
    }

    public async Task<NmcProvince?> GetProvinceByNameAsync(string provinceName, CancellationToken cancellationToken = default)
    {
        var normalizedName = NmcClientArgumentHelper.NormalizeRequiredText(provinceName, nameof(provinceName));
        var provinces = await GetProvincesAsync(cancellationToken).ConfigureAwait(false);
        return provinces.FirstOrDefault(province =>
            LocationNameNormalizer.IsProvinceNameMatch(province.Name, normalizedName));
    }

    public async Task<NmcProvince?> GetProvinceAsync(string provinceNameOrCode, CancellationToken cancellationToken = default)
    {
        var normalizedText = NmcClientArgumentHelper.NormalizeRequiredText(provinceNameOrCode, nameof(provinceNameOrCode));
        var provinces = await GetProvincesAsync(cancellationToken).ConfigureAwait(false);
        return provinces.FirstOrDefault(province =>
                   LocationNameNormalizer.IsCodeMatch(province.Code, normalizedText))
               ?? provinces.FirstOrDefault(province =>
                   LocationNameNormalizer.IsProvinceNameMatch(province.Name, normalizedText));
    }

    public async Task<string?> GetProvinceCodeAsync(string provinceName, CancellationToken cancellationToken = default)
    {
        var province = await GetProvinceByNameAsync(provinceName, cancellationToken).ConfigureAwait(false);
        return province?.Code;
    }

    public async Task<string?> GetProvinceNameAsync(string provinceCode, CancellationToken cancellationToken = default)
    {
        var province = await GetProvinceByCodeAsync(provinceCode, cancellationToken).ConfigureAwait(false);
        return province?.Name;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetProvinceCodeMapAsync(CancellationToken cancellationToken = default)
    {
        var provinces = await GetProvincesAsync(cancellationToken).ConfigureAwait(false);
        return provinces
            .Where(static province => !string.IsNullOrWhiteSpace(province.Name) && !string.IsNullOrWhiteSpace(province.Code))
            .GroupBy(static province => province.Name)
            .ToDictionary(static group => group.Key, static group => group.First().Code);
    }

    public async Task<IReadOnlyList<NmcCity>> GetCitiesByProvinceCodeAsync(string provinceCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = NmcClientArgumentHelper.NormalizeRequiredProvinceCode(provinceCode, nameof(provinceCode));
        var cities = await _httpHelper
            .GetAsync<List<NmcCity>>(BuildProvinceUrl(normalizedCode), cancellation: cancellationToken)
            .ConfigureAwait(false);

        return cities ?? new List<NmcCity>();
    }

    public async Task<IReadOnlyList<NmcCity>> GetCitiesByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default)
    {
        var province = await GetProvinceByNameAsync(provinceName, cancellationToken).ConfigureAwait(false);
        return province == null
            ? Array.Empty<NmcCity>()
            : await GetCitiesByProvinceCodeAsync(province.Code, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<NmcCity>> GetCitiesByProvinceAsync(string provinceNameOrCode, CancellationToken cancellationToken = default)
    {
        var province = await GetProvinceAsync(provinceNameOrCode, cancellationToken).ConfigureAwait(false);
        return province == null
            ? Array.Empty<NmcCity>()
            : await GetCitiesByProvinceCodeAsync(province.Code, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> GetCityNamesByProvinceCodeAsync(string provinceCode, CancellationToken cancellationToken = default)
    {
        var cities = await GetCitiesByProvinceCodeAsync(provinceCode, cancellationToken).ConfigureAwait(false);
        return ExtractDistinctCityNames(cities);
    }

    public async Task<IReadOnlyList<string>> GetCityNamesByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default)
    {
        var cities = await GetCitiesByProvinceNameAsync(provinceName, cancellationToken).ConfigureAwait(false);
        return ExtractDistinctCityNames(cities);
    }

    public async Task<IReadOnlyList<string>> GetCityNamesByProvinceAsync(string provinceNameOrCode, CancellationToken cancellationToken = default)
    {
        var cities = await GetCitiesByProvinceAsync(provinceNameOrCode, cancellationToken).ConfigureAwait(false);
        return ExtractDistinctCityNames(cities);
    }

    public async Task<IReadOnlyList<string>> GetCityCodesByProvinceCodeAsync(string provinceCode, CancellationToken cancellationToken = default)
    {
        var cities = await GetCitiesByProvinceCodeAsync(provinceCode, cancellationToken).ConfigureAwait(false);
        return ExtractDistinctCityCodes(cities);
    }

    public async Task<IReadOnlyList<string>> GetCityCodesByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default)
    {
        var cities = await GetCitiesByProvinceNameAsync(provinceName, cancellationToken).ConfigureAwait(false);
        return ExtractDistinctCityCodes(cities);
    }

    public async Task<IReadOnlyList<string>> GetCityCodesByProvinceAsync(string provinceNameOrCode, CancellationToken cancellationToken = default)
    {
        var cities = await GetCitiesByProvinceAsync(provinceNameOrCode, cancellationToken).ConfigureAwait(false);
        return ExtractDistinctCityCodes(cities);
    }

    public async Task<IReadOnlyDictionary<string, string>> GetCityCodeMapByProvinceCodeAsync(string provinceCode, CancellationToken cancellationToken = default)
    {
        var cities = await GetCitiesByProvinceCodeAsync(provinceCode, cancellationToken).ConfigureAwait(false);
        return ExtractCityCodeMap(cities);
    }

    public async Task<IReadOnlyDictionary<string, string>> GetCityCodeMapByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default)
    {
        var cities = await GetCitiesByProvinceNameAsync(provinceName, cancellationToken).ConfigureAwait(false);
        return ExtractCityCodeMap(cities);
    }

    public async Task<IReadOnlyDictionary<string, string>> GetCityCodeMapByProvinceAsync(string provinceNameOrCode, CancellationToken cancellationToken = default)
    {
        var cities = await GetCitiesByProvinceAsync(provinceNameOrCode, cancellationToken).ConfigureAwait(false);
        return ExtractCityCodeMap(cities);
    }

    public async Task<NmcCity?> GetCityByCodeAsync(string cityCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = NmcClientArgumentHelper.NormalizeRequiredCityCode(cityCode, nameof(cityCode));
        var provinces = await GetProvincesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var province in provinces)
        {
            var city = await FindCityByCodeAsync(province.Code, normalizedCode, cancellationToken).ConfigureAwait(false);
            if (city != null)
            {
                return city;
            }
        }

        return null;
    }

    public async Task<NmcCity?> GetCityByNameAsync(
        string cityName,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedCityName = NmcClientArgumentHelper.NormalizeRequiredText(cityName, nameof(cityName));

        if (!string.IsNullOrWhiteSpace(provinceCode))
        {
            return await FindCityByNameAsync(provinceCode, normalizedCityName, cancellationToken).ConfigureAwait(false);
        }

        if (!string.IsNullOrWhiteSpace(provinceName))
        {
            var province = await GetProvinceByNameAsync(provinceName, cancellationToken).ConfigureAwait(false);
            return province == null
                ? null
                : await FindCityByNameAsync(province.Code, normalizedCityName, cancellationToken).ConfigureAwait(false);
        }

        var matchedCities = await SearchCitiesByNameAsync(normalizedCityName, cancellationToken).ConfigureAwait(false);
        return matchedCities.FirstOrDefault();
    }

    public async Task<NmcCity?> GetCityAsync(
        string cityNameOrCode,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedText = NmcClientArgumentHelper.NormalizeRequiredText(cityNameOrCode, nameof(cityNameOrCode));

        if (NmcClientArgumentHelper.LooksLikeCityCode(normalizedText))
        {
            return await GetCityByCodeAsync(normalizedText, cancellationToken).ConfigureAwait(false)
                   ?? await GetCityByNameAsync(normalizedText, provinceCode, provinceName, cancellationToken).ConfigureAwait(false);
        }

        return await GetCityByNameAsync(normalizedText, provinceCode, provinceName, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string?> GetCityCodeByNameAsync(
        string cityName,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default)
    {
        var city = await GetCityByNameAsync(cityName, provinceCode, provinceName, cancellationToken).ConfigureAwait(false);
        return city?.Code;
    }

    public async Task<string?> GetCityNameByCodeAsync(string cityCode, CancellationToken cancellationToken = default)
    {
        var city = await GetCityByCodeAsync(cityCode, cancellationToken).ConfigureAwait(false);
        return city?.City;
    }

    public async Task<string?> GetCityCodeAsync(
        string cityNameOrCode,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default)
    {
        var city = await GetCityAsync(cityNameOrCode, provinceCode, provinceName, cancellationToken).ConfigureAwait(false);
        return city?.Code;
    }

    public async Task<IReadOnlyList<NmcCity>> SearchCitiesByNameAsync(string cityName, CancellationToken cancellationToken = default)
    {
        var normalizedCityName = NmcClientArgumentHelper.NormalizeRequiredText(cityName, nameof(cityName));
        var provinces = await GetProvincesAsync(cancellationToken).ConfigureAwait(false);
        var matchedCities = new List<NmcCity>();

        foreach (var province in provinces)
        {
            var cities = await GetCitiesByProvinceCodeAsync(province.Code, cancellationToken).ConfigureAwait(false);
            matchedCities.AddRange(cities.Where(city =>
                LocationNameNormalizer.IsCityNameMatch(city.City, normalizedCityName)));
        }

        return matchedCities;
    }

    private async Task<NmcCity?> FindCityByCodeAsync(string provinceCode, string cityCode, CancellationToken cancellationToken)
    {
        var cities = await GetCitiesByProvinceCodeAsync(provinceCode, cancellationToken).ConfigureAwait(false);
        return cities.FirstOrDefault(city => LocationNameNormalizer.IsCodeMatch(city.Code, cityCode));
    }

    private async Task<NmcCity?> FindCityByNameAsync(string provinceCode, string cityName, CancellationToken cancellationToken)
    {
        var cities = await GetCitiesByProvinceCodeAsync(provinceCode, cancellationToken).ConfigureAwait(false);
        return cities.FirstOrDefault(city => LocationNameNormalizer.IsCityNameMatch(city.City, cityName));
    }

    private string BuildProvinceUrl(string? provinceCode = null)
    {
        var baseUrl = NmcClientArgumentHelper.NormalizeBaseUrl(_options.BaseUrl);
        var provincePath = NmcClientArgumentHelper.NormalizePath(_options.ProvincePath);
        return string.IsNullOrWhiteSpace(provinceCode)
            ? $"{baseUrl}{provincePath}"
            : $"{baseUrl}{provincePath}/{NmcClientArgumentHelper.NormalizeRequiredProvinceCode(provinceCode, nameof(provinceCode))}";
    }

    private static IReadOnlyList<string> ExtractDistinctCityNames(IEnumerable<NmcCity> cities)
    {
        return cities
            .Select(static city => city.City)
            .Where(static cityName => !string.IsNullOrWhiteSpace(cityName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<string> ExtractDistinctCityCodes(IEnumerable<NmcCity> cities)
    {
        return cities
            .Select(static city => city.Code)
            .Where(static cityCode => !string.IsNullOrWhiteSpace(cityCode))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyDictionary<string, string> ExtractCityCodeMap(IEnumerable<NmcCity> cities)
    {
        return cities
            .Where(static city => !string.IsNullOrWhiteSpace(city.City) && !string.IsNullOrWhiteSpace(city.Code))
            .GroupBy(static city => city.City)
            .ToDictionary(static group => group.Key, static group => group.First().Code);
    }
}
