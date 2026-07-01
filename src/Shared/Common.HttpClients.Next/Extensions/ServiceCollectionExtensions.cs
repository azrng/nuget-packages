using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
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
        /// 添加命名 HTTP 客户端服务，返回 IHttpClientBuilder 支持链式添加 DelegatingHandler
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="name">客户端名称</param>
        /// <param name="configure">配置委托</param>
        /// <returns>IHttpClientBuilder，支持链式调用 AddHttpMessageHandler 等</returns>
        public static IHttpClientBuilder AddHttpClientService(this IServiceCollection services, string name, Action<HttpClientOptions> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            // 命名 options：configure 只会由 OptionsFactory 触发一次，验证由 IValidateOptions 承担
            services.AddOptions<HttpClientOptions>(name)
                    .Configure(configure);

            // 单次注册全局 IValidateOptions（对所有命名 options 生效）
            services.TryAddSingleton<IValidateOptions<HttpClientOptions>, HttpClientOptionsValidator>();

            // 首次注册时添加公共依赖
            services.TryAddSingleton<IHttpHelperFactory, HttpHelperFactory>();
            services.AddHttpContextAccessor();

            // 注意：不在此处注册 LoggingHandler。
            // LoggingHandler 构造函数需要 string clientName 参数，无法被 DI 容器直接激活；
            // 实际由下方 AddHttpMessageHandler 通过 ActivatorUtilities 注入客户端名称创建。
            // 若直接 TryAddTransient<LoggingHandler>()，在开启 ValidateOnBuild 时会因
            // 无法解析 System.String 而抛激活异常。

            // 配置命名 HttpClient
            var clientBuilder = services.AddHttpClient(name)

                .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                {
                    var config = serviceProvider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>().Get(name);
                    var handler = new HttpClientHandler();

                    if (config.IgnoreUntrustedCertificate)
                    {
                        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
                    }

                    return handler;
                })

                .ConfigureHttpClient((serviceProvider, client) =>
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;

                    var config = serviceProvider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>().Get(name);
                    if (!string.IsNullOrWhiteSpace(config.BaseAddress))
                    {
                        client.BaseAddress = new Uri(config.BaseAddress);
                    }

                    ApplyDefaultHeaders(client, config);
                })

                .AddHttpMessageHandler(sp => ActivatorUtilities.CreateInstance<LoggingHandler>(sp, name));

            // 添加弹性策略处理器（外 -> 内：Fallback -> Timeout -> ConcurrencyLimiter -> CircuitBreaker -> Retry）
            clientBuilder.AddResilienceHandler($"{name}_handler", (builder, handler) =>
            {
                var httpOptions = handler.ServiceProvider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>().Get(name);

                // 1. 降级策略（最外层兜底）
                builder.AddFallback(BuildFallbackOptions(httpOptions))

                // 2. 总超时策略（涵盖整条重试链）
                       .AddTimeout(new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromSeconds(httpOptions.Timeout) });

                // 3. 并发限制策略（0 表示禁用）。permitLimit 决定最大并发，超出部分进入队列等待
                //    （由外层 Timeout 兜底，等待时间不会超过 Timeout）。
                if (httpOptions.ConcurrencyLimit > 0)
                {
                    builder.AddConcurrencyLimiter(
                        permitLimit: httpOptions.ConcurrencyLimit,
                        queueLimit: Math.Max(httpOptions.ConcurrencyLimit * 10, 100));
                }

                // 4. 熔断器策略
                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions());

                // 5. 重试策略（最内层，每次重试由 CircuitBreaker / Timeout 监督；MaxRetryAttempts=0 表示禁用）
                if (httpOptions.MaxRetryAttempts > 0)
                {
                    builder.AddRetry(new HttpRetryStrategyOptions
                    {
                        MaxRetryAttempts = httpOptions.MaxRetryAttempts,
                        Delay = TimeSpan.FromSeconds(httpOptions.RetryDelaySeconds),
                        BackoffType = DelayBackoffType.Exponential,
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

                            var shouldRetryStatusCode = response.StatusCode >= HttpStatusCode.InternalServerError ||
                                                        response.StatusCode == HttpStatusCode.RequestTimeout;
                            return ValueTask.FromResult(shouldRetryStatusCode);
                        }
                    });
                }
            });

            return clientBuilder;
        }

        /// <summary>
        /// 添加 HTTP 客户端服务（使用指定配置，注册为 "default"）
        /// </summary>
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

            services.AddHttpClientService("default", configure);

            // 向后兼容：注册 IHttpHelper 单例指向 "default"
            services.AddSingleton<IHttpHelper>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpHelperFactory>();
                return factory.CreateClient("default");
            });

            return services;
        }

        /// <summary>
        /// 添加 HTTP 客户端服务（使用默认配置）
        /// </summary>
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
                config.MaxRequestBodyLength = 4096;
                config.MaxOutputResponseLength = 4096;
                config.ConcurrencyLimit = 100;
                config.MaxRetryAttempts = 3;
                config.RetryDelaySeconds = 1;
            });
        }

        private static FallbackStrategyOptions<HttpResponseMessage> BuildFallbackOptions(HttpClientOptions httpOptions)
        {
            return new FallbackStrategyOptions<HttpResponseMessage>()
            {
                ShouldHandle = args =>
                {
                    if (args.Context.CancellationToken.IsCancellationRequested)
                    {
                        return ValueTask.FromResult(false);
                    }

                    // 仅在“无法得到真实响应”的异常路径上兜底；
                    // HTTP 5xx 是真实响应，应原样返回（由调用方按 StatusCode 判断）。
                    if (args.Outcome.Exception is HttpRequestException or TaskCanceledException or TimeoutException or TimeoutRejectedException)
                    {
                        return ValueTask.FromResult(true);
                    }

                    return ValueTask.FromResult(false);
                },
                FallbackAction = args =>
                {
                    if (!httpOptions.FailThrowException)
                    {
                        return Outcome.FromResultAsValueTask(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                        {
                            Content = new StringContent("Fallback: request failed."),
                            Headers = { { HttpClientHeaderNames.FallbackResponse, "true" } }
                        });
                    }

                    var exception = args.Outcome.Exception ?? WrapFailedResponseAsException(args.Outcome.Result);
                    return Outcome.FromExceptionAsValueTask<HttpResponseMessage>(exception);
                }
            };
        }

        private static HttpRequestException WrapFailedResponseAsException(HttpResponseMessage? response)
        {
            var statusCode = response?.StatusCode ?? HttpStatusCode.ServiceUnavailable;
            return new HttpRequestException($"Request failed with status code {(int)statusCode} ({statusCode}).", null, statusCode);
        }

        private static void ApplyDefaultHeaders(HttpClient client, HttpClientOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.UserAgent))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", options.UserAgent);
            }

            if (options.DefaultHeaders == null)
            {
                return;
            }

            foreach (var (key, value) in options.DefaultHeaders)
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
                }
            }
        }

        private static bool ValidateOptionsCore(HttpClientOptions opt)
        {
            return HttpClientOptionsValidator.ValidateInstance(opt).Succeeded;
        }
    }

    /// <summary>
    /// 在 OptionsFactory 创建实例时（含 reload）触发的验证器
    /// </summary>
    internal sealed class HttpClientOptionsValidator : IValidateOptions<HttpClientOptions>
    {
        public ValidateOptionsResult Validate(string? name, HttpClientOptions options)
        {
            return ValidateInstance(options);
        }

        public static ValidateOptionsResult ValidateInstance(HttpClientOptions opt)
        {
            opt.AdditionalSensitiveHeaders ??= new List<string>();
            opt.AdditionalSensitiveFields ??= new List<string>();

            var failures = new List<string>();

            if (opt.Timeout < 1 || opt.Timeout > 3600)
            {
                failures.Add("Timeout必须在1-3600秒之间");
            }

            if (opt.MaxOutputResponseLength < 0)
            {
                failures.Add("MaxOutputResponseLength不能小于0");
            }

            if (opt.MaxRequestBodyLength < 0)
            {
                failures.Add("MaxRequestBodyLength不能小于0");
            }

            if (opt.ConcurrencyLimit < 0 || opt.ConcurrencyLimit > 10000)
            {
                failures.Add("ConcurrencyLimit必须在0-10000之间（0 表示禁用）");
            }

            if (opt.MaxRetryAttempts < 0 || opt.MaxRetryAttempts > 10)
            {
                failures.Add("MaxRetryAttempts必须在0-10之间");
            }

            if (opt.RetryDelaySeconds < 1 || opt.RetryDelaySeconds > 300)
            {
                failures.Add("RetryDelaySeconds必须在1-300之间");
            }

            return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
        }
    }
}
