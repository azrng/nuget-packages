using System.Reflection;
using DevLogDashboard.Options;
using Microsoft.AspNetCore.Http;

namespace DevLogDashboard.Middleware;

/// <summary>
/// DevLogDashboard 仪表板中间件
/// </summary>
public class DevLogDashboardMiddleware
{
    private readonly RequestDelegate _next;
    private readonly DevLogDashboardOptions _options;
    private readonly string _staticFilesContent;
    private readonly string _indexHtmlContent;

    public DevLogDashboardMiddleware(RequestDelegate next, DevLogDashboardOptions options)
    {
        _next = next;
        _options = options;

        // 读取嵌入的静态文件
        var assembly = typeof(DevLogDashboardMiddleware).Assembly;
        _staticFilesContent = ReadEmbeddedFile(assembly, "DevLogDashboard.wwwroot.styles.css") ?? string.Empty;
        _indexHtmlContent = ReadEmbeddedFile(assembly, "DevLogDashboard.wwwroot.index.html") ?? string.Empty;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;

        if (string.IsNullOrEmpty(path) || !path.StartsWith(_options.EndpointPath, StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // 检查授权
        if (_options.AuthorizationFilter != null)
        {
            var authorized = await _options.AuthorizationFilter(context);
            if (!authorized)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }

        // 处理子路径
        var subPath = path.Length > _options.EndpointPath.Length
            ? path.Substring(_options.EndpointPath.Length)
            : "";

        // 处理 API 请求
        if (subPath.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // 处理 CSS 请求
        if (subPath == "/styles.css")
        {
            context.Response.ContentType = "text/css";
            context.Response.Headers.Add("Cache-Control", "public, max-age=31536000");
            await context.Response.WriteAsync(_staticFilesContent);
            return;
        }

        // 处理 JS 请求
        if (subPath == "/app.js")
        {
            context.Response.ContentType = "application/javascript";
            context.Response.Headers.Add("Cache-Control", "public, max-age=31536000");
            var jsContent = ReadEmbeddedFile(typeof(DevLogDashboardMiddleware).Assembly, "DevLogDashboard.wwwroot.app.js");
            await context.Response.WriteAsync(jsContent ?? string.Empty);
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
