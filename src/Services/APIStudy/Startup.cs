using APIStudy.Model;
using Azrng.AspNetCore.Core.JsonConverters;
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
        services.AddDefaultControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new LongToStringConverter());
                });

        services.AddAnyCors()
                .AddDefaultSwaggerGen();

        // services.ConfigureNewtonsoftJson();
        services.ConfigureDefaultJson();

        services.AddShowAllServices();
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        app.UseDefaultSwagger();

        app.UseShowAllServices();
        app.UseRequestBodyRepetitionRead();

        app.UseRouting();
        app.UseStaticFiles();

        // app.UseAutoAuditLog();
        app.UseGlobalException();

        //使用跨域
        app.UseCorsPolicy();

        app.UseAuthorization();

        app.MapControllers();
    }
}