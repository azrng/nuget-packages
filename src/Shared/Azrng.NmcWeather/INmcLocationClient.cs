using Azrng.NmcWeather.Models;

namespace Azrng.NmcWeather;

/// <summary>
/// 中央气象台地区与编码查询客户端。
/// </summary>
public interface INmcLocationClient
{
    /// <summary>
    /// 获取全部省份列表。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>省份列表。</returns>
    Task<IReadOnlyList<NmcProvince>> GetProvincesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份编码获取省份信息。
    /// </summary>
    /// <param name="provinceCode">省份编码，例如 "ABJ"。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>匹配的省份信息，未找到时返回 <c>null</c>。</returns>
    Task<NmcProvince?> GetProvinceByCodeAsync(string provinceCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份名称获取省份信息，支持行政后缀宽松匹配（如 "北京" 与 "北京市"）。
    /// </summary>
    /// <param name="provinceName">省份名称。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>匹配的省份信息，未找到时返回 <c>null</c>。</returns>
    Task<NmcProvince?> GetProvinceByNameAsync(string provinceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份名称或编码获取省份信息，自动判断输入类型。
    /// </summary>
    /// <param name="provinceNameOrCode">省份名称或编码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>匹配的省份信息，未找到时返回 <c>null</c>。</returns>
    Task<NmcProvince?> GetProvinceAsync(string provinceNameOrCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份名称获取省份编码。
    /// </summary>
    /// <param name="provinceName">省份名称。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>省份编码，未找到时返回 <c>null</c>。</returns>
    Task<string?> GetProvinceCodeAsync(string provinceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份编码获取省份名称。
    /// </summary>
    /// <param name="provinceCode">省份编码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>省份名称，未找到时返回 <c>null</c>。</returns>
    Task<string?> GetProvinceNameAsync(string provinceCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取省份名称与编码的映射字典。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>键为省份名称、值为省份编码的字典。</returns>
    Task<IReadOnlyDictionary<string, string>> GetProvinceCodeMapAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份编码获取该省下辖城市列表。
    /// </summary>
    /// <param name="provinceCode">省份编码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>城市列表。</returns>
    Task<IReadOnlyList<NmcCity>> GetCitiesByProvinceCodeAsync(string provinceCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份名称获取该省下辖城市列表。
    /// </summary>
    /// <param name="provinceName">省份名称。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>城市列表。</returns>
    Task<IReadOnlyList<NmcCity>> GetCitiesByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份名称或编码获取该省下辖城市列表，自动判断输入类型。
    /// </summary>
    /// <param name="provinceNameOrCode">省份名称或编码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>城市列表。</returns>
    Task<IReadOnlyList<NmcCity>> GetCitiesByProvinceAsync(string provinceNameOrCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份编码获取该省下辖城市的名称列表（去重）。
    /// </summary>
    /// <param name="provinceCode">省份编码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>城市名称列表。</returns>
    Task<IReadOnlyList<string>> GetCityNamesByProvinceCodeAsync(string provinceCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份名称获取该省下辖城市的名称列表（去重）。
    /// </summary>
    /// <param name="provinceName">省份名称。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>城市名称列表。</returns>
    Task<IReadOnlyList<string>> GetCityNamesByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份名称或编码获取该省下辖城市的名称列表（去重），自动判断输入类型。
    /// </summary>
    /// <param name="provinceNameOrCode">省份名称或编码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>城市名称列表。</returns>
    Task<IReadOnlyList<string>> GetCityNamesByProvinceAsync(string provinceNameOrCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份编码获取该省下辖城市的编码列表（去重）。
    /// </summary>
    /// <param name="provinceCode">省份编码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>城市编码列表。</returns>
    Task<IReadOnlyList<string>> GetCityCodesByProvinceCodeAsync(string provinceCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份名称获取该省下辖城市的编码列表（去重）。
    /// </summary>
    /// <param name="provinceName">省份名称。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>城市编码列表。</returns>
    Task<IReadOnlyList<string>> GetCityCodesByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份名称或编码获取该省下辖城市的编码列表（去重），自动判断输入类型。
    /// </summary>
    /// <param name="provinceNameOrCode">省份名称或编码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>城市编码列表。</returns>
    Task<IReadOnlyList<string>> GetCityCodesByProvinceAsync(string provinceNameOrCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份编码获取该省下辖城市名称与编码的映射字典。
    /// </summary>
    /// <param name="provinceCode">省份编码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>键为城市名称、值为城市编码的字典。</returns>
    Task<IReadOnlyDictionary<string, string>> GetCityCodeMapByProvinceCodeAsync(string provinceCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份名称获取该省下辖城市名称与编码的映射字典。
    /// </summary>
    /// <param name="provinceName">省份名称。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>键为城市名称、值为城市编码的字典。</returns>
    Task<IReadOnlyDictionary<string, string>> GetCityCodeMapByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据省份名称或编码获取该省下辖城市名称与编码的映射字典，自动判断输入类型。
    /// </summary>
    /// <param name="provinceNameOrCode">省份名称或编码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>键为城市名称、值为城市编码的字典。</returns>
    Task<IReadOnlyDictionary<string, string>> GetCityCodeMapByProvinceAsync(string provinceNameOrCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据城市编码获取城市信息（遍历所有省份查找）。
    /// </summary>
    /// <param name="cityCode">城市编码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>匹配的城市信息，未找到时返回 <c>null</c>。</returns>
    Task<NmcCity?> GetCityByCodeAsync(string cityCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据城市名称获取城市信息，可选指定省份范围以提高查找精度。
    /// </summary>
    /// <param name="cityName">城市名称。</param>
    /// <param name="provinceCode">省份编码，可选。</param>
    /// <param name="provinceName">省份名称，可选。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>匹配的城市信息，未找到时返回 <c>null</c>。</returns>
    Task<NmcCity?> GetCityByNameAsync(
        string cityName,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据城市名称或编码获取城市信息，自动判断输入类型并支持可选省份范围。
    /// </summary>
    /// <param name="cityNameOrCode">城市名称或编码。</param>
    /// <param name="provinceCode">省份编码，可选。</param>
    /// <param name="provinceName">省份名称，可选。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>匹配的城市信息，未找到时返回 <c>null</c>。</returns>
    Task<NmcCity?> GetCityAsync(
        string cityNameOrCode,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据城市名称获取城市编码。
    /// </summary>
    /// <param name="cityName">城市名称。</param>
    /// <param name="provinceCode">省份编码，可选。</param>
    /// <param name="provinceName">省份名称，可选。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>城市编码，未找到时返回 <c>null</c>。</returns>
    Task<string?> GetCityCodeByNameAsync(
        string cityName,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据城市编码获取城市名称。
    /// </summary>
    /// <param name="cityCode">城市编码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>城市名称，未找到时返回 <c>null</c>。</returns>
    Task<string?> GetCityNameByCodeAsync(string cityCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据城市名称或编码获取城市编码，自动判断输入类型。
    /// </summary>
    /// <param name="cityNameOrCode">城市名称或编码。</param>
    /// <param name="provinceCode">省份编码，可选。</param>
    /// <param name="provinceName">省份名称，可选。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>城市编码，未找到时返回 <c>null</c>。</returns>
    Task<string?> GetCityCodeAsync(
        string cityNameOrCode,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 在全部省份中搜索匹配指定名称的城市（支持行政后缀宽松匹配）。
    /// </summary>
    /// <param name="cityName">城市名称。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>匹配的城市列表。</returns>
    Task<IReadOnlyList<NmcCity>> SearchCitiesByNameAsync(string cityName, CancellationToken cancellationToken = default);
}
