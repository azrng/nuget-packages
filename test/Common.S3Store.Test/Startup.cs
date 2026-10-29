using Azrng.S3Store;
using Microsoft.Extensions.DependencyInjection;

namespace Common.S3Store.Test;

public class Startup
{
    // 连接信息通过环境变量注入，仓库内不保留真实凭据：
    //   S3STORE_URL / S3STORE_ACCESS_KEY / S3STORE_SECRET_KEY
    // 示例（PowerShell）：
    //   $env:S3STORE_URL = "https://minio.example.com:9000"
    //   $env:S3STORE_ACCESS_KEY = "<access-key>"
    //   $env:S3STORE_SECRET_KEY = "<secret-key>"
    //   dotnet test test/Common.S3Store.Test
    // 未设置时回退到本地默认值（http://localhost:9000 admin/123456789）
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddS3Store(x =>
        {
            x.Url = Environment.GetEnvironmentVariable("S3STORE_URL") ?? "http://localhost:9000";
            x.AccessKey = Environment.GetEnvironmentVariable("S3STORE_ACCESS_KEY") ?? "admin";
            x.SecretKey = Environment.GetEnvironmentVariable("S3STORE_SECRET_KEY") ?? "123456789";
        });
    }
}
