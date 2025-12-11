using Azrng.SettingConfig.Dto;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace Azrng.SettingConfig
{
    public static class AspNetCoreDashboardContextExtensions
    {
        public static HttpContext GetHttpContext([NotNull] this DashboardContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var aspNetCoreContext = context as AspNetCoreDashboardContext;
            if (aspNetCoreContext == null)
            {
                throw new ArgumentException($"Context argument should be of type `{nameof(AspNetCoreDashboardContext)}`!", nameof(context));
            }

            return aspNetCoreContext.HttpContext;
        }
    }
}