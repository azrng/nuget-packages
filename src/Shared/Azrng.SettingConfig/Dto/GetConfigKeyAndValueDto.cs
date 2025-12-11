namespace Azrng.SettingConfig.Dto
{
    /// <summary>
    /// 获取配置key以及内容
    /// </summary>
    public class GetConfigKeyAndValueDto
    {
        public GetConfigKeyAndValueDto(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// 配置
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Value { get; set; }
    }
}