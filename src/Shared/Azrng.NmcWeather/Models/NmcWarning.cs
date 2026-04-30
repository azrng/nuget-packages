namespace Azrng.NmcWeather.Models;

/// <summary>
/// 预警信息。
/// </summary>
public class NmcWarning
{
    public string? Alert { get; set; }

    public string? Pic { get; set; }

    public string? Province { get; set; }

    public string? City { get; set; }

    public string? Url { get; set; }

    public string? Issuecontent { get; set; }

    public string? Name { get; set; }

    public string? Signaltype { get; set; }

    public string? Signalevel { get; set; }

    public string? Pid2 { get; set; }
}
