using Azrng.Core.Json;
using Azrng.DbOperator;
using Azrng.DbOperator.DbBridge;
using Azrng.DbOperator.Helper;
using Microsoft.Extensions.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace Common.DbSystemOperator.ClickHouse.Test;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // 注入日志
        services.AddLogging(x => x.AddXunitOutput());

        var chConfig = new DataSourceConfig
                       {
                           DbName = "default",
                           Host = "localhost",
                           Port = 8123,
                           User = "default",
                           Password = "123456"
                       };

        // var chConfig = new DataSourceConfig
        //                {
        //                    DbName = "default",
        //                    Host = "172.16.1.18",
        //                    Port = 8123,
        //                    User = "default",
        //                    Password = "123456789",
        //                    DecimalIsTwo = true
        //                };

        services.AddScoped<IBasicDbBridge>(p =>
        {
            var systemOperator = new ClickHouseBasicDbBridge(chConfig);
            return systemOperator;
        });

        services.AddScoped<IDbHelper>(p => new ClickHouseDbHelper(chConfig));

        services.ConfigureDefaultJson((options) => { });
    }

    public void Configure() { }
}