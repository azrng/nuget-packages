using Azrng.Core.Exceptions;
using Azrng.Core.Extension;
using Azrng.SettingConfig.Dto;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Azrng.SettingConfig.Service
{
    public class ConfigExternalProvideService : IConfigExternalProvideService
    {
        private readonly IDataSourceProvider _dataSourceProvider;
        private readonly ILogger<ConfigExternalProvideService> _logger;
        private readonly IDistributedCache _cache;
        private readonly DashboardOptions _config;

        public ConfigExternalProvideService(IDataSourceProvider dataSourceProvider,
            IOptions<DashboardOptions> options,
            ILogger<ConfigExternalProvideService> logger, IDistributedCache cache)
        {
            _dataSourceProvider = dataSourceProvider;
            _logger = logger;
            _cache = cache;
            _config = options.Value;
        }

        public async Task<string> GetConfigContentAsync(string configKey, bool throwError = true)
        {
            var key = SettingConfigConst.ConfigPrefix + configKey;
            var crConfigContent = await _cache.GetStringAsync(key).ConfigureAwait(false);
            if (crConfigContent != null)
                return crConfigContent;

            var crConfig = await _dataSourceProvider.GetConfigValueAsync(configKey)
                .ConfigureAwait(false);
            if (crConfig.IsNullOrWhiteSpace())
            {
                _logger.LogError("根据当前key{Key}没有查询到配置信息", key);
                return null;
            }

            await _cache.SetStringAsync(key, crConfig,
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_config.ConfigCacheTime) })
                .ConfigureAwait(false);
            return crConfig;
        }

        public async Task<List<GetConfigKeyAndValueDto>> GetConfigContentAsync(IEnumerable<string> configKeys,
            bool throwError = true)
        {
            var result = new List<GetConfigKeyAndValueDto>();
            foreach (var key in configKeys)
            {
                var value = await GetConfigContentAsync(key, throwError);
                if (value.IsNotNullOrEmpty())
                {
                    result.Add(new GetConfigKeyAndValueDto(key, value));
                }
            }

            return result;
        }

        public async Task<T> GetConfigAsync<T>(string configKey, bool throwError = true)
        {
            try
            {
                var content = await GetConfigContentAsync(configKey, throwError).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(content)) return JsonConvert.DeserializeObject<T>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取配置信息进行反序列化出错 key：{Key} message:{ExMessage} stackTrace:{ExStackTrace}",
                    configKey,
                    ex.Message,
                    ex.StackTrace);

                if (ex is NotFoundException)
                {
                    throw;
                }

                if (throwError)
                {
                    throw new ArgumentException("获取配置信息出错");
                }
            }

            return default;
        }

        public Task<bool> UpdateConfigContentAsync(string key, string value, string updateUserId = null)
        {
            return _dataSourceProvider.UpdateConfigValueAsync(key, value, updateUserId);
        }

        public Task<bool> AddIfNotExistsAsync(List<AddSettingInfoDto> settingInfoDto)
        {
            return _dataSourceProvider.AddConfigListAsync(settingInfoDto);
        }
    }
}