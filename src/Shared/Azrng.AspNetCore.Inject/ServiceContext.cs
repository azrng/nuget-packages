using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Azrng.AspNetCore.Inject;

/// <summary>
/// 服务上下文
/// </summary>
/// <remarks>用于模块间传递服务上下文信息</remarks>
public class ServiceContext
{
    private readonly IServiceCollection _serviceCollection;
    private readonly IConfiguration _configuration;

    internal ServiceContext(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        _serviceCollection = serviceCollection;
        _configuration = configuration;
    }

    /// <summary>
    /// 服务
    /// </summary>
    public IServiceCollection Services => _serviceCollection;

    /// <summary>
    /// 配置信息
    /// </summary>
    public IConfiguration Configuration => _configuration;
}