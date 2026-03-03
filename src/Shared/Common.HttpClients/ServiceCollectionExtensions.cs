using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Fallback;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Common.HttpClients
{
    /// <summary>
    /// 服务集合扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加HTTP客户端服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configure">配置委托</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddHttpClientService(this IServiceCollection services, Action<HttpClientOptions> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var opt = new HttpClientOptions();
            configure.Invoke(opt);

            opt.AdditionalSensitiveHeaders ??= new List<string>();
            opt.AdditionalSensitiveFields ??= new List<string>();

            if (opt.Timeout < 1 || opt.Timeout > 3600)
            {
                throw new ArgumentOutOfRangeException(nameof(opt.Timeout), opt.Timeout, "Timeout必须在1-3600秒之间");
            }

            if (opt.MaxOutputResponseLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(opt.MaxOutputResponseLength), opt.MaxOutputResponseLength,
                    "MaxOutputResponseLength不能小于0");
            }

            if (opt.ConcurrencyLimit < 1 || opt.ConcurrencyLimit > 10000)
            {
                throw new ArgumentOutOfRangeException(nameof(opt.ConcurrencyLimit), opt.ConcurrencyLimit,
                    "ConcurrencyLimit必须在1-10000之间");
            }

            if (opt.MaxRetryAttempts < 0 || opt.MaxRetryAttempts > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(opt.MaxRetryAttempts), opt.MaxRetryAttempts,
                    "MaxRetryAttempts必须在0-10之间");
            }

            if (opt.RetryDelaySeconds < 1 || opt.RetryDelaySeconds > 300)
            {
                throw new ArgumentOutOfRangeException(nameof(opt.RetryDelaySeconds), opt.RetryDelaySeconds,
                    "RetryDelaySeconds必须在1-300之间");
            }

            // 配置HttpClient选项
            services.Configure(configure);

            // 注册 HttpContextAccessor（如果还没有注册）
            services.AddHttpContextAccessor();

            // 注册日志处理器
            services.AddTransient<LoggingHandler>();

            // 配置HttpClient，包含处理器和弹性策略
            var clientBuilder = services.AddHttpClient<IHttpHelper, HttpClientHelper>("default")

                                        // 配置主要的消息处理器，处理SSL证书验证
                                         .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                                         {
                                             var config = serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>().Value;
                                             var handler = new HttpClientHandler();

                                            // 根据配置决定是否忽略不安全证书
                                            if (config.IgnoreUntrustedCertificate)
                                            {
                                                handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
                                            }

                                            return handler;
                                        })

                                        // 配置 HttpClient，将 Timeout 设置为无限，让 Polly 的超时策略完全控制超时行为
                                        .ConfigureHttpClient(client =>
                                        {
                                            client.Timeout = Timeout.InfiniteTimeSpan;
                                        })

                                        // 添加日志处理器在内层，每次请求（包括重试）都会记录
                                        .AddHttpMessageHandler<LoggingHandler>();

            // 添加弹性策略处理器，按照依赖注入顺序：从外层到内层执行
            clientBuilder.AddResilienceHandler("defaultHandler", (builder, handler) =>
            {
                // 获取HTTP配置选项
                var httpOptions = handler.ServiceProvider.GetRequiredService<IOptions<HttpClientOptions>>().Value;

                // 添加弹性策略链，按以下顺序执行（从外层到内层）：

                // 1. 降级处理策略 - 当所有策略都失败时的最后保障
                builder.AddFallback(new FallbackStrategyOptions<HttpResponseMessage>()
                                    {
                                        ShouldHandle = args =>
                                        {
                                            if (args.Context.CancellationToken.IsCancellationRequested)
                                            {
                                                return ValueTask.FromResult(false);
                                            }

                                            var ex = args.Outcome.Exception;
                                            var shouldHandle = ex is HttpRequestException or TaskCanceledException or TimeoutException or TimeoutRejectedException;
                                            return ValueTask.FromResult(shouldHandle);
                                        },
                                        FallbackAction = args =>
                                        {
                                            // 如果配置了不抛出异常,返回一个服务不可用响应,添加标识头以区分真实服务端响应
                                            // 否则保持原始异常,让它向上传播
                                             if (!httpOptions.FailThrowException)
                                             {
                                                 return Outcome.FromResultAsValueTask(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                                                                                      {
                                                                                          Content = new StringContent("Fallback: request failed."),
                                                                                          Headers = { { "X-Fallback-Response", "true" } }
                                                                                      });
                                             }

                                            // 如果配置要抛出异常,则重新抛出原始异常
                                            return Outcome.FromExceptionAsValueTask<HttpResponseMessage>(args.Outcome.Exception!);
                                        }
                                    })

                       // 2. 并发限制策略 - 限制同时进行的HTTP请求数量，防止资源耗尽
                       .AddConcurrencyLimiter(httpOptions.ConcurrencyLimit)

                       // 3. 重试策略 - 根据配置决定是否重试，最多重试3次，使用指数退避
                       // 注意：重试策略放在超时策略之前（外层），这样超时异常会被重试策略捕获并触发重试
                        .AddRetry(new HttpRetryStrategyOptions
                                  {
                                      // 最大重试次数由配置控制
                                      MaxRetryAttempts = httpOptions.MaxRetryAttempts,

                                      // 初始延迟时间由配置控制
                                      Delay = TimeSpan.FromSeconds(httpOptions.RetryDelaySeconds),

                                      // 指数退避策略，避免对服务器造成过大压力
                                      BackoffType = DelayBackoffType.Exponential,

                                      // 自定义判断是否需要重试的条件
                                      ShouldHandle = args =>
                                      {
                                          if (args.Context.CancellationToken.IsCancellationRequested)
                                          {
                                              return ValueTask.FromResult(false);
                                          }

                                          if (args.Outcome.Exception != null)
                                          {
                                              var shouldRetryException =
                                                  args.Outcome.Exception is HttpRequestException or TaskCanceledException or TimeoutException or TimeoutRejectedException;
                                              return ValueTask.FromResult(shouldRetryException);
                                          }

                                          var response = args.Outcome.Result;
                                          if (response == null)
                                          {
                                              return ValueTask.FromResult(false);
                                          }

                                          if (httpOptions.RetryOnUnauthorized && response.StatusCode == HttpStatusCode.Unauthorized)
                                          {
                                              return ValueTask.FromResult(true);
                                          }

                                          // 默认的重试条件（5xx服务器错误和408请求超时）
                                          var shouldRetryStatusCode = response.StatusCode >= HttpStatusCode.InternalServerError ||
                                                                      response.StatusCode == HttpStatusCode.RequestTimeout;
                                          return ValueTask.FromResult(shouldRetryStatusCode);
                                      }
                                  })

                       // 4. 熔断器策略 - 当错误率达到阈值时暂时停止请求，保护系统
                       .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions())

                       // 5. 超时策略 - 防止请求长时间阻塞,使用配置的超时时间
                       // 注意：超时策略放在最内层，这样每次重试都会应用超时限制
                       .AddTimeout(new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromSeconds(httpOptions.Timeout) });
            });

            return services;
        }

        /// <summary>
        /// 添加HTTP客户端服务（使用默认配置）
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddHttpClientService(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddHttpClientService(config =>
            {
                config.AuditLog = true;
                config.EnableLogRedaction = true;
                config.FailThrowException = false;
                config.Timeout = 100;
                config.MaxOutputResponseLength = 1024 * 1024; // 1MB
                config.ConcurrencyLimit = 100;
                config.MaxRetryAttempts = 3;
                config.RetryDelaySeconds = 1;
            });
        }
    }
}
