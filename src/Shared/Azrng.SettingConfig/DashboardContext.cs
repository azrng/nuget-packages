using Azrng.SettingConfig.Dto;
using System.Diagnostics.CodeAnalysis;

namespace Azrng.SettingConfig
{
    public abstract class DashboardContext
    {
        protected DashboardContext([NotNull] DashboardOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public AspNetCoreDashboardRequest Request { get; protected set; }

        public AspNetCoreDashboardResponse Response { get; protected set; }

        /// <summary>
        /// Dashboard配置
        /// </summary>
        public DashboardOptions Options { get; }
    }
}