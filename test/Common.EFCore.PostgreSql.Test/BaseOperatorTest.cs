using Azrng.EFCore;
using Common.Extension;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Common.EFCore.PostgreSql.Test;

public class BaseOperatorTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _connectionStr;

    public BaseOperatorTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _connectionStr = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test;port=5432";
    }

    /// <summary>
    /// 更新一键更新
    /// </summary>
    [Fact]
    public async Task BatchUpdate_Test()
    {
        var service = new ServiceCollection();
        service.AddLogging(loggerBuilder =>
        {
            loggerBuilder.AddConsole();
        });
        service.AddEntityFramework<TestDbContext>(options =>
        {
            options.ConnectionString = _connectionStr;
            options.Schema = "public";
        });
        await using var serviceProvider = service.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var testRep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity>>();
        var list = await testRep.EntitiesNoTacking.Where(t => t.Content.Contains("完成"))
                                .ToListAsync();
        list.ForEach(x => x.CreatedTime = DateTime.UtcNow.AddHours(1));
        await testRep.UpdateAsync(list, true);
    }

    /// <summary>
    /// 更新一键更新
    /// </summary>
    [Fact]
    public async Task Batch_SetProperty_Update_Test()
    {
        var service = new ServiceCollection();
        service.AddLogging(loggerBuilder =>
        {
            loggerBuilder.AddConsole();
        });
        service.AddEntityFramework<TestDbContext>(options =>
        {
            options.ConnectionString = _connectionStr;
            options.Schema = "public";
        });
        await using var serviceProvider = service.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var testRep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity>>();

        var time = DateTime.UtcNow.AddHours(1);

        var list = await testRep.UpdateAsync(x => x.Content.Contains("完成"),
            x => x.SetProperty(t => t.CreatedTime, time));
        _testOutputHelper.WriteLine(list.ToString());
    }
}