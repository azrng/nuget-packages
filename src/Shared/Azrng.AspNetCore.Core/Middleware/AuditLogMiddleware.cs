using Azrng.AspNetCore.Core.AuditLog;
using Azrng.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;

namespace Azrng.AspNetCore.Core.Middleware;

/// <summary>
/// 审计日志中间件
/// </summary>
public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Stopwatch _stopwatch;
    private readonly List<string> _ignoreFilterRoutes;

    public AuditLogMiddleware(RequestDelegate next, List<string>? ignoreFilterRoutes = null)
    {
        _next = next;
        _ignoreFilterRoutes = ignoreFilterRoutes ?? new List<string>();
        _stopwatch = new Stopwatch();
    }

    public async Task Invoke(HttpContext context)
    {
        var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

        #region 前置检查

        var path = context.Request.Path.ToString();

        // 1、默认get为读取数据、2、不是api开头的接口不记录日志、3、不是全量日志的接口不记录日志
        if (path.Contains("get") || !path.StartsWith("/api/", StringComparison.CurrentCultureIgnoreCase))
        {
            await _next(context);
            return;
        }

        #endregion

        // 读取需要过滤的路由
        if (_ignoreFilterRoutes.Count > 0)
        {
            if (_ignoreFilterRoutes.Any(p => path.Contains(p, StringComparison.CurrentCultureIgnoreCase)))
            {
                await _next(context);
                return;
            }
        }

        #region 记录请求日志相关信息

        _stopwatch.Restart();
        var startTime = DateTime.Now;
        var request = context.Request;
        var reqHeaders = request
                         .Headers
                         .ToDictionary(x => x.Key,
                             v => string.Join(";", v.Value.ToList()));
        var reqBody = "";
        switch (request.Method.ToLowerInvariant())
        {
            case "post":
            case "put":
                {
                    context.Request.EnableBuffering();
                    var reqStream = new StreamReader(context.Request.Body);
                    reqBody = await reqStream.ReadToEndAsync();
                    context.Request.Body.Seek(0, SeekOrigin.Begin);
                    break;
                }
            case "get":
            case "delete":
                reqBody = (request.QueryString.HasValue ? request.QueryString.Value : "")!;
                break;
        }

        reqBody = reqBody.Replace("\n", "").Replace("\t", "");
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        await _next(context);
        var respBody = await GetResponse(context.Response);
        respBody = respBody.Replace("\n", "").Replace("\t", "");

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
                var loggerFactory =
                    context.RequestServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                var jsonSerializer = context.RequestServices.GetService(typeof(IJsonSerializer)) as IJsonSerializer;
                loggerService = new DefaultLoggerService(loggerFactory, jsonSerializer);
            }

            loggerService.Write(new AuditLogInfo
                                {
                                    ServiceName = serviceName,
                                    TraceId = Activity.Current != null ? Activity.Current.TraceId.ToString() : context.TraceIdentifier,
                                    ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds,
                                    StartTime = startTime,
                                    EndTime = endTime,
                                    LogLevel =
                                        context.Response.StatusCode == 200
                                            ? LogLevel.Information
                                            : LogLevel.Error,
                                    Route = request.Path,
                                    HttpMethod = request.Method,
                                    RequestBody = reqBody,
                                    ResponseBody = respBody,
                                    RawData = JsonConvert.SerializeObject(reqHeaders),
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

    private static async Task<string> GetResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);
        return text;
    }
}