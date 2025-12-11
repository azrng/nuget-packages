
using Azrng.Core.Extension;

namespace Azrng.SettingConfig.Dto;

/// <summary>
/// 获取配置版本列表返回类
/// </summary>
public class GetConfigVersionListResult
{
    /// <summary>
    /// 历史表的标识
    /// </summary>
    public int HisoryId { get; set; }

    /// <summary>
    /// 配置key(唯一)
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// 配置的值
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 配置版本
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public string UpdateTimeStr => UpdateTime.ToStandardString();
}