namespace Azrng.NmcWeather.Models;

/// <summary>
/// 预报天气信息。
/// </summary>
public class NmcPredictWeather
{
    /// <summary>
    /// 气象站点信息。
    /// </summary>
    public NmcStation? Station { get; set; }

    /// <summary>
    /// 预报发布时间。
    /// </summary>
    public string? PublishTime { get; set; }

    /// <summary>
    /// 逐日预报详情列表。
    /// </summary>
    public List<NmcPredictDetail> Detail { get; set; } = [];
}
