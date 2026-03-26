using Azrng.Core.Json;
using Azrng.DataAccess;
using Azrng.DataAccess.DbBridge;
using Azrng.DataAccess.Helper;
using Microsoft.Extensions.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace Common.DbSystemOperator.MySql.Test;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // 注入日志
        services.AddLogging(x => x.AddXunitOutput());

        var pgConfig = new DataSourceConfig
                       {
                           DbName = "zyp-test",
                           Host = "localhost",
                           Port = 3306,
                           User = "root",
                           Password = "123456"
                       };

        services.AddScoped<IBasicDbBridge>(p =>
        {
            var systemOperator = new MySqlBasicDbBridge(pgConfig);
            return systemOperator;
        });

        services.AddScoped<IDbHelper>(p => new MySqlDbHelper(pgConfig));

        services.ConfigureDefaultJson((options) => { });
    }

    public void Configure() { }
}