using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.EFCore.PostgreSql.Test;

public class ProviderLoggingTests
{
    [Fact]
    public void AddEntityFramework_ShouldUseHostLoggerFactory()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddEntityFramework<TestDbContext>(options =>
        {
            options.ConnectionString = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test;port=5432";
            options.Schema = "public";
        });

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        var configuredLoggerFactory = GetConfiguredLoggerFactory(dbContext);

        Assert.Same(loggerFactory, configuredLoggerFactory);
    }

    [Fact]
    public void AddEntityFrameworkFactory_ShouldUseHostLoggerFactory()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddEntityFrameworkFactory<TestDbContext>(options =>
        {
            options.ConnectionString = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test;port=5432";
            options.Schema = "public";
        });

        using var serviceProvider = services.BuildServiceProvider();
        using var dbContext = serviceProvider.GetRequiredService<IDbContextFactory<TestDbContext>>().CreateDbContext();

        var configuredLoggerFactory = GetConfiguredLoggerFactory(dbContext);

        Assert.Same(loggerFactory, configuredLoggerFactory);
    }

    private static ILoggerFactory? GetConfiguredLoggerFactory(DbContext dbContext)
    {
        var options = dbContext.GetService<IDbContextOptions>();
        return options.Extensions.OfType<CoreOptionsExtension>().Single().LoggerFactory;
    }
}
