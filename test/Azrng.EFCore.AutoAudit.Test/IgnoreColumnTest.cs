using Azrng.Core.Model;
using Azrng.EFCore.AutoAudit.Test.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azrng.EFCore.AutoAudit.Test;

public class IgnoreColumnTest
{
    [Fact]
    public void IgnoreColumn1()
    {
        var services = new ServiceCollection();

        //services.AddLogging(builder => builder.AddSimpleConsole());

        // 添加业务数据库
        services.AddDbContext<TestDbContext>((provider, options) =>
        {
            options.UseSqlite($"Data Source={Guid.NewGuid()}.db");
            options.AddInterceptors(provider.GetRequiredService<AuditInterceptor>());
        });

        // 添加审计
        services.AddEFCoreAutoAudit(builder =>
        {
            builder.IgnoreColumn("id")
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

        context.Test2Entities.Add(new Test2Entity { Name = "test1", CreatedAt = DateTimeOffset.Now });
        context.SaveChanges();
        var entity = context.TestEntities.Find(1);
        if (entity is not null)
        {
            context.TestEntities.Remove(entity);
            context.SaveChanges();
        }
    }
}