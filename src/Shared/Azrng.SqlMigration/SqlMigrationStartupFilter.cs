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
    private readonly Func<IServiceProvider, Task<IAsyncDisposable?>>? _lockProvider;

    public SqlMigrationStartupFilter(ILogger<SqlMigrationStartupFilter> logger, IOptions<SqlMigrationOption> option,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _lockProvider = option.Value?.LockProvider;
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

        // 循环执行迁移操作
        foreach (var migrationName in SqlMigrationServiceExtension.DbNames)
        {
            SqlMigrationServiceExtension.CurrDbName = migrationName;

            var migrateService = scope.ServiceProvider.GetRequiredKeyedService<ISqlMigrationService>(migrationName);
            if (_lockProvider != null)
            {
                await using var _ = await _lockProvider(scope.ServiceProvider);
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