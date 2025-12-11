using Azrng.SqlMigration;

namespace WebDbSqlMigrationSample;

public class DefaultMigrationHandler : IMigrationHandler
{
    private readonly ILogger<DefaultMigrationHandler> _logger;

    public DefaultMigrationHandler(ILogger<DefaultMigrationHandler> logger)
    {
        _logger = logger;
    }

    public Task<bool> BeforeMigrateAsync(string oloVersion)
    {
        _logger.LogInformation($"原始版本：{oloVersion}");
        return Task.FromResult(true);
    }

    public Task<bool> VersionUpdateBeforeMigrateAsync(string version)
    {
        _logger.LogInformation($"版本：{version}迁移前");
        return Task.FromResult(true);
    }

    public Task VersionUpdateMigratedAsync(string version)
    {
        _logger.LogInformation($"版本：{version}迁移成功后");
        return Task.FromResult(true);
    }

    public Task VersionUpdateMigrateFailedAsync(string version)
    {
        _logger.LogInformation($"版本：{version}迁移失败");
        return Task.FromResult(true);
    }

    public Task MigratedAsync(string oldVersion, string version)
    {
        _logger.LogInformation($"原始版本：{oldVersion} 当前版本：{version}迁移成功");
        return Task.FromResult(true);
    }

    public Task MigrateFailedAsync(string oldVersion,string version)
    {
        _logger.LogInformation($"原始版本：{oldVersion} 当前版本：{version}迁移失败");
        return Task.FromResult(true);
    }
}