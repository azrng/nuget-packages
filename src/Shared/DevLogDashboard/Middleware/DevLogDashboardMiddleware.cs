using Azrng.DevLogDashboard.Options;
using Azrng.DevLogDashboard.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Azrng.DevLogDashboard.Middleware;

/// <summary>
/// DevLogDashboard 仪表板中间件
/// </summary>
public class DevLogDashboardMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _staticFilesContent;
    private readonly string _indexHtmlContent;

    public DevLogDashboardMiddleware(RequestDelegate next)
    {
        _next = next;

        // 读取嵌入的静态文件
        var assembly = typeof(DevLogDashboardMiddleware).Assembly;
        _staticFilesContent = ReadEmbeddedFile(assembly, "DevLogDashboard.wwwroot.styles.css") ?? string.Empty;
        _indexHtmlContent = ReadEmbeddedFile(assembly, "DevLogDashboard.wwwroot.index.html") ?? string.Empty;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 从服务容器获取所需服务
        var options = context.RequestServices.GetRequiredService<DevLogDashboardOptions>();
        var logStore = context.RequestServices.GetRequiredService<ILogStore>();
        var apiHandler = new DevLogDashboardApiHandler(logStore);

        var path = context.Request.Path.Value ?? "";

        // 检查授权
        if (options.AuthorizationFilter != null)
        {
            var authorized = await options.AuthorizationFilter(context);
            if (!authorized)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }

        // 处理 API 请求（在 Map 分支内，路径已经不包含 EndpointPath）
        if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await apiHandler.HandleApiRequestAsync(context);
            return;
        }

        // 处理 CSS 请求
        if (path == "/styles.css")
        {
            context.Response.ContentType = "text/css";
            // 开发环境禁用缓存，生产环境使用长期缓存
            context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            await context.Response.WriteAsync(_staticFilesContent);
            return;
        }

        // 处理 JS 请求
        if (path == "/app.js")
        {
            context.Response.ContentType = "application/javascript";
            // 开发环境禁用缓存，生产环境使用长期缓存
            context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            var jsContent = ReadEmbeddedFile(typeof(DevLogDashboardMiddleware).Assembly, "DevLogDashboard.wwwroot.app.js");
            await context.Response.WriteAsync(jsContent ?? string.Empty);
            return;
        }

        // 如果路径是空的（访问 /dev-logs），重定向到 /dev-logs/ 以确保相对路径正确工作
        if (string.IsNullOrEmpty(path))
        {
            context.Response.StatusCode = 302;
            context.Response.Headers.Append("Location", options.EndpointPath + "/");
            return;
        }

        // 默认返回首页
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(_indexHtmlContent);
    }

    private string? ReadEmbeddedFile(Assembly assembly, string resourceName)
    {
        var name = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(resourceName));
        if (name == null) return null;

        using var stream = assembly.GetManifestResourceStream(name);
        if (stream == null) return null;

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
