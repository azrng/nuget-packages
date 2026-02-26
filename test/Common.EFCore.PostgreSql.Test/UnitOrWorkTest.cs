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

    /// <summary>
    /// 测试 BeginTransactionScopeAsync - 正常提交成功
    /// </summary>
    [Fact]
    public async Task BeginTransactionScopeAsync_Commit_Success()
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

        var contentList = new List<string>();

        await using var tranScope = await unitOfWork.BeginTransactionScopeAsync();
        try
        {
            // 添加第一条数据
            var content1 = Guid.NewGuid().ToString();
            await testRep.AddAsync(new TestEntity(content1, "Test User 1", "test1@example.com", "TransactionScope test 1"));
            contentList.Add(content1);

            // 添加第二条数据
            var content2 = Guid.NewGuid().ToString();
            await testRep.AddAsync(new TestEntity(content2, "Test User 2", "test2@example.com", "TransactionScope test 2"));
            contentList.Add(content2);

            // 提交事务
            await tranScope.CommitAsync();

            // 验证数据已保存
            var savedEntities = await testRep.GetListAsync(t => contentList.Contains(t.Content));
            Assert.Equal(2, savedEntities.Count);

            // 清理测试数据
            await testRep.DeleteAsync(t => contentList.Contains(t.Content));
        }
        catch (Exception ex)
        {
            await tranScope.RollbackAsync();
            _testOutputHelper.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 测试 BeginTransactionScopeAsync - 异常时回滚
    /// </summary>
    [Fact]
    public async Task BeginTransactionScopeAsync_Rollback_OnException()
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

        var contentList = new List<string>();

        await using var tranScope = await unitOfWork.BeginTransactionScopeAsync();
        try
        {
            // 添加数据
            var content1 = Guid.NewGuid().ToString();
            await testRep.AddAsync(new TestEntity(content1, "Test User 1", "test1@example.com", "TransactionScope rollback test"));
            await unitOfWork.SaveChangesAsync();
            contentList.Add(content1);

            // 模拟异常
            throw new InvalidOperationException("Simulated exception for rollback test");
        }
        catch (InvalidOperationException)
        {
            await tranScope.RollbackAsync();

            // 验证数据未保存（事务已回滚）
            var savedEntities = await testRep.GetListAsync(t => contentList.Contains(t.Content));
            Assert.Empty(savedEntities);
        }
    }

    /// <summary>
    /// 测试 BeginTransactionScopeAsync - 未提交时自动回滚
    /// </summary>
    [Fact]
    public async Task BeginTransactionScopeAsync_AutoRollback_WhenNotCommitted()
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

        var contentList = new List<string>();

        // 创建事务作用域但不提交
        await using var tranScope = await unitOfWork.BeginTransactionScopeAsync();
        var content1 = Guid.NewGuid().ToString();
        await testRep.AddAsync(new TestEntity(content1, "Test User 1", "test1@example.com", "Auto rollback test"));
        await unitOfWork.SaveChangesAsync();
        contentList.Add(content1);

        // 不调用 CommitAsync，让 Dispose 自动回滚

        // 验证数据未保存（自动回滚）
        var savedEntities = await testRep.GetListAsync(t => contentList.Contains(t.Content));
        Assert.Empty(savedEntities);
    }

    /// <summary>
    /// 测试 BeginTransactionScopeAsync - 使用显式 RollbackAsync
    /// </summary>
    [Fact]
    public async Task BeginTransactionScopeAsync_ExplicitRollback_Success()
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

        var contentList = new List<string>();

        await using var tranScope = await unitOfWork.BeginTransactionScopeAsync();
        try
        {
            var content1 = Guid.NewGuid().ToString();
            await testRep.AddAsync(new TestEntity(content1, "Test User 1", "test1@example.com", "Explicit rollback test"));
            await unitOfWork.SaveChangesAsync();
            contentList.Add(content1);

            // 显式回滚
            await tranScope.RollbackAsync();

            // 验证数据未保存
            var savedEntities = await testRep.GetListAsync(t => contentList.Contains(t.Content));
            Assert.Empty(savedEntities);
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 测试 BeginTransactionScopeAsync - 重复提交处理
    /// </summary>
    [Fact]
    public async Task BeginTransactionScopeAsync_DuplicateCommit_HandleGracefully()
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

        var contentList = new List<string>();

        await using var tranScope = await unitOfWork.BeginTransactionScopeAsync();
        try
        {
            var content1 = Guid.NewGuid().ToString();
            await testRep.AddAsync(new TestEntity(content1, "Test User 1", "test1@example.com", "Duplicate commit test"));
            await unitOfWork.SaveChangesAsync();
            contentList.Add(content1);

            // 第一次提交
            await tranScope.CommitAsync();

            // 第二次提交（应该被忽略，不抛出异常）
            await tranScope.CommitAsync();

            // 验证数据已保存
            var savedEntities = await testRep.GetListAsync(t => contentList.Contains(t.Content));
            Assert.Single(savedEntities);

            // 清理测试数据
            await testRep.DeleteAsync(t => contentList.Contains(t.Content));
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 测试 BeginTransactionScope - 同步版本
    /// </summary>
    [Fact]
    public async Task BeginTransactionScope_Commit_Success()
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

        var contentList = new List<string>();

        await using var tranScope = unitOfWork.BeginTransactionScope();
        try
        {
            // 添加数据
            var content1 = Guid.NewGuid().ToString();
            await testRep.AddAsync(new TestEntity(content1, "Test User 1", "test1@example.com", "Sync TransactionScope test"));
            await unitOfWork.SaveChangesAsync();
            contentList.Add(content1);

            // 提交事务
            await tranScope.CommitAsync();

            // 验证数据已保存
            var savedEntities = await testRep.GetListAsync(t => contentList.Contains(t.Content));
            Assert.Single(savedEntities);

            // 清理测试数据
            await testRep.DeleteAsync(t => contentList.Contains(t.Content));
        }
        catch (Exception ex)
        {
            await tranScope.RollbackAsync();
            _testOutputHelper.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 测试 BeginTransactionScopeAsync - 指定隔离级别
    /// </summary>
    [Fact]
    public async Task BeginTransactionScopeAsync_WithIsolationLevel_Success()
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

        var contentList = new List<string>();

        await using var tranScope = await unitOfWork.BeginTransactionScopeAsync(System.Data.IsolationLevel.ReadCommitted);
        try
        {
            var content1 = Guid.NewGuid().ToString();
            await testRep.AddAsync(new TestEntity(content1, "Test User 1", "test1@example.com", "IsolationLevel test"));
            await unitOfWork.SaveChangesAsync();
            contentList.Add(content1);

            await tranScope.CommitAsync();

            // 验证数据已保存
            var savedEntities = await testRep.GetListAsync(t => contentList.Contains(t.Content));
            Assert.Single(savedEntities);

            // 清理测试数据
            await testRep.DeleteAsync(t => contentList.Contains(t.Content));
        }
        catch (Exception ex)
        {
            await tranScope.RollbackAsync();
            _testOutputHelper.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 测试 BeginTransactionScopeAsync - 提交后回滚应抛出异常
    /// </summary>
    [Fact]
    public async Task BeginTransactionScopeAsync_RollbackAfterCommit_ThrowsException()
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

        var contentList = new List<string>();

        await using var tranScope = await unitOfWork.BeginTransactionScopeAsync();
        try
        {
            var content1 = Guid.NewGuid().ToString();
            await testRep.AddAsync(new TestEntity(content1, "Test User 1", "test1@example.com", "Rollback after commit test"));
            await unitOfWork.SaveChangesAsync();
            contentList.Add(content1);

            await tranScope.CommitAsync();

            // 清理测试数据
            await testRep.DeleteAsync(t => contentList.Contains(t.Content));

            // 尝试在提交后回滚，应抛出异常
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await tranScope.RollbackAsync());
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _testOutputHelper.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }
}