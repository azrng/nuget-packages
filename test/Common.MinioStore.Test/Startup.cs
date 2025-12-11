using Microsoft.Extensions.DependencyInjection;

namespace Common.MinioStore.Test;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // services.AddMinioStore(x =>
        // {
        //     x.Url = "http://localhost:9000";
        //     x.AccessKey = "admin";
        //     x.SecretKey = "123456789";
        // });

        services.AddMinioStore("http://admin:123456789@localhost:9008");
    }
}