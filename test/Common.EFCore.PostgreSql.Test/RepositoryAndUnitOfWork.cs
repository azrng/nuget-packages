using Azrng.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.EFCore.PostgreSql.Test;

public class RepositoryAndUnitOfWork
{
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
        var content = Guid.NewGuid().ToString();
        await testRep.AddAsync(new TestEntity(content, "Test User", "test@example.com", "Single database test"));

        var flag = await unitOfWork.SaveChangesAsync();
        Assert.True(flag > 0);

        await testRep.DeleteAsync(t => t.Content == content);
    }

    /// <summary>
    /// 指定上下文添加
    /// </summary>
    [Fact]
    public async Task SpecifyDbContextAdd()
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
               })
               .AddUnitOfWork<TestDbContext>();

        await using var serviceProvider = service.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var testRep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity, TestDbContext>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var unitOfWork2 = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TestDbContext>>();
        var content = Guid.NewGuid().ToString();
        await testRep.AddAsync(new TestEntity(content, "Test User", "test@example.com", "Specify context test"));

        // IBaseRepository<TEntity, TDbContext> 和 IUnitOfWork 现在共享同一个 DbContext 实例
        var flag = await unitOfWork.SaveChangesAsync();
        Assert.True(flag > 0);

        // 同一 DbContext 已提交，二次提交无待保存变更
        var flag2 = await unitOfWork2.SaveChangesAsync();
        Assert.True(flag2 == 0);

        await testRep.DeleteAsync(t => t.Content == content);
    }

    /// <summary>
    /// 单个上下文使用指定上下文的工作单元
    /// </summary>
    [Fact]
    public async Task SingleDbContextGetRepository()
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
               })
               .AddUnitOfWork<TestDbContext>();
        await using var serviceProvider = service.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TestDbContext>>();
        var testRep = unitOfWork.GetRepository<TestEntity>();
        var content = Guid.NewGuid().ToString();
        await testRep.AddAsync(new TestEntity(content, "Test User", "test@example.com", "Get repository test"));
        var flag = await unitOfWork.SaveChangesAsync();
        Assert.True(flag > 0);

        var count = await testRep.CountAsync(t => true);
        Assert.True(count > 0);

        await testRep.DeleteAsync(t => t.Content == content);
    }

    /// <summary>
    /// 多个上下文并且使用工作单元
    /// </summary>
    [Fact]
    public async Task MultiContextUseUnitOfWorkGetResp()
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
               })
               .AddUnitOfWork<TestDbContext>();

        service.AddEntityFramework<TestDb2Context>(options =>
               {
                   options.ConnectionString = connection2Str;
                   options.Schema = "public";
               })
               .AddUnitOfWork<TestDb2Context>();
        await using var serviceProvider = service.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TestDbContext>>();
            var testDb1Rep = unitOfWork.GetRepository<TestEntity>();
            var content = Guid.NewGuid().ToString();
            await testDb1Rep.AddAsync(new TestEntity(content, "Test User 1", "test1@example.com", "Multi-context database 1"));
            var flag = await unitOfWork.SaveChangesAsync();
            Assert.True(flag > 0);

            var count = await testDb1Rep.CountAsync(t => true);
            Assert.True(count > 0);

            await testDb1Rep.DeleteAsync(t => t.Content == content);
        }

        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TestDb2Context>>();
            var testDb2Rep = unitOfWork.GetRepository<TestEntity>();
            var content = Guid.NewGuid().ToString();
            await testDb2Rep.AddAsync(new TestEntity(content, "Test User 2", "test2@example.com", "Multi-context database 2"));
            var flag = await unitOfWork.SaveChangesAsync();
            Assert.True(flag > 0);

            var count = await testDb2Rep.CountAsync(t => true);
            Assert.True(count > 0);

            await testDb2Rep.DeleteAsync(t => t.Content == content);
        }
    }

    /// <summary>
    /// 多个上下文并且使用工作单元
    /// </summary>
    [Fact]
    public async Task MultiContextUseUnitOfWork()
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
               })
               .AddUnitOfWork<TestDbContext>();

        service.AddEntityFramework<TestDb2Context>(options =>
               {
                   options.ConnectionString = connection2Str;
                   options.Schema = "public";
               })
               .AddUnitOfWork<TestDb2Context>();
        await using var serviceProvider = service.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TestDbContext>>();
            var testDb1Rep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity, TestDbContext>>();
            var content = Guid.NewGuid().ToString();
            await testDb1Rep.AddAsync(new TestEntity(content, "Test User 1", "test1@example.com", "Multi-context DB1 test"));
            var flag = await unitOfWork.SaveChangesAsync();
            Assert.True(flag > 0);

            var count = await testDb1Rep.CountAsync(t => true);
            Assert.True(count > 0);

            await testDb1Rep.DeleteAsync(t => t.Content == content);
        }

        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TestDb2Context>>();
            var testDb2Rep = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity, TestDb2Context>>();
            var content = Guid.NewGuid().ToString();
            await testDb2Rep.AddAsync(new TestEntity(content, "Test User 2", "test2@example.com", "Multi-context DB2 test"));
            var flag = await unitOfWork.SaveChangesAsync();
            Assert.True(flag > 0);

            var count = await testDb2Rep.CountAsync(t => true);
            Assert.True(count > 0);

            await testDb2Rep.DeleteAsync(t => t.Content == content);
        }
    }
}