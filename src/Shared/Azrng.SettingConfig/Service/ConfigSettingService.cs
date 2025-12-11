using Azrng.Core.Extension;
using Azrng.Core.Service;
using Azrng.SettingConfig.Dto;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Azrng.SettingConfig.Service;

/// <summary>
/// 内部业务逻辑
/// </summary>
internal class ConfigSettingService : BaseService, IConfigSettingService
{
    private readonly IDataSourceProvider _dataSourceProvider;
    private readonly ILogger<ConfigSettingService> _logger;
    private readonly IDistributedCache _cache;

    public ConfigSettingService(IDataSourceProvider dataSourceProvider,
        ILogger<ConfigSettingService> logger,
        IDistributedCache cache)
    {
        _dataSourceProvider = dataSourceProvider;
        _logger = logger;
        _cache = cache;
    }

    public async Task<GetSettingPageListResult> GetPageListAsync(GetSettingPageListRequest request)
    {
        var total = await _dataSourceProvider.GetConfigCount();
        var row = await _dataSourceProvider.GetPageListAsync(request.PageIndex, request.PageSize, request.Keyword,
            request.Version);

        return new GetSettingPageListResult(total, row);
    }

    public async Task<IResultModel<GetConfigDetailsResult>> GetDetailsConfigIdAsync(int configId)
    {
        var config = await _dataSourceProvider.GetConfigDetails(configId);
        if (config is not null) return Success(config);
        _logger.LogError($"获取配置详情 配置标识无效：{configId}");
        return Error<GetConfigDetailsResult>("配置标识无效");
    }

    public async Task<IResultModel<bool>> UpdateConfigDetailsAsync(UpdateConfigDetailsRequest request)
    {
        try
        {
            var key = await _dataSourceProvider.GetConfigKeyAsync(request.ConfigId);
            if (key.IsNullOrWhiteSpace())
                return Error<bool>("配置版本标识无效");

            // 清除缓存
            var cacheKey = SettingConfigConst.ConfigPrefix + key;
            await _cache.RemoveAsync(cacheKey);
            var flag = await _dataSourceProvider.UpdateConfigVersionAsync(request.ConfigId, request.Value,
                request.Description, string.Empty);
            return flag ? Success(true) : Error<bool>("更新失败");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"更新版本内容报错  message：{e.Message}");
            return Error<bool>("更新失败");
        }
    }

    public Task<List<GetConfigVersionListResult>> GetConfigHistoryListAsync(string key)
    {
        return _dataSourceProvider.GetConfigHistoryListAsync(key);
    }

    public async Task<IResultModel<bool>> DeleteConfigAsync(int configId)
    {
        if (configId == 0)
            return Error<bool>("版本ID无效");
        try
        {
            var config = await _dataSourceProvider.GetConfigInfoAsync(configId);
            if (config is null)
                return Error<bool>("配置标识无效");

            var key = SettingConfigConst.ConfigPrefix + config.Key;
            await _cache.RemoveAsync(key);
            await _dataSourceProvider.DeleteConfigAsync(configId);
            return Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"删除配置报错：{ex.Message} configId:{configId}");
            return Error<bool>("删除配置报错");
        }
    }

    public async Task<IResultModel<bool>> RestoreConfigAsync(int historyId)
    {
        try
        {
            var result = await _dataSourceProvider.RestoreConfigAsync(historyId).ConfigureAwait(false);
            return Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "还原配置信息出错 message:{ExMessage} stackTrace:{ExStackTrace}", ex.Message,
                ex.StackTrace);
            throw new ArgumentException("还原配置信息出错");
        }
    }
}