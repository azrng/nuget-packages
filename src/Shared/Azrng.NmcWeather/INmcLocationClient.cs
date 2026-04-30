using Azrng.NmcWeather.Models;

namespace Azrng.NmcWeather;

/// <summary>
/// 中央气象台地区与编码查询客户端。
/// </summary>
public interface INmcLocationClient
{
    Task<IReadOnlyList<NmcProvince>> GetProvincesAsync(CancellationToken cancellationToken = default);

    Task<NmcProvince?> GetProvinceByCodeAsync(string provinceCode, CancellationToken cancellationToken = default);

    Task<NmcProvince?> GetProvinceByNameAsync(string provinceName, CancellationToken cancellationToken = default);

    Task<NmcProvince?> GetProvinceAsync(string provinceNameOrCode, CancellationToken cancellationToken = default);

    Task<string?> GetProvinceCodeAsync(string provinceName, CancellationToken cancellationToken = default);

    Task<string?> GetProvinceNameAsync(string provinceCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, string>> GetProvinceCodeMapAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NmcCity>> GetCitiesByProvinceCodeAsync(string provinceCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NmcCity>> GetCitiesByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NmcCity>> GetCitiesByProvinceAsync(string provinceNameOrCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetCityNamesByProvinceCodeAsync(string provinceCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetCityNamesByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetCityNamesByProvinceAsync(string provinceNameOrCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetCityCodesByProvinceCodeAsync(string provinceCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetCityCodesByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetCityCodesByProvinceAsync(string provinceNameOrCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, string>> GetCityCodeMapByProvinceCodeAsync(string provinceCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, string>> GetCityCodeMapByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, string>> GetCityCodeMapByProvinceAsync(string provinceNameOrCode, CancellationToken cancellationToken = default);

    Task<NmcCity?> GetCityByCodeAsync(string cityCode, CancellationToken cancellationToken = default);

    Task<NmcCity?> GetCityByNameAsync(
        string cityName,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default);

    Task<NmcCity?> GetCityAsync(
        string cityNameOrCode,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default);

    Task<string?> GetCityCodeByNameAsync(
        string cityName,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default);

    Task<string?> GetCityNameByCodeAsync(string cityCode, CancellationToken cancellationToken = default);

    Task<string?> GetCityCodeAsync(
        string cityNameOrCode,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NmcCity>> SearchCitiesByNameAsync(string cityName, CancellationToken cancellationToken = default);
}
