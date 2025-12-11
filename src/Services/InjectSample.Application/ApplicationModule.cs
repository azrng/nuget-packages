using Azrng.AspNetCore.Inject;
using Microsoft.Extensions.Configuration;

namespace InjectSample.Application;

public class ApplicationModule : IModule
{
    private readonly IConfiguration _configuration;

    public ApplicationModule(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(ServiceContext services)
    {
        // 编写模块初始化代码
    }
}