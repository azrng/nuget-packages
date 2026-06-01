namespace Azrng.NmcWeather.Models;

/// <summary>
/// 气象雷达信息。
/// </summary>
public class NmcRadar
{
    /// <summary>
    /// 雷达图标题。
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 雷达图图片 URL。
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// 雷达详情页 URL。
    /// </summary>
    public string? Url { get; set; }
}
