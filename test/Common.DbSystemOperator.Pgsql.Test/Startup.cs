using Azrng.Core.Json;
using Azrng.DbOperator;
using Azrng.DbOperator.DbBridge;
using Azrng.DbOperator.Helper;
using Microsoft.Extensions.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace Common.DbSystemOperator.Pgsql.Test;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // 注入日志
        services.AddLogging(x => x.AddXunitOutput());

        var pgConfig = new DataSourceConfig
                       {
                           DbName = "20250801order",
                           Host = "localhost",
                           Port = 5432,
                           User = "postgres",
                           Password = "123456"
                       };

        services.AddScoped<IBasicDbBridge>(p =>
        {
            var systemOperator = new PostgreBasicDbBridge(pgConfig);
            return systemOperator;
        });

        services.AddScoped<IDbHelper>(p => new PostgresSqlDbHelper(pgConfig));
        services.ConfigureDefaultJson((options) => { });
    }

    public void Configure() { }
}