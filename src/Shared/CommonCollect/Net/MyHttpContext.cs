using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CommonCollect.Net
{
    /// <summary>
    /// 上下文
    /// </summary>
    public class MyHttpContext
    {
        public static IServiceProvider ServiceProvider;

        static MyHttpContext() { }

        public static HttpContext Current
        {
            get
            {
                object factory = ServiceProvider.GetService(typeof(IHttpContextAccessor));
                HttpContext context = ((HttpContextAccessor)factory).HttpContext;
                return context;
            }
        }

        public static HttpContext httpContext
        {
            get
            {
                object factory = ServiceProvider.GetService(typeof(IHttpContextAccessor));
                HttpContext context = ((HttpContextAccessor)factory).HttpContext;
                return context;
            }
        }

        public static IHostingEnvironment HostingEnvironment
        {
            get
            {
                return ServiceProvider.GetRequiredService<IHostingEnvironment>();
            }
        }

        public static string MapPath(string path)
        {
            return HostingEnvironment.ContentRootPath.Replace('\\', '/') + path;
        }

        public static string WebRootPath(string path)
        {
            return HostingEnvironment.WebRootPath.Replace('\\', '/') + path;
        }
    }
}