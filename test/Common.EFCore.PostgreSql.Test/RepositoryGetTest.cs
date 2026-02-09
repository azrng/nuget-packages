using Azrng.EFCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.EFCore.PostgreSql.Test;

public class RepositoryGetTest
{
    /// <summary>
    /// 单个上下文不使用工作单元
    /// </summary>
    [Fact]
    public async Task SingleDbContext()
    {
        var connectionStr = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test;port=5432";
        var service = new ServiceCollection();
        service.AddLogging(loggerBuilder =>
        {
            loggerBuilder.AddConsole();
        });
        service.AddEntityFramework<TestDbContext>(options =>
        {
            options.ConnectionString = connectionStr;
            options.Schema = "public";
        });
        await using var serviceProvider = service.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var testRep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity>>();
        var content = Guid.NewGuid().ToString();
        await testRep.AddAsync(new TestEntity(content, "Test User", "test@example.com", "Test description"), true);

        var count = await testRep.CountAsync(t => true);
        Assert.True(count > 0);

        await testRep.DeleteAsync(t => t.Content == content);
    }

    /// <summary>
    /// 多个上下文不使用工作单元
    /// </summary>
    [Fact]
    public async Task MultiDbContext()
    {
        var connectionStr = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test;port=5432";
        var connection2Str = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test2;port=5432";
        var service = new ServiceCollection();
        service.AddLogging(loggerBuilder =>
        {
            loggerBuilder.AddConsole();
        });
        service.AddEntityFramework<TestDbContext>(options =>
        {
            options.ConnectionString = connectionStr;
            options.Schema = "public";
        });
        service.AddEntityFramework<TestDb2Context>(options =>
        {
            options.ConnectionString = connection2Str;
            options.Schema = "public";
        });
        await using var serviceProvider = service.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        {
            var testRep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity, TestDbContext>>();
            var content = Guid.NewGuid().ToString();
            await testRep.AddAsync(new TestEntity(content, "Test User 1", "test1@example.com", "Database 1 test"), true);

            var count = await testRep.CountAsync(t => true);
            Assert.True(count > 0);

            await testRep.DeleteAsync(t => t.Content == content);
        }

        {
            var testRep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity, TestDb2Context>>();
            var content = Guid.NewGuid().ToString();
            await testRep.AddAsync(new TestEntity(content, "Test User 2", "test2@example.com", "Database 2 test"), true);

            var count = await testRep.CountAsync(t => true);
            Assert.True(count > 0);

            await testRep.DeleteAsync(t => t.Content == content);
        }
    }
}