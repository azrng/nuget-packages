using Azrng.EFCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Common.EFCore.PostgreSql.Test;

public class UnitOrWorkTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitOrWorkTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// 单个上下文使用工作单元
    /// </summary>
    [Fact]
    public async Task SingleDbContextAdd()
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
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await using var tran = await unitOfWork.GetDatabase().BeginTransactionAsync();

        try
        {
            var list = new List<string>();
            {
                var content = Guid.NewGuid().ToString();
                await testRep.AddAsync(new TestEntity(content, "Test User 1", "test1@example.com", "Transaction test 1"));

                var flag = await unitOfWork.SaveChangesAsync();
                Assert.True(flag > 0);
                list.Add(content);
            }

            {
                var content = Guid.NewGuid().ToString();
                await testRep.AddAsync(new TestEntity(content, "Test User 2", "test2@example.com", "Transaction test 2"));

                var flag = await unitOfWork.SaveChangesAsync();
                Assert.True(flag > 0);
                list.Add(content);
            }
            await tran.CommitAsync();

            await testRep.DeleteAsync(t => list.Contains(t.Content));
        }
        catch (Exception ex)
        {
            await tran.RollbackAsync();
            _testOutputHelper.WriteLine(ex.Message);
        }
    }
}