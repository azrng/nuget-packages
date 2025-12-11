using Azrng.SettingConfig.Dto;

namespace Azrng.SettingConfig.Service;

/// <summary>
/// 获取配置服务
/// </summary>
public interface IConfigSettingService
{
    /// <summary>
    /// 获取分页列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<GetSettingPageListResult> GetPageListAsync(GetSettingPageListRequest request);

    /// <summary>
    /// 根据配置id查询启用版本的详情
    /// </summary>
    /// <param name="configId"></param>
    /// <returns></returns>
    Task<IResultModel<GetConfigDetailsResult>> GetDetailsConfigIdAsync(int configId);

    /// <summary>
    /// 更新配置详情
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<IResultModel<bool>> UpdateConfigDetailsAsync(UpdateConfigDetailsRequest request);

    /// <summary>
    /// 根据配置key查询配置历史列表
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<List<GetConfigVersionListResult>> GetConfigHistoryListAsync(string key);

    /// <summary>
    /// 删除指定配置(逻辑删除)
    /// </summary>
    /// <param name="configId"></param>
    /// <returns></returns>
    Task<IResultModel<bool>> DeleteConfigAsync(int configId);

    /// <summary>
    /// 还原配置
    /// </summary>
    /// <param name="historyId"></param>
    /// <returns></returns>
    Task<IResultModel<bool>> RestoreConfigAsync(int historyId);
}