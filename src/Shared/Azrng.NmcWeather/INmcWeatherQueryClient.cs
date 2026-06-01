using Azrng.NmcWeather.Models;

namespace Azrng.NmcWeather;

/// <summary>
/// 中央气象台天气便捷查询客户端。
/// </summary>
public interface INmcWeatherQueryClient
{
    /// <summary>
    /// 根据城市名称便捷获取天气信息，内部自动解析城市编码。
    /// </summary>
    /// <param name="cityName">城市名称。</param>
    /// <param name="provinceCode">省份编码，可选，用于缩小查找范围。</param>
    /// <param name="provinceName">省份名称，可选，用于缩小查找范围。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>天气数据信封，城市编码无法解析时返回 <c>null</c>。</returns>
    Task<NmcWeatherEnvelope?> GetWeatherByCityNameAsync(
        string cityName,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据城市名称或编码便捷获取天气信息，自动判断输入类型并解析编码。
    /// </summary>
    /// <param name="cityNameOrCode">城市名称或编码。</param>
    /// <param name="provinceCode">省份编码，可选，用于缩小查找范围。</param>
    /// <param name="provinceName">省份名称，可选，用于缩小查找范围。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>天气数据信封，城市编码无法解析时返回 <c>null</c>。</returns>
    Task<NmcWeatherEnvelope?> GetWeatherByCityAsync(
        string cityNameOrCode,
        string? provinceCode = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default);
}
