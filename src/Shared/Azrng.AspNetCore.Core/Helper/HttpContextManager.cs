using Microsoft.AspNetCore.Http;

namespace Azrng.AspNetCore.Core.Helper
{
    /// <summary>
    /// http上下文
    /// </summary>
    public static class HttpContextManager
    {
        private static Lazy<IHttpContextAccessor>? _httpContextAccessor;

        /// <summary>
        /// 初始化IHttpContextAccessor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public static void Init(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = new Lazy<IHttpContextAccessor>(httpContextAccessor);
        }

        public static HttpContext? Current => _httpContextAccessor?.Value?.HttpContext;
    }
}