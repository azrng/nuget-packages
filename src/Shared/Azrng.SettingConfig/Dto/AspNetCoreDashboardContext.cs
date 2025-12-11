using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace Azrng.SettingConfig.Dto
{
    internal sealed class AspNetCoreDashboardContext : DashboardContext
    {
        public AspNetCoreDashboardContext(
            [NotNull] DashboardOptions options,
            [NotNull] HttpContext httpContext)
            : base(options)
        {
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            Request = new AspNetCoreDashboardRequest(httpContext);
            Response = new AspNetCoreDashboardResponse(httpContext);
        }

        public HttpContext HttpContext { get; }
    }
}