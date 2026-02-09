using Azrng.EFCore;
using Azrng.EFCore.Extensions;
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
        Assert.True(list.Count > 0);
    }

    /// <summary>
    /// SetProperty一键更新
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
        Assert.True(list > 0);
    }

    /// <summary>
    /// 测试条件更新 - SetPropertyIfIsNotNull
    /// </summary>
    [Fact]
    public async Task ConditionalUpdate_SetPropertyIfIsNotNull_Test()
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

        // 先创建测试数据
        var content = Guid.NewGuid().ToString();
        await testRep.AddAsync(new TestEntity(content, "Original Name", "original@test.com", null), true);

        // 测试：email 不为 null 时应该更新
        var newEmail = "new@test.com";
        string? nullEmail = null;
        var result = await testRep.UpdateAsync(x => x.Content == content,
            x => x.SetPropertyIfIsNotNull(t => t.Email, newEmail) // 应该更新
                  .SetPropertyIfIsNotNull(t => t.Name, nullEmail) // 不应该更新
        );

        _testOutputHelper.WriteLine($"Updated {result} records");
        Assert.True(result > 0);

        // 验证
        var updated = await testRep.GetAsync(x => x.Content == content);
        Assert.Equal("new@test.com", updated.Email);
        Assert.Equal("Original Name", updated.Name); // 应该保持原值

        // 清理
        await testRep.DeleteAsync(x => x.Content == content);
    }

    /// <summary>
    /// 测试条件更新 - SetPropertyIfIsNotNullOrWhitespace
    /// </summary>
    [Fact]
    public async Task ConditionalUpdate_SetPropertyIfIsNotNullOrWhitespace_Test()
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

        // 先创建测试数据
        var content = Guid.NewGuid().ToString();
        await testRep.AddAsync(new TestEntity(content, "Original Name", "original@test.com", "Original desc"), true);

        // 测试：各种字符串情况
        var validString = "Valid Value";
        string? nullString = null;
        var emptyString = "";
        var whitespaceString = "   ";

        var result = await testRep.UpdateAsync(x => x.Content == content,
            x => x
                 .SetPropertyIfIsNotNullOrWhitespace(t => t.Name, validString) // 应该更新
                 .SetPropertyIfIsNotNullOrWhitespace(t => t.Email, nullString) // 不应该更新
                 .SetPropertyIfIsNotNullOrWhitespace(t => t.Description, emptyString) // 不应该更新
                 .SetPropertyIfIsNotNullOrWhitespace(t => t.Content, whitespaceString) // 不应该更新
        );

        _testOutputHelper.WriteLine($"Updated {result} records");
        Assert.True(result > 0);

        // 验证
        var updated = await testRep.GetAsync(x => x.Content == content);
        Assert.Equal("Valid Value", updated.Name);
        Assert.Equal("original@test.com", updated.Email); // 应该保持原值
        Assert.Equal("Original desc", updated.Description); // 应该保持原值

        // 清理
        await testRep.DeleteAsync(x => x.Content == content);
    }

    /// <summary>
    /// 测试条件更新 - SetPropertyIfTrue
    /// </summary>
    [Fact]
    public async Task ConditionalUpdate_SetPropertyIfTrue_Test()
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

        // 先创建测试数据
        var content = Guid.NewGuid().ToString();
        await testRep.AddAsync(new TestEntity(content, "Original Name", "original@test.com", null), true);

        // 测试：布尔条件
        var shouldUpdateName = true;
        var shouldUpdateEmail = false;

        var result = await testRep.UpdateAsync(x => x.Content == content,
            x => x.SetPropertyIfTrue(shouldUpdateName, t => t.Name, "Updated Name") // 应该更新
                  .SetPropertyIfTrue(shouldUpdateEmail, t => t.Email, "updated@test.com") // 不应该更新
        );

        _testOutputHelper.WriteLine($"Updated {result} records");
        Assert.True(result > 0);

        // 验证
        var updated = await testRep.GetAsync(x => x.Content == content);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("original@test.com", updated.Email); // 应该保持原值

        // 清理
        await testRep.DeleteAsync(x => x.Content == content);
    }

    /// <summary>
    /// 测试条件更新 - SetPropertyIf 自定义条件
    /// </summary>
    [Fact]
    public async Task ConditionalUpdate_SetPropertyIf_Test()
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

        // 先创建测试数据
        var content = Guid.NewGuid().ToString();
        await testRep.AddAsync(new TestEntity(content, "Original Name", "original@test.com", null), true);

        // 测试：自定义条件（只有长度 > 5 的字符串才更新）
        var longName = "This is a long name";
        var shortName = "Short";

        var result = await testRep.UpdateAsync(x => x.Content == content,
            x => x
                 .SetPropertyIf(name => name.Length > 5, t => t.Name, longName) // 应该更新
                 .SetPropertyIf(name => name.Length > 10, t => t.Email, shortName) // 不应该更新（长度不够）
        );

        _testOutputHelper.WriteLine($"Updated {result} records");
        Assert.True(result > 0);

        // 验证
        var updated = await testRep.GetAsync(x => x.Content == content);
        Assert.Equal("This is a long name", updated.Name);
        Assert.Equal("original@test.com", updated.Email); // 应该保持原值

        // 清理
        await testRep.DeleteAsync(x => x.Content == content);
    }

    /// <summary>
    /// 测试条件更新 - 混合使用普通 SetProperty 和条件更新
    /// </summary>
    [Fact]
    public async Task ConditionalUpdate_Mixed_With_SetProperty_Test()
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

        // 先创建测试数据
        var content = Guid.NewGuid().ToString();
        var originalTime = DateTimeOffset.UtcNow;
        await testRep.AddAsync(
            new TestEntity(content, "Original Name", "original@test.com", "Original desc") { CreatedTime = originalTime }, true);

        // 测试：混合使用
        var newTime = DateTimeOffset.UtcNow.AddHours(1);
        string? nullValue = null;
        var validValue = "Updated Description";

        var result = await testRep.UpdateAsync(x => x.Content == content,
            x => x
                 .SetProperty(t => t.CreatedTime, newTime) // 无条件更新
                 .SetPropertyIfIsNotNull(t => t.Name, nullValue) // 不更新
                 .SetPropertyIfIsNotNullOrWhitespace(t => t.Description, validValue) // 更新
        );

        _testOutputHelper.WriteLine($"Updated {result} records");
        Assert.True(result > 0);

        // 验证
        var updated = await testRep.GetAsync(x => x.Content == content);
        Assert.Equal("Original Name", updated.Name); // Name 应该保持原值
        Assert.Equal("Updated Description", updated.Description); // Description 应该被更新

        // 清理
        await testRep.DeleteAsync(x => x.Content == content);
    }
}