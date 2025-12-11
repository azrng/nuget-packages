using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;

namespace PostgresSqlSample.EFCoreExtensions;

public class PgDbContextOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo _info;

    public void Validate(IDbContextOptions options) { }

    public void ApplyServices(IServiceCollection services)
    {
        //这里将转换器注入到服务当中.
        services.AddSingleton<IMethodCallTranslatorProvider, PgMethodCallTranslatorProvider>();

        //services.AddEntityFrameworkPg();
    }

    public DbContextOptionsExtensionInfo Info => _info ??= new MyDbContextOptionsExtensionInfo(this);

    private sealed class MyDbContextOptionsExtensionInfo : DbContextOptionsExtensionInfo
    {
        public MyDbContextOptionsExtensionInfo(IDbContextOptionsExtension instance) : base(instance) { }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "";

        public override int GetServiceProviderHashCode()
        {
            return 0;
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            return false;
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) { }
    }
}