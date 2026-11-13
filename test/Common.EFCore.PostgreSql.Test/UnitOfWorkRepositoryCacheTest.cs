using Azrng.EFCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.EFCore.PostgreSql.Test;

/// <summary>
/// 仓储缓存生命周期回归测试：仓储持有 scoped 的 DbContext，缓存必须随工作单元实例走，
/// 不能跨 Scope 复用（否则会拿到已释放的上下文）
/// </summary>
public class UnitOfWorkRepositoryCacheTest
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
    /// 前一个 Scope 结束后，新 Scope 通过工作单元获取的仓储必须绑定新的 DbContext 并可正常查询
    /// </summary>
    [Fact]
    public async Task GetRepository_AcrossScopes_WorkAfterPreviousScopeDisposed()
    {
        await using var provider = BuildProvider();

        using (var scope1 = provider.CreateScope())
        {
            var unitOfWork1 = scope1.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository1 = unitOfWork1.GetRepository<TestEntity>();
            // 触发一次真实查询，确保仓储被实际使用并缓存
            Assert.True(await repository1.CountAsync(t => true) >= 0);
        }

        // scope1 的 DbContext 已随 Scope 释放；仓储若被全局缓存，这里会拿到已释放的上下文
        using (var scope2 = provider.CreateScope())
        {
            var unitOfWork2 = scope2.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository2 = unitOfWork2.GetRepository<TestEntity>();
            Assert.True(await repository2.CountAsync(t => true) >= 0);
        }
    }

    /// <summary>
    /// 同一工作单元内多次获取同一实体的仓储，应命中缓存返回同一实例
    /// </summary>
    [Fact]
    public void GetRepository_SameUnitOfWork_ReturnsSameInstance()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        Assert.Same(unitOfWork.GetRepository<TestEntity>(), unitOfWork.GetRepository<TestEntity>());
    }

    /// <summary>
    /// 两个并存 Scope 并发使用各自的仓储互不干扰
    /// </summary>
    [Fact]
    public async Task GetRepository_ConcurrentScopes_Isolated()
    {
        await using var provider = BuildProvider();

        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var unitOfWork1 = scope1.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var unitOfWork2 = scope2.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var repository1 = unitOfWork1.GetRepository<TestEntity>();
        var repository2 = unitOfWork2.GetRepository<TestEntity>();

        var results = await Task.WhenAll(
            repository1.CountAsync(t => true),
            repository2.CountAsync(t => true));

        Assert.True(results[0] >= 0);
        Assert.True(results[1] >= 0);
    }
}
