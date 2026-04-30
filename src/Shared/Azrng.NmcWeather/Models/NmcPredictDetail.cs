namespace Azrng.NmcWeather.Models;

/// <summary>
/// 单日预报详情。
/// </summary>
public class NmcPredictDetail
{
    public string? Date { get; set; }

    public string? Pt { get; set; }

    public NmcForecastPeriod? Day { get; set; }

    public NmcForecastPeriod? Night { get; set; }
}
