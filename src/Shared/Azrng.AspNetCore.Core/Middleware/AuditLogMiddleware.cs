using Azrng.AspNetCore.Core.AuditLog;
using Azrng.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace Azrng.AspNetCore.Core.Middleware;

/// <summary>
/// 审计日志中间件
/// </summary>
public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Stopwatch _stopwatch;
    private readonly AuditLogOptions _options;

    public AuditLogMiddleware(RequestDelegate next, IOptions<AuditLogOptions>? options = null)
    {
        _next = next;
        _options = options?.Value ?? new AuditLogOptions();
        _stopwatch = new Stopwatch();
    }

    public async Task Invoke(HttpContext context)
    {
        var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
        var request = context.Request;
        var path = request.Path.ToString();

        #region 前置检查

        // 检查是否应该记录日志
        if (!ShouldLogRequest(request, path))
        {
            await _next(context);
            return;
        }

        // 检查是否需要忽略的路由
        if (_options.IgnoreRoutePrefix.Count > 0)
        {
            if (_options.IgnoreRoutePrefix.Any(p => path.Contains(p, StringComparison.CurrentCultureIgnoreCase)))
            {
                await _next(context);
                return;
            }
        }

        #endregion

        #region 记录请求日志相关信息

        _stopwatch.Restart();
        var startTime = DateTime.Now;

        var reqHeaders = request.Headers
            .ToDictionary(x => x.Key, v => string.Join(";", v.Value.ToList()));

        var reqBody = await GetRequestBodyAsync(request);

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        var respBody = await GetResponseBodyAsync(context.Response, responseBody);

        await responseBody.CopyToAsync(originalBodyStream);

        var endTime = DateTime.Now;

        context.Response.OnCompleted(() =>
        {
            _stopwatch.Stop();
            var serviceName = configuration.GetValue<string>("ServiceName") ?? "CommonService";

            // 获取日志服务
            var loggerService = context.RequestServices.GetService<ILoggerService>();
            if (loggerService == null)
            {
                var loggerFactory = context.RequestServices.GetService<ILoggerFactory>();
                var jsonSerializer = context.RequestServices.GetService<IJsonSerializer>();
                loggerService = new DefaultLoggerService(loggerFactory, jsonSerializer);
            }

            loggerService.Write(new AuditLogInfo
            {
                ServiceName = serviceName,
                TraceId = Activity.Current != null ? Activity.Current.TraceId.ToString() : context.TraceIdentifier,
                ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds,
                StartTime = startTime,
                EndTime = endTime,
                LogLevel = context.Response.StatusCode == 200 ? LogLevel.Information : LogLevel.Error,
                Route = request.Path,
                HttpMethod = request.Method,
                RequestBody = reqBody,
                ResponseBody = respBody,
                RawData = JsonSerializer.Serialize(reqHeaders),
                StatusCode = context.Response.StatusCode,
                UserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                UserName = context.User.FindFirst(ClaimTypes.Name)?.Value,
                UserAgent = request.Headers["User-Agent"],
                IpAddress = request.Headers["X-Real-IP"],
                AliasName = "AuditLog"
            });
            return Task.CompletedTask;
        });

        #endregion
    }

    /// <summary>
    /// 判断是否应该记录请求日志
    /// </summary>
    private bool ShouldLogRequest(HttpRequest request, string path)
    {
        // 检查是否只记录 API 路由
        if (_options.LogOnlyApiRoutes &&
            !path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // 检查 HTTP 方法
        if (_options.IncludeHttpMethods.Count > 0)
        {
            return _options.IncludeHttpMethods.Contains(
                request.Method.ToUpperInvariant(),
                StringComparer.OrdinalIgnoreCase);
        }

        return true;
    }

    /// <summary>
    /// 获取请求体
    /// </summary>
    private async Task<string> GetRequestBodyAsync(HttpRequest request)
    {
        return request.Method.ToUpperInvariant() switch
        {
            "POST" or "PUT" or "PATCH" => await ReadRequestBodyAsync(request),
            "GET" or "DELETE" => request.QueryString.HasValue ? request.QueryString.Value! : "",
            _ => ""
        };
    }

    /// <summary>
    /// 读取请求体
    /// </summary>
    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        try
        {
            request.EnableBuffering();
            request.Body.Position = 0;

            using var reader = new StreamReader(
                request.Body,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            return FormatBody(body);
        }
        catch
        {
            return "[无法读取请求体]";
        }
    }

    /// <summary>
    /// 获取响应体
    /// </summary>
    private async Task<string> GetResponseBodyAsync(HttpResponse response, MemoryStream responseBody)
    {
        try
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            // 检查响应体大小
            if (body.Length > _options.MaxResponseBodySize)
            {
                return $"[响应体过大，已截断。原始大小: {body.Length} 字节，前 {_options.MaxResponseBodySize} 字节:]\n" +
                       body.Substring(0, _options.MaxResponseBodySize);
            }

            return FormatBody(body);
        }
        catch
        {
            return "[无法读取响应体]";
        }
    }

    /// <summary>
    /// 格式化请求/响应体
    /// </summary>
    private string FormatBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return body;

        // 如果启用格式化且是有效的 JSON，尝试美化输出
        if (_options.FormatJson)
        {
            var trimmed = body.TrimStart();
            if (trimmed.StartsWith('{') || trimmed.StartsWith('['))
            {
                try
                {
                    var jsonDoc = JsonDocument.Parse(body);
                    return JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                }
                catch
                {
                    // 不是有效的 JSON，返回原始内容
                    return body;
                }
            }
        }

        return body;
    }
}
