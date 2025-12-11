namespace Azrng.AspNetCore.Inject;

/// <summary>
/// 模块接口
/// </summary>
public interface IModule
{
    void ConfigureServices(ServiceContext services);
}