using Azrng.Core.NewtonsoftJson;
using Common.Dapper.Repository;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;
using Xunit.DependencyInjection.Logging;

namespace Azrng.DynamicSqlBuilder.Test;

public class Startup
{
    public void ConfigureServices(IServiceCollection service)
    {
        // 注入日志
        service.AddLogging(x => x.AddXunitOutput());

        var pgsqlConn = "Host=localhost;Username=postgres;Password=123456;Database=test;port=5432";
        // var pgsqlConn = "host=172.16.1.2;port=5432;database=zyp_test;username=postgres;password=765@#sy666;Persist Security Info=true;";
        service.AddScoped<IDbConnection>(_ => new NpgsqlConnection(pgsqlConn));
        service.AddScoped<IDapperRepository, DapperRepository>();

        service.ConfigureNewtonsoftJson();
    }

    public void Configure() { }
}