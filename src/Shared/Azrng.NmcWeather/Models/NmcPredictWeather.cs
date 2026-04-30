namespace Azrng.NmcWeather.Models;

/// <summary>
/// 预报天气。
/// </summary>
public class NmcPredictWeather
{
    public NmcStation? Station { get; set; }

    public string? PublishTime { get; set; }

    public List<NmcPredictDetail> Detail { get; set; } = [];
}
