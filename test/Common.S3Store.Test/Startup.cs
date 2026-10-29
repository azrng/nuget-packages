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
            x.Url = "http://s3.sy";
            x.AccessKey = "LZRD2CEDD3V1I1OIOWGW";
            x.SecretKey = "p2mx7MVURaCsEsA2lJNYo9JkPOdZt71alromYfqP";
        });

    }
}