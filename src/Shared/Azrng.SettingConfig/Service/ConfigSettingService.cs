using Azrng.Core.Extension;
using Azrng.Core.Service;
using Azrng.SettingConfig.Dto;
using Azrng.SettingConfig.Interface;
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
    private readonly IConnectInterface _connectInterface;

    public ConfigSettingService(IDataSourceProvider dataSourceProvider,
                                ILogger<ConfigSettingService> logger,
                                IDistributedCache cache,
                                IConnectInterface connectInterface)
    {
        _dataSourceProvider = dataSourceProvider;
        _logger = logger;
        _cache = cache;
        _connectInterface = connectInterface;
    }

    public async Task<GetSettingPageListResult> GetPageListAsync(GetSettingPageListRequest request)
    {
        var total = await _dataSourceProvider.GetConfigCount();
        var row = await _dataSourceProvider.GetPageListAsync(request.PageIndex, request.PageSize, request.Keyword,
            request.Version);

        return new GetSettingPageListResult(total, row ?? new List<GetSettingInfoDto>());
    }

    public async Task<IResultModel<GetConfigDetailsResult>> GetDetailsConfigIdAsync(int configId)
    {
        var config = await _dataSourceProvider.GetConfigDetails(configId);
        if (config is not null) return Success(config);
        _logger.LogError($"获取配置详情 配置标识无效：{configId}");
        return Fail<GetConfigDetailsResult>("配置标识无效");
    }

    public async Task<IResultModel<bool>> UpdateConfigDetailsAsync(UpdateConfigDetailsRequest request)
    {
        try
        {
            var key = await _dataSourceProvider.GetConfigKeyAsync(request.ConfigId);
            if (key.IsNullOrWhiteSpace())
                return Fail<bool>("配置版本标识无效");

            // 编辑前回调：配置项业务校验
            var (isValid, errMsg) = await _connectInterface.ItemValidate(request);
            if (!isValid)
            {
                _logger.LogWarning($"配置校验失败：{errMsg}，ConfigId:{request.ConfigId}");
                return Fail<bool>(errMsg);
            }

            // 清除缓存
            var cacheKey = SettingConfigConst.ConfigPrefix + key;
            await _cache.RemoveAsync(cacheKey);
            var flag = await _dataSourceProvider.UpdateConfigVersionAsync(request.ConfigId, request.Value,
                request.Description, string.Empty);

            if (!flag)
                return Fail<bool>("更新失败");

            // 编辑成功后回调
            try
            {
                var (isOk, successErrMsg) = await _connectInterface.EditSuccessHandle(key, request.Value);
                if (!isOk)
                {
                    _logger.LogError($"配置编辑成功回调处理失败：{successErrMsg}，ConfigId:{request.ConfigId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"配置编辑成功回调执行异常：{ex.Message}，ConfigId:{request.ConfigId}");
            }

            return Success(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"更新版本内容报错  message：{e.Message}");
            return Fail<bool>("更新失败");
        }
    }

    public async Task<List<GetConfigVersionListResult>> GetConfigHistoryListAsync(string key)
    {
        return await _dataSourceProvider.GetConfigHistoryListAsync(key) ?? new List<GetConfigVersionListResult>();
    }

    public async Task<IResultModel<bool>> DeleteConfigAsync(int configId)
    {
        if (configId == 0)
            return Fail<bool>("版本ID无效");
        try
        {
            var config = await _dataSourceProvider.GetConfigInfoAsync(configId);
            if (config is null)
                return Fail<bool>("配置标识无效");

            // 清除缓存
            var cacheKey = SettingConfigConst.ConfigPrefix + config.Key;
            await _cache.RemoveAsync(cacheKey);

            // 删除配置
            await _dataSourceProvider.DeleteConfigAsync(configId);

            // 删除成功后回调
            try
            {
                var (isOk, successErrMsg) = await _connectInterface.DeleteSuccessHandle(config.Key, config.Value);
                if (!isOk)
                {
                    _logger.LogError($"删除配置回调处理失败：{successErrMsg}，ConfigId:{configId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除配置回调执行异常：{ex.Message}，ConfigId:{configId}");
            }

            return Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"删除配置报错：{ex.Message} configId:{configId}");
            return Fail<bool>("删除配置报错");
        }
    }

    public async Task<IResultModel<bool>> RestoreConfigAsync(int historyId)
    {
        try
        {
            // 获取历史记录信息
            var historyInfo = await _dataSourceProvider.GetHistoryInfoAsync(historyId);
            if (historyInfo == null)
            {
                _logger.LogError($"历史记录不存在：{historyId}");
                return Fail<bool>("历史记录不存在");
            }

            // 清除缓存
            var cacheKey = SettingConfigConst.ConfigPrefix + historyInfo.Key;
            await _cache.RemoveAsync(cacheKey);

            // 恢复配置
            var result = await _dataSourceProvider.RestoreConfigAsync(historyId).ConfigureAwait(false);

            if (!result)
                return Fail<bool>("恢复配置失败");

            // 恢复成功后回调
            try
            {
                var (isOk, errMsg) = await _connectInterface.EditSuccessHandle(historyInfo.Key, historyInfo.Value);
                if (!isOk)
                {
                    _logger.LogError($"还原配置回调处理失败：{errMsg}，HistoryId:{historyId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"还原配置回调执行异常：{ex.Message}，HistoryId:{historyId}");
            }

            return Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "还原配置信息出错 message:{ExMessage} stackTrace:{ExStackTrace}", ex.Message,
                ex.StackTrace);
            return Fail<bool>("还原配置信息出错");
        }
    }
}