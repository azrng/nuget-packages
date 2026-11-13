using Azrng.EFCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Common.EFCore.PostgreSql.Test;

/// <summary>
/// 原生 SQL 执行回归测试：连接已在事务中打开时命令必须复用连接；参数必须是 DbParameter；
/// 命令与连接资源需正确释放
/// </summary>
public class SqlExecutionTest
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
    /// 连接被事务打开后执行原生 SQL，应复用连接而不是重复 Open 抛异常
    /// </summary>
    [Fact]
    public async Task ExecuteScalar_InsideTransactionScope_ReusesOpenConnection()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await using var transaction = await unitOfWork.BeginTransactionScopeAsync();

        var result = await unitOfWork.ExecuteScalarAsync("select 1");
        Assert.Equal(1, Convert.ToInt32(result));

        // 事务未被破坏，可正常提交
        await transaction.CommitAsync();
    }

    /// <summary>
    /// DbParameter 参数应正确绑定
    /// </summary>
    [Fact]
    public async Task ExecuteScalar_WithDbParameter_BindsCorrectly()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var result = await unitOfWork.ExecuteScalarAsync(
            "select count(*) from test_table where id = @id",
            new NpgsqlParameter("id", 1L));
        Assert.Equal(1, Convert.ToInt32(result));
    }

    /// <summary>
    /// 传入非 DbParameter 的原始值应抛出清晰的参数异常，而不是依赖 provider 抛 InvalidCastException
    /// </summary>
    [Fact]
    public async Task ExecuteScalar_WithRawValueParameter_ThrowsArgumentException()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            unitOfWork.ExecuteScalarAsync("select @p", "raw-value"));
    }

    /// <summary>
    /// ExecuteSqlCommand 带 DbParameter 执行并返回受影响行数；执行路径完整走命令释放逻辑
    /// </summary>
    [Fact]
    public async Task ExecuteSqlCommand_WithDbParameter_ReturnsAffectedRows()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IBaseRepository<TestEntity>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var content = Guid.NewGuid().ToString();
        await repository.AddAsync(new TestEntity(content, "SqlExec Test 1"), true);
        await repository.AddAsync(new TestEntity(content, "SqlExec Test 2"), true);

        var affected = await unitOfWork.ExecuteSqlCommandAsync(
            "delete from test_table where content = @content",
            new NpgsqlParameter("content", content));
        Assert.Equal(2, affected);

        // 确认已删除
        Assert.Equal(0, await repository.CountAsync(t => t.Content == content));
    }
}
