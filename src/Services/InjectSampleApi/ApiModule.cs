using Azrng.AspNetCore.Inject;
using Azrng.AspNetCore.Inject.Attributes;
using InjectSample.Application;

namespace InjectSampleApi
{
    [InjectModule<ApplicationModule>]
    public class ApiModule : IModule
    {
        public void ConfigureServices(ServiceContext services)
        {
            // 模块初始化代码
        }
    }
}