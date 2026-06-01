namespace Azrng.NmcWeather.Models;

/// <summary>
/// 气象预警信息。
/// </summary>
public class NmcWarning
{
    /// <summary>
    /// 预警内容摘要。
    /// </summary>
    public string? Alert { get; set; }

    /// <summary>
    /// 预警信号图片 URL。
    /// </summary>
    public string? Pic { get; set; }

    /// <summary>
    /// 预警所属省份。
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// 预警所属城市。
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// 预警详情页 URL。
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 预警发布内容。
    /// </summary>
    public string? Issuecontent { get; set; }

    /// <summary>
    /// 预警名称。
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 预警信号类型。
    /// </summary>
    public string? Signaltype { get; set; }

    /// <summary>
    /// 预警信号等级。
    /// </summary>
    public string? Signalevel { get; set; }

    /// <summary>
    /// 预警唯一标识。
    /// </summary>
    public string? Pid2 { get; set; }
}
