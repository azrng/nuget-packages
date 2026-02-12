using Microsoft.Extensions.DependencyInjection;

namespace Azrng.AspNetCore.Core.Model
{
    /// <summary>
    /// 服务配置
    /// </summary>
    public class ShowServiceConfig
    {
        /// <summary>
        /// 显示所有服务的路径
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 服务列表
        /// </summary>
        public List<ServiceDescriptor> Services { get; set; } = new();
    }
}