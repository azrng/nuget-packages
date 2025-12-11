using System.Diagnostics.CodeAnalysis;

namespace Azrng.SettingConfig.Service
{
    public interface IDashboardAuthorizationFilter
    {
        bool Authorize([NotNull] DashboardContext context);
    }
}