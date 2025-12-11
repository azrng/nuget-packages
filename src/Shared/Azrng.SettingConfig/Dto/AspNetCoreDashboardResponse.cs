using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace Azrng.SettingConfig.Dto
{
    public sealed class AspNetCoreDashboardResponse
    {
        private readonly HttpContext _context;

        public AspNetCoreDashboardResponse([NotNull] HttpContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string ContentType
        {
            get => _context.Response.ContentType;
            set
            {
                if (!_context.Response.HasStarted)
                {
                    _context.Response.ContentType = value;
                }
            }
        }

        public int StatusCode
        {
            get => _context.Response.StatusCode;
            set
            {
                if (!_context.Response.HasStarted)
                {
                    _context.Response.StatusCode = value;
                }
            }
        }

        public Stream Body => _context.Response.Body;

        public Task WriteAsync(string text)
        {
            return _context.Response.WriteAsync(text);
        }
    }
}