using Azrng.S3Store;
using Microsoft.Extensions.DependencyInjection;

namespace Common.S3Store.Test;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // services.AddS3Store(x =>
        // {
        //     x.Url = "http://localhost:9000";
        //     x.AccessKey = "admin";
        //     x.SecretKey = "123456789";
        // });

        // services.AddS3Store("http://admin:123456789@localhost:9000");


        services.AddS3Store(x =>
        {
            x.Url = "https://172.16.110.40:39900/";
            x.AccessKey = "415WK7QG60FJHUGCE3TO";
            x.SecretKey = "904nqDB1kcmPrzLWTNkjA4n3t77AU2JRJyqAKkNo";
        });

    }
}