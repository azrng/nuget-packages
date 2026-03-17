namespace Azrng.SettingConfig.Dto
{
    /// <summary>
    /// 获取配置key以及内容
    /// </summary>
    public record GetConfigKeyAndValueDto(string Key, string Value);
}