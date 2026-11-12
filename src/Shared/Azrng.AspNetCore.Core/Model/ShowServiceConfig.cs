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

        /// <summary>
        /// 是否允许在非开发环境展示服务清单（泄露内部架构信息，谨慎开启）
        /// </summary>
        public bool AllowNonDevelopment { get; set; }
    }
}