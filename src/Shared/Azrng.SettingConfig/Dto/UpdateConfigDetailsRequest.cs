using System.ComponentModel.DataAnnotations;

namespace Azrng.SettingConfig.Dto;

/// <summary>
/// 更新配置版本详情请求类
/// </summary>
public class UpdateConfigDetailsRequest
{
    /// <summary>
    /// 配置标识
    /// </summary>
    public int ConfigId { get; set; }

    /// <summary>
    /// 配置说明
    /// </summary>
    [Required(ErrorMessage = "请输入配置说明")]
    public string Description { get; set; }

    /// <summary>
    /// 配置的值
    /// </summary>
    [Required(ErrorMessage = "请输入配置的值")]
    public string Value { get; set; }
}