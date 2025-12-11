using Azrng.SettingConfig.Attributes;
using Azrng.SettingConfig.Dto;
using Azrng.SettingConfig.Service;
using Microsoft.AspNetCore.Mvc;

namespace Azrng.SettingConfig.Controller;

/// <summary>
/// 配置控制器
/// </summary>
[ApiController]
[SettingMatchRoute]
public class SystemSettingController : ControllerBase
{
    private readonly IConfigSettingService _configSettingService;

    public SystemSettingController(IConfigSettingService configSettingService)
    {
        _configSettingService = configSettingService;
    }

    /// <summary>
    /// 获取分页列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("page")]
    public Task<GetSettingPageListResult> GetPageListAsync(
        [FromQuery] GetSettingPageListRequest request)
    {
        return _configSettingService.GetPageListAsync(request);
    }

    /// <summary>
    /// 根据配置id查询启用版本的详情
    /// </summary>
    /// <param name="configId"></param>
    /// <returns></returns>
    [HttpGet("{configId:int}/details")]
    public Task<IResultModel<GetConfigDetailsResult>> GetDetailsConfigIdAsync(int configId)
    {
        return _configSettingService.GetDetailsConfigIdAsync(configId);
    }

    /// <summary>
    /// 更新配置启用版本详情
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("details")]
    public Task<IResultModel<bool>> UpdateConfigDetailsAsync(
        [FromBody] UpdateConfigDetailsRequest request)
    {
        return _configSettingService.UpdateConfigDetailsAsync(request);
    }

    /// <summary>
    /// 根据配置key查询配置历史列表
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpGet("history/list/{key}")]
    public Task<List<GetConfigVersionListResult>> GetConfigHistoryListAsync([FromRoute] string key)
    {
        return _configSettingService.GetConfigHistoryListAsync(key);
    }

    /// <summary>
    /// 删除指定配置(逻辑删除)
    /// </summary>
    /// <param name="configId"></param>
    /// <returns></returns>
    [HttpDelete("{configId:int}")]
    public Task<IResultModel<bool>> DeleteConfigAsync([FromRoute] int configId)
    {
        return _configSettingService.DeleteConfigAsync(configId);
    }

    /// <summary>
    /// 还原配置信息
    /// </summary>
    /// <param name="historyId"></param>
    /// <returns></returns>
    [HttpPut("restore/{historyId:int}")]
    public Task<IResultModel<bool>> RestoreConfig(int historyId)
    {
        return _configSettingService.RestoreConfigAsync(historyId);
    }
}