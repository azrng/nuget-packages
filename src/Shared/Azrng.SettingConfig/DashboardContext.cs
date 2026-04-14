using Azrng.SettingConfig.Dto;

namespace Azrng.SettingConfig
{
    public abstract class DashboardContext
    {
        protected DashboardContext(DashboardOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public AspNetCoreDashboardRequest Request { get; protected set; } = default!;

        public AspNetCoreDashboardResponse Response { get; protected set; } = default!;

        /// <summary>
        /// Dashboard配置
        /// </summary>
        public DashboardOptions Options { get; }
    }
}
