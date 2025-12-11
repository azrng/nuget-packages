using APIStudy.Model;
using Azrng.AspNetCore.Core.Extension;
using Azrng.Core.Json;
using Azrng.Swashbuckle;

namespace APIStudy;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddAnyCors()
                .AddDefaultSwaggerGen()
                .AddMvcModelVerifyFilter()
                .AddMvcResultPackFilterFilter();

        // services.ConfigureNewtonsoftJson();
        services.ConfigureDefaultJson();

        services.AddShowAllServices();

        #region 测试配置注入

        services.AddObjectAccessor(new TestOptions { Name = "李四" });
        var info = services.GetObjectOrNull<TestOptions>();
        Console.WriteLine($"配置值为：{info?.Name}");

        // 还是没有搞懂咋回事
        services.PreConfigure<TestOptions>(options => options.Name = "bbb");
        var info2 = services.GetPreConfigureActions<TestOptions>();
        var bbb = info2.Configure();

        #endregion
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        app.UseDefaultSwagger();

        app.UseShowAllServicesMiddleware();
        app.UseRequestBodyRepetitionRead();

        app.UseRouting();
        app.UseStaticFiles();

        app.UseAutoAuditLog();
        app.UseCustomExceptionMiddleware();

        //使用跨域
        app.UseAnyCors();

        app.UseAuthorization();

        app.MapControllers();
    }
}