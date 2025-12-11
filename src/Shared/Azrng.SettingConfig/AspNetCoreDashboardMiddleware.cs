using Azrng.SettingConfig.Dto;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Azrng.SettingConfig
{
    /// <summary>
    /// 配置UI控制器
    /// </summary>
    public class AspNetCoreDashboardMiddleware
    {
        private const string _embeddedFileNamespace = "Azrng.SettingConfig.wwwroot";
        private readonly StaticFileMiddleware _staticFileMiddleware;
        private readonly DashboardOptions _dashboardOptions;
        private readonly ManifestResourceService _manifestResourceService;

        public AspNetCoreDashboardMiddleware(RequestDelegate next,
            IWebHostEnvironment hostingEnv,
            ILoggerFactory loggerFactory,
            IOptions<DashboardOptions> options,
            ManifestResourceService manifestResourceService)
        {
            _dashboardOptions = options.Value;
            _staticFileMiddleware = CreateStaticFileMiddleware(next, hostingEnv, loggerFactory);
            _manifestResourceService = manifestResourceService;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var httpMethod = httpContext.Request.Method;
            var path = httpContext.Request.Path.Value;

            // 如果请求方法为GET，并且路径匹配指定的RoutePrefix（带有或不带尾部斜杠），则重定向到index.html页面
            if (httpMethod == "GET" &&
                Regex.IsMatch(path, $"^/?{Regex.Escape(_dashboardOptions.RoutePrefix)}/?$", RegexOptions.IgnoreCase))
            {
                // Use relative redirect to support proxy environments
                var relativeIndexUrl = string.IsNullOrEmpty(path) || path.EndsWith("/")
                    ? "index.html"
                    : $"{path.Split('/').Last()}/index.html";

                RespondWithRedirect(httpContext.Response, relativeIndexUrl);
                return;
            }

            // 如果请求方法为GET，并且路径以指定前缀和index.html结尾，则根据授权过滤器对请求进行授权
            if (httpMethod == "GET" && Regex.IsMatch(path,
                    $"^/{Regex.Escape(_dashboardOptions.RoutePrefix)}/?index.html$", RegexOptions.IgnoreCase))
            {
                var context = new AspNetCoreDashboardContext(_dashboardOptions, httpContext);
                foreach (var filter in _dashboardOptions.Authorization)
                {
                    if (!filter.Authorize(context))
                    {
                        SetResponseStatusCode(httpContext, GetUnauthorizedStatusCode(httpContext));
                        return;
                    }
                }

                await RespondWithIndexHtml(httpContext.Response);
                return;
            }

            await _staticFileMiddleware.Invoke(httpContext);
        }

        /// <summary>
        /// 创建一个静态文件中间件
        /// </summary>
        /// <param name="next"></param>
        /// <param name="hostingEnv"></param>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        private StaticFileMiddleware CreateStaticFileMiddleware(
            RequestDelegate next,
            IWebHostEnvironment hostingEnv,
            ILoggerFactory loggerFactory)
        {
            //用于配置静态文件处理的选项
            var staticFileOptions = new StaticFileOptions
            {
                //根据_dashboardOptions.RoutePrefix的值来确定静态文件的请求路径
                RequestPath =
                    string.IsNullOrEmpty(_dashboardOptions.RoutePrefix)
                        ? string.Empty
                        : $"/{_dashboardOptions.RoutePrefix}",
                //使用EmbeddedFileProvider来提供静态文件，它从指定的程序集（typeof(DashboardOptions).GetTypeInfo().Assembly）和命名空间（_embeddedFileNamespace）中获取静态文件
                FileProvider = new EmbeddedFileProvider(typeof(DashboardOptions).GetTypeInfo().Assembly,
                    _embeddedFileNamespace),
            };

            return new StaticFileMiddleware(next, hostingEnv, Options.Create(staticFileOptions), loggerFactory);
        }

        /// <summary>
        /// 响应跳转
        /// </summary>
        /// <param name="response"></param>
        /// <param name="location"></param>
        private void RespondWithRedirect(HttpResponse response, string location)
        {
            response.StatusCode = 301;
            response.Headers["Location"] = location;
        }

        /// <summary>
        /// 响应Html内容
        /// </summary>
        /// <param name="response"></param>
        private async Task RespondWithIndexHtml(HttpResponse response)
        {
            response.StatusCode = 200;
            response.ContentType = "text/html;charset=utf-8";

            // 获取或者设置用于检索setting-ui页面的stream函数
            var bytes = await _manifestResourceService.GetManifestResource();
            // Inject arguments before writing to response
            var htmlBuilder = new StringBuilder(Encoding.UTF8.GetString(bytes));
            foreach (var entry in GetIndexArguments())
            {
                htmlBuilder.Replace(entry.Key, entry.Value);
            }

            await response.WriteAsync(htmlBuilder.ToString(), Encoding.UTF8);
        }

        private IDictionary<string, string> GetIndexArguments()
        {
            return new Dictionary<string, string>()
            {
                { "%(PageTitle)%", _dashboardOptions.PageTitle },
                { "%(PageDescription)%", _dashboardOptions.PageDescription },
                { "%(BaseUrl)%", _dashboardOptions.ApiRoutePrefix },
                // { "%(ConfigObject)", JsonSerializer.Serialize(_options.ConfigObject, _jsonSerializerOptions) },
                // { "%(OAuthConfigObject)", JsonSerializer.Serialize(_options.OAuthConfigObject, _jsonSerializerOptions) },
                // { "%(Interceptors)", JsonSerializer.Serialize(_options.Interceptors) },
            };
        }

        private static void SetResponseStatusCode(HttpContext httpContext, int statusCode)
        {
            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.StatusCode = statusCode;
            }
        }

        private static int GetUnauthorizedStatusCode(HttpContext httpContext)
        {
            return httpContext.User?.Identity?.IsAuthenticated == true
                ? (int)HttpStatusCode.Forbidden
                : (int)HttpStatusCode.Unauthorized;
        }
    }
}