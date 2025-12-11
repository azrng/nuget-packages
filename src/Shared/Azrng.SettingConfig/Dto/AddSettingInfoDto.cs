namespace Azrng.SettingConfig.Dto
{
    /// <summary>
    /// 添加配置
    /// </summary>
    public class AddSettingInfoDto
    {
        /// <summary>
        /// 配置key(唯一)
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 配置名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 配置的值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 配置说明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }
    }
}