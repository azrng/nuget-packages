using Azrng.SettingConfig.Dto;

namespace Azrng.SettingConfig.Service;

/// <summary>
/// 数据存储提供者接口
/// </summary>
public interface IDataSourceProvider
{
    /// <summary>
    /// 初始化数据库
    /// </summary>
    /// <returns></returns>
    Task<bool> InitAsync();

    /// <summary>
    /// 查询配置分页列表
    /// </summary>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">页大小</param>
    /// <param name="keyword">关键字</param>
    /// <param name="version">版本</param>
    /// <returns></returns>
    Task<List<GetSettingInfoDto>> GetPageListAsync(int pageIndex, int pageSize, string keyword,
        string version);

    /// <summary>
    /// 查询配置总数
    /// </summary>
    /// <returns></returns>
    Task<int> GetConfigCount();

    /// <summary>
    /// 根据配置id查询启用的版本详情
    /// </summary>
    /// <param name="configId"></param>
    /// <returns></returns>
    Task<GetConfigDetailsResult> GetConfigDetails(int configId);

    /// <summary>
    /// 获取配置信息
    /// </summary>
    /// <param name="configId"></param>
    /// <returns></returns>
    Task<GetConfigInfoDto> GetConfigInfoAsync(int configId);

    /// <summary>
    /// 获取配置key
    /// </summary>
    /// <param name="configVersionId"></param>
    /// <returns></returns>
    Task<string> GetConfigKeyAsync(int configVersionId);

    /// <summary>
    /// 更新配置版本详情
    /// </summary>
    /// <param name="versionId">版本记录表ID</param>
    /// <param name="value">配置值</param>
    /// <param name="description">配置说明</param>
    /// <param name="updateUserId"></param>
    /// <returns></returns>
    Task<bool> UpdateConfigVersionAsync(int versionId, string value, string description,
        string updateUserId);

    /// <summary>
    /// 查询配置版本列表
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<List<GetConfigVersionListResult>> GetConfigHistoryListAsync(string key);

    /// <summary>
    /// 删除指定配置
    /// </summary>
    /// <param name="configId"></param>
    /// <returns></returns>
    Task<bool> DeleteConfigAsync(int configId);

    /// <summary>
    /// 还原配置
    /// </summary>
    /// <param name="historyId"></param>
    /// <returns></returns>
    Task<bool> RestoreConfigAsync(int historyId);

    /// <summary>
    /// 根据key查询配置内容
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<string> GetConfigValueAsync(string key);

    /// <summary>
    /// 更新配置
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="updateUserId"></param>
    /// <returns></returns>
    Task<bool> UpdateConfigValueAsync(string key, string value, string updateUserId = null);

    /// <summary>
    /// 添加配置
    /// </summary>
    /// <param name="addSettingInfos"></param>
    /// <returns></returns>
    Task<bool> AddConfigListAsync(List<AddSettingInfoDto> addSettingInfos);
}