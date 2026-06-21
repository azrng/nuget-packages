using Common.HttpClients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection.Logging;

namespace Common.HttpClients.Next.Test;

/// <summary>
/// 测试主机启动配置：注册 Apifox Echo（https://echo.apifox.com）的命名 IHttpHelper 客户端。
/// 参考 DevLogDashboard.Test 的 Startup 写法（Xunit.DependencyInjection 依赖注入）。
/// </summary>
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // 日志输出到 xunit 测试输出
        services.AddLogging(x => x.AddXunitOutput());

        // 默认 Echo 客户端：失败不抛异常，返回结构化 IHttpResult；低重试以加快测试
        services.AddHttpClientService("apifox", options =>
        {
            options.BaseAddress = "https://echo.apifox.com/";
            options.FailThrowException = false;
            options.AuditLog = false;
            options.EnableLogRedaction = false;
            options.Timeout = 30;
            options.MaxRetryAttempts = 1;
            options.RetryDelaySeconds = 1;
        });

        // 失败抛异常的客户端：用于测试 FailThrowException = true 的异常路径
        services.AddHttpClientService("apifox-throw", options =>
        {
            options.BaseAddress = "https://echo.apifox.com/";
            options.FailThrowException = true;
            options.AuditLog = false;
            options.EnableLogRedaction = false;
            options.Timeout = 30;
            options.MaxRetryAttempts = 1;
            options.RetryDelaySeconds = 1;
        });

        // 短超时客户端（2s）：用于 /delay 超时测试；失败不抛异常，由 Fallback 兜底为 503
        services.AddHttpClientService("apifox-timeout", options =>
        {
            options.BaseAddress = "https://echo.apifox.com/";
            options.FailThrowException = false;
            options.AuditLog = false;
            options.EnableLogRedaction = false;
            options.Timeout = 2;
            options.MaxRetryAttempts = 0;
            options.RetryDelaySeconds = 1;
        });

        // 短超时 + 抛异常客户端：用于超时抛异常路径测试
        services.AddHttpClientService("apifox-timeout-throw", options =>
        {
            options.BaseAddress = "https://echo.apifox.com/";
            options.FailThrowException = true;
            options.AuditLog = false;
            options.EnableLogRedaction = false;
            options.Timeout = 2;
            options.MaxRetryAttempts = 0;
            options.RetryDelaySeconds = 1;
        });
    }
}
