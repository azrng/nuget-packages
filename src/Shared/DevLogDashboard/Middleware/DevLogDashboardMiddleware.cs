using Azrng.DevLogDashboard.Options;
using Azrng.DevLogDashboard.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Reflection;

namespace Azrng.DevLogDashboard.Middleware;

/// <summary>
/// DevLogDashboard middleware
/// </summary>
public class DevLogDashboardMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _stylesContent;
    private readonly string _indexHtmlContent;
    private readonly string _appJsContent;

    public DevLogDashboardMiddleware(RequestDelegate next)
    {
        _next = next;

        var assembly = typeof(DevLogDashboardMiddleware).Assembly;
        _stylesContent = ReadEmbeddedFile(assembly, "DevLogDashboard.wwwroot.styles.css") ?? string.Empty;
        _indexHtmlContent = ReadEmbeddedFile(assembly, "DevLogDashboard.wwwroot.index.html") ?? string.Empty;
        _appJsContent = ReadEmbeddedFile(assembly, "DevLogDashboard.wwwroot.app.js") ?? string.Empty;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var options = context.RequestServices.GetRequiredService<DevLogDashboardOptions>();
        var logStore = context.RequestServices.GetRequiredService<ILogStore>();
        var logQueue = context.RequestServices.GetRequiredService<Background.IBackgroundLogQueue>();
        var apiHandler = new DevLogDashboardApiHandler(logStore, logQueue);

        var path = context.Request.Path.Value ?? string.Empty;

        if (!options.AllowNonLocalAccess && !IsLocalRequest(context))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden");
            return;
        }

        if (options.AuthorizationFilter != null)
        {
            var authorized = await options.AuthorizationFilter(context);
            if (!authorized)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden");
                return;
            }
        }

        if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await apiHandler.HandleApiRequestAsync(context);
            return;
        }

        if (path == "/styles.css")
        {
            context.Response.ContentType = "text/css";
            context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            await context.Response.WriteAsync(_stylesContent);
            return;
        }

        if (path == "/app.js")
        {
            context.Response.ContentType = "application/javascript";
            context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            await context.Response.WriteAsync(_appJsContent);
            return;
        }

        if (string.IsNullOrEmpty(path))
        {
            context.Response.StatusCode = 302;
            context.Response.Headers.Append("Location", options.EndpointPath + "/");
            return;
        }

        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(_indexHtmlContent);
    }

    private static string? ReadEmbeddedFile(Assembly assembly, string resourceName)
    {
        var name = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(resourceName, StringComparison.Ordinal));
        if (name == null)
        {
            return null;
        }

        using var stream = assembly.GetManifestResourceStream(name);
        if (stream == null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static bool IsLocalRequest(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp == null)
        {
            return true;
        }

        if (remoteIp.IsIPv4MappedToIPv6)
        {
            remoteIp = remoteIp.MapToIPv4();
        }

        if (IPAddress.IsLoopback(remoteIp))
        {
            return true;
        }

        var localIp = context.Connection.LocalIpAddress;
        if (localIp == null)
        {
            return false;
        }

        if (localIp.IsIPv4MappedToIPv6)
        {
            localIp = localIp.MapToIPv4();
        }

        return remoteIp.Equals(localIp);
    }
}
