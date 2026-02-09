using Azrng.EFCore;
using Common.EFCore.PostgreSql.Test.Model.SqlQuerys;
using Common.Extension;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Common.EFCore.PostgreSql.Test;

/// <summary>
/// ef7.0+支持的非标实体SqlQuery查询
/// </summary>
public class SqlQueryTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _connectionStr;

    public SqlQueryTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _connectionStr = "Host=localhost;Username=postgres;Password=123456;Database=pgsql_test;port=5432";
    }

    [Fact]
    public async Task 字符串参数化查询()
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
               })
               .AddUnitOfWork<TestDbContext>();
        await using var serviceProvider = service.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TestDbContext>>();

        // 原始写法
        var originSql = "select id,content from public.test_table where content='用户登录成功'";
        var result = await unitOfWork.ExecuteScalarAsync(originSql);
        _testOutputHelper.WriteLine(result.ToString());
        Assert.NotEmpty(result.ToString() ?? string.Empty);

        var param = "用户登录成功"; // 入参

        // 参数化查询
        FormattableString sql = $"select id,content from public.test_table where content={param}";
        var list = await unitOfWork.SqlQuery<SqlQueryTestDto>(sql).ToListAsync();
        Assert.True(list.Count > 0);
        _testOutputHelper.WriteLine(list.ToJson());
    }

    [Fact]
    public async Task 数组参数化查询()
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
               })
               .AddUnitOfWork<TestDbContext>();
        await using var serviceProvider = service.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TestDbContext>>();

        var param = new List<string>() { "用户登录成功", "fghig" };

        // 参数化查询
        FormattableString sql = $"select id,content from public.test_table where content=any({param})";
        var list = await unitOfWork.SqlQuery<SqlQueryTestDto>(sql).ToListAsync();
        Assert.True(list.Count > 0);
        _testOutputHelper.WriteLine(list.ToJson());
    }

    [Fact]
    public async Task 模糊匹配参数化查询()
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
               })
               .AddUnitOfWork<TestDbContext>();
        await using var serviceProvider = service.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<TestDbContext>>();

        var param = "%完成%";

        // 参数化查询
        FormattableString sql = $"select id,content from public.test_table where content like {param}";
        var list = await unitOfWork.SqlQuery<SqlQueryTestDto>(sql).ToListAsync();
        Assert.True(list.Count > 0);
        _testOutputHelper.WriteLine(list.ToJson());
    }
}