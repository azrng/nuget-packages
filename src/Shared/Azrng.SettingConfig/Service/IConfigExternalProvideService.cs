using Azrng.SettingConfig.Dto;

namespace Azrng.SettingConfig.Service
{
    public interface IConfigExternalProvideService
    {
        /// <summary>
        /// 根据key查询配置
        /// </summary>
        /// <param name="configKey"></param>
        /// <param name="throwError"></param>
        /// <returns></returns>
        Task<string> GetConfigContentAsync(string configKey, bool throwError = true);

        /// <summary>
        /// 根据key查询配置
        /// </summary>
        /// <param name="configKeys"></param>
        /// <param name="throwError"></param>
        /// <returns></returns>
        Task<List<GetConfigKeyAndValueDto>> GetConfigContentAsync(IEnumerable<string> configKeys,
            bool throwError = true);

        /// <summary>
        /// 根据key查询配置
        /// </summary>
        /// <param name="crConfigKey"></param>
        /// <param name="throwError"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> GetConfigAsync<T>(string crConfigKey, bool throwError = true);

        /// <summary>
        /// 更新系统配置表内容
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="updateUserId"></param>
        /// <returns></returns>
        Task<bool> UpdateConfigContentAsync(string key, string value, string updateUserId = null);

        /// <summary>
        /// 将不存在的配置添加到配置表中
        /// </summary>
        /// <param name="settingInfoDto"></param>
        /// <returns></returns>
        Task<bool> AddIfNotExistsAsync(List<AddSettingInfoDto> settingInfoDto);
    }
}