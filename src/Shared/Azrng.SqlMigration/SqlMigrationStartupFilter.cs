using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azrng.SqlMigration;

/// <summary>
/// 执行sql迁移过滤器
/// </summary>
public class SqlMigrationStartupFilter : IStartupFilter
{
    private readonly ILogger<SqlMigrationStartupFilter> _logger;
    private readonly IServiceProvider _serviceProvider;

    public SqlMigrationStartupFilter(ILogger<SqlMigrationStartupFilter> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            ExecuteAsync().GetAwaiter().GetResult();
            next(builder);
        };
    }

    private async Task ExecuteAsync()
    {
        _logger.LogInformation("版本升级开始");

        using var scope = _serviceProvider.CreateScope();
        var optionsSnapshot = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SqlMigrationOption>>();

        // 循环执行迁移操作
        foreach (var migrationName in SqlMigrationServiceExtension.DbNames)
        {
            var migrateService = scope.ServiceProvider.GetRequiredKeyedService<ISqlMigrationService>(migrationName);
            var config = optionsSnapshot.Get(migrationName);

            if (config.LockProvider != null)
            {
                await using var _ = await config.LockProvider(scope.ServiceProvider);
                await migrateService.MigrateAsync(migrationName);
            }
            else
            {
                await migrateService.MigrateAsync(migrationName);
            }
        }

        _logger.LogInformation("版本升级结束");
    }
}
