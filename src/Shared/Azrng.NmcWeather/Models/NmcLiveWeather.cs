using Azrng.NmcWeather.Serialization;
using System.Text.Json.Serialization;

namespace Azrng.NmcWeather.Models;

/// <summary>
/// 实时天气详情。
/// </summary>
public class NmcLiveWeather
{
    /// <summary>
    /// 当前温度（摄氏度）。
    /// </summary>
    [JsonConverter(typeof(NmcStringValueConverter))]
    public string? Temperature { get; set; }

    /// <summary>
    /// 温度与昨日差值。
    /// </summary>
    [JsonConverter(typeof(NmcStringValueConverter))]
    public string? TemperatureDiff { get; set; }

    /// <summary>
    /// 气压（百帕）。
    /// </summary>
    [JsonConverter(typeof(NmcStringValueConverter))]
    public string? Airpressure { get; set; }

    /// <summary>
    /// 相对湿度（百分比）。
    /// </summary>
    [JsonConverter(typeof(NmcStringValueConverter))]
    public string? Humidity { get; set; }

    /// <summary>
    /// 降水量（毫米）。
    /// </summary>
    [JsonConverter(typeof(NmcStringValueConverter))]
    public string? Rain { get; set; }

    /// <summary>
    /// 体感指数代码。
    /// </summary>
    [JsonConverter(typeof(NmcStringValueConverter))]
    public string? Rcomfort { get; set; }

    /// <summary>
    /// 体感描述。
    /// </summary>
    [JsonConverter(typeof(NmcStringValueConverter))]
    public string? Comfort { get; set; }

    /// <summary>
    /// 天气现象描述。
    /// </summary>
    [JsonConverter(typeof(NmcStringValueConverter))]
    public string? Info { get; set; }

    /// <summary>
    /// 天气现象图标编号。
    /// </summary>
    [JsonConverter(typeof(NmcStringValueConverter))]
    public string? Img { get; set; }

    /// <summary>
    /// 体感温度（摄氏度）。
    /// </summary>
    [JsonConverter(typeof(NmcStringValueConverter))]
    public string? FeelsLike { get; set; }
}
