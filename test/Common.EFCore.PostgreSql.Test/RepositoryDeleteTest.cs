using Azrng.EFCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.EFCore.PostgreSql.Test;

/// <summary>
/// 条件删除回归测试：DeleteAsync(expression) 应返回实际删除行数，
/// 且不能顺带提交上下文中其他未保存的挂起更改
/// </summary>
public class RepositoryDeleteTest
{
    private const string ConnectionStr = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test;port=5432";

    private static ServiceProvider BuildProvider()
    {
        var service = new ServiceCollection();
        service.AddLogging(builder => builder.AddConsole());
        service.AddEntityFramework<TestDbContext>(options =>
        {
            options.ConnectionString = ConnectionStr;
            options.Schema = "public";
        });
        return service.BuildServiceProvider();
    }

    /// <summary>
    /// 条件删除的返回值必须是删除的行数（而非后续 SaveChanges 的行数）
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ReturnsActualDeletedRowCount()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity>>();

        var content = Guid.NewGuid().ToString();
        await repository.AddAsync(new TestEntity(content, "Delete Test 1"), true);
        await repository.AddAsync(new TestEntity(content, "Delete Test 2"), true);
        await repository.AddAsync(new TestEntity(content, "Delete Test 3"), true);

        var deleted = await repository.DeleteAsync(t => t.Content == content);
        Assert.Equal(3, deleted);
        Assert.Equal(0, await repository.CountAsync(t => t.Content == content));
    }

    /// <summary>
    /// 条件删除不应把上下文中未提交的其他更改一并保存到数据库
    /// </summary>
    [Fact]
    public async Task DeleteAsync_DoesNotFlushUnrelatedPendingChanges()
    {
        var pendingContent = Guid.NewGuid().ToString();
        var deleteContent = Guid.NewGuid().ToString();

        await using (var provider = BuildProvider())
        using (var scope = provider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity>>();

            // 准备一条待删除数据（已提交）
            await repository.AddAsync(new TestEntity(deleteContent, "Pending Delete"), true);

            // 挂起一条未提交的新增
            await repository.AddAsync(new TestEntity(pendingContent, "Pending Add"), false);

            var deleted = await repository.DeleteAsync(t => t.Content == deleteContent);
            Assert.Equal(1, deleted);
        }

        // 用全新 Scope 验证挂起的新增没有被条件删除顺带提交
        await using (var provider = BuildProvider())
        using (var scope = provider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity>>();
            Assert.Equal(0, await repository.CountAsync(t => t.Content == pendingContent));
        }
    }
}
