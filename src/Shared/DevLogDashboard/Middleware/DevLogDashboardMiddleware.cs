using Azrng.DevLogDashboard.Options;
using Azrng.DevLogDashboard.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Reflection;
using System.Text;

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

        var authorized = await AuthorizeAsync(context, options);
        if (!authorized)
        {
            return;
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

    private static async Task<bool> AuthorizeAsync(HttpContext context, DevLogDashboardOptions options)
    {
        if (options.BasicAuthentication == null)
        {
            return true;
        }

        var authorizationHeader = context.Request.Headers.Authorization.ToString();
        if (!TryReadBasicCredentials(authorizationHeader, out var userName, out var password))
        {
            await WriteUnauthorizedAsync(context, options.BasicAuthentication.Realm);
            return false;
        }

        if (!CredentialsMatch(options.BasicAuthentication, userName, password))
        {
            await WriteUnauthorizedAsync(context, options.BasicAuthentication.Realm);
            return false;
        }

        context.User = CreatePrincipal(userName);
        return true;
    }

    private static async Task WriteUnauthorizedAsync(HttpContext context, string realm)
    {
        const string headerName = "WWW-Authenticate";
        if (!context.Response.Headers.ContainsKey(headerName))
        {
            context.Response.Headers.Append(headerName, $"Basic realm=\"{realm}\"");
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
    }

    private static bool TryReadBasicCredentials(string authorizationHeader, out string userName, out string password)
    {
        userName = string.Empty;
        password = string.Empty;

        if (string.IsNullOrWhiteSpace(authorizationHeader)
            || !authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var encodedCredential = authorizationHeader["Basic ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(encodedCredential))
        {
            return false;
        }

        try
        {
            var rawCredential = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredential));
            var separatorIndex = rawCredential.IndexOf(':');
            if (separatorIndex <= 0)
            {
                return false;
            }

            userName = rawCredential[..separatorIndex];
            password = rawCredential[(separatorIndex + 1)..];
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool CredentialsMatch(DevLogDashboardBasicAuthenticationOptions options, string userName, string password)
    {
        return FixedTimeEquals(options.UserName, userName)
               && FixedTimeEquals(options.Password, password);
    }

    private static bool FixedTimeEquals(string expected, string actual)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(actual);

        return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }

    private static ClaimsPrincipal CreatePrincipal(string userName)
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, userName)
            },
            "Basic");

        return new ClaimsPrincipal(identity);
    }
}
