using Azrng.Core.Model;
using Azrng.EFCore.AutoAudit.Domain;
using Azrng.EFCore.AutoAudit.Test.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Azrng.EFCore.AutoAudit.Test;

/// <summary>
/// 参考资料：https://mp.weixin.qq.com/s/H_0zoFpRRt_foNByJmyUBA
/// </summary>
public class SampleTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SampleTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void DefaultAddAndRemove()
    {
        var services = new ServiceCollection();

        //services.AddLogging(builder => builder.AddSimpleConsole());

        // 添加业务数据库
        services.AddDbContext<TestDbContext>((provider, options) =>
        {
            options.UseSqlite("Data Source=AutoAuditTest2.db");
            options.AddAuditInterceptor(provider);
        });

        // 添加审计
        services.AddEFCoreAutoAudit(builder =>
        {
            builder // .WithStore<AuditFileStore>() // 自定义存储
                .WithAuditRecordsDbContextStore(options => { options.UseSqlite("Data Source=d:\\db\\AutoAudit.db"); });
        }, DatabaseType.PostgresSql);
        using var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // 添加 查询  删除

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.TestEntities.Add(new TestEntity { Name = "test1", CreatedAt = DateTimeOffset.Now });
        context.SaveChanges();
        var entity = context.TestEntities.Find(1);
        if (entity is not null)
        {
            context.TestEntities.Remove(entity);
            context.SaveChanges();
        }

        // 查询审计记录表
        var auditRecordsContext = scope.ServiceProvider.GetRequiredService<AuditRecordsDbContext>();
        var auditRecords = auditRecordsContext.AuditRecords.AsNoTracking().ToArray();
        _testOutputHelper.WriteLine(auditRecords.Length.ToString());
    }
}