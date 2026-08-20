using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using System.Net;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// IApplicationBuilder扩展
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 使用 CORS 策略
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="policyName">策略名称，默认为 "DefaultCors"</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app, string policyName = "DefaultCors")
    {
        app.UseCors(policyName);
        return app;
    }

    /// <summary>
    /// 启用 Body 重复读功能
    /// </summary>
    /// <remarks>须在 app.UseRouting() 之前注册</remarks>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseRequestBodyRepetitionRead(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            context.Request.EnableBuffering();
            await next.Invoke();
        });
    }

    /// <summary>
    /// 启用转发头处理，使反向代理（Nginx/网关）后的应用获取真实客户端 IP 与请求协议
    /// </summary>
    /// <param name="app">应用构建器</param>
    /// <param name="knownProxies">可信代理 IP 列表，不传时仅信任本机回环，传入后回环信任仍保留</param>
    /// <returns>应用构建器</returns>
    /// <remarks>
    /// 须在管道靠前位置注册（UseRouting 之前）。
    /// 仅当请求来源命中可信代理列表时才消费 X-Forwarded-For/X-Forwarded-Proto，外部直连请求伪造的转发头会被忽略。
    /// 需要按网段配置等高级场景时，请直接使用框架原生 ForwardedHeaders 中间件。
    /// </remarks>
    public static IApplicationBuilder UseForwardedHeaders(this IApplicationBuilder app,
                                                          params string[] knownProxies)
    {
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };

        if (knownProxies.Length > 0)
        {
            // 覆盖默认仅信任回环的代理列表，并放开跳数限制以支持多层代理链
            options.KnownProxies.Clear();
            options.ForwardLimit = null;

            foreach (var proxy in knownProxies)
            {
                if (IPAddress.TryParse(proxy, out var address))
                {
                    options.KnownProxies.Add(address);
                }
                else
                {
                    throw new ArgumentException($"无效的可信代理地址：{proxy}，仅支持单个 IP", nameof(knownProxies));
                }
            }
        }

        return app.UseMiddleware<ForwardedHeadersMiddleware>(Options.Create(options));
    }
}