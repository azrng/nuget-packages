using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace Azrng.SettingConfig.Dto
{
    public sealed class AspNetCoreDashboardRequest
    {
        private readonly HttpContext _context;

        public AspNetCoreDashboardRequest([NotNull] HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _context = context;
        }

        public string Method => _context.Request.Method;
        public string Path => _context.Request.Path.Value ?? string.Empty;
        public string PathBase => _context.Request.PathBase.Value ?? string.Empty;
        public string LocalIpAddress => _context.Connection.LocalIpAddress?.ToString() ?? string.Empty;
        public string RemoteIpAddress => _context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        public string GetQuery(string key) => _context.Request.Query[key].ToString();

        public async Task<IList<string>> GetFormValuesAsync(string key)
        {
            var form = await _context.Request.ReadFormAsync();
            return form[key];
        }
    }
}
