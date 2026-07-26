using Azrng.DistributeLock.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azrng.DistributeLock.PostgreSql;

/// <summary>
/// 数据库分布式锁
/// </summary>
public class DbLockProvider : ILockProvider
{
    private readonly DbLockDataSourceProvider _dbLockDataSourceProvider;
    private readonly ILogger<DbLockProvider> _logger;
    private readonly DbLockOptions _options;

    public DbLockProvider(IOptions<DbLockOptions> options, ILogger<DbLockProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
        _dbLockDataSourceProvider =
            new DbLockDataSourceProvider(_options.ConnectionString, _options.Schema, _options.Table);

        try
        {
            _dbLockDataSourceProvider.Init();
        }
        catch (Exception ex)
        {
            // 不输出连接字符串，避免泄漏密码
            _logger.LogError(ex, "Db分布式锁初始化失败。schema:{Schema}, table:{Table}", _options.Schema, _options.Table);
            throw;
        }
    }

    public async Task<LockInstance?> LockAsync(string lockKey, TimeSpan? expire = null,
        TimeSpan? getLockTimeOut = null, bool autoExtend = true)
    {
        var lockValue = Guid.NewGuid().ToString();
        expire ??= _options.DefaultExpireTime;
        getLockTimeOut ??= TimeSpan.FromSeconds(5);

        var lockData = new LockInstance(_dbLockDataSourceProvider, lockKey, lockValue, _logger, autoExtend,expire.Value);

        var flag = await lockData.LockAsync(expire.Value, getLockTimeOut.Value);
        return flag ? lockData : null;
    }
}