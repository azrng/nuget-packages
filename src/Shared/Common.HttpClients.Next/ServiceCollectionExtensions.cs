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

            var opt = new HttpClientOptions();
            configure.Invoke(opt);

            ValidateOptions(opt);

            // 注册该名称的 HttpClientOptions
            services.Configure(name, configure);

            // 首次注册时添加公共依赖
            services.TryAddSingleton<IHttpHelperFactory, HttpHelperFactory>();
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IHttpLogRedactor, DefaultHttpLogRedactor>();
            services.TryAddTransient<LoggingHandler>();

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
                })

                .AddHttpMessageHandler<LoggingHandler>();

            // 添加弹性策略处理器
            clientBuilder.AddResilienceHandler($"{name}_handler", (builder, handler) =>
            {
                var httpOptions = handler.ServiceProvider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>().Get(name);

                // 1. 降级处理策略
                builder.AddFallback(new FallbackStrategyOptions<HttpResponseMessage>()
                {
                    ShouldHandle = args =>
                    {
                        if (args.Context.CancellationToken.IsCancellationRequested)
                        {
                            return ValueTask.FromResult(false);
                        }

                        var ex = args.Outcome.Exception;
                        if (ex is HttpRequestException or TaskCanceledException or TimeoutException or TimeoutRejectedException)
                        {
                            return ValueTask.FromResult(true);
                        }

                        var response = args.Outcome.Result;
                        if (response != null && !response.IsSuccessStatusCode)
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
                                Headers = { { "X-Fallback-Response", "true" } }
                            });
                        }

                        return Outcome.FromExceptionAsValueTask<HttpResponseMessage>(args.Outcome.Exception!);
                    }
                })

                // 2. 并发限制策略
                .AddConcurrencyLimiter(httpOptions.ConcurrencyLimit)

                // 3. 重试策略
                .AddRetry(new HttpRetryStrategyOptions
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
                })

                // 4. 熔断器策略
                .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions())

                // 5. 超时策略
                .AddTimeout(new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromSeconds(httpOptions.Timeout) });
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

        private static void ValidateOptions(HttpClientOptions opt)
        {
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

            if (opt.MaxRequestBodyLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(opt.MaxRequestBodyLength), opt.MaxRequestBodyLength,
                    "MaxRequestBodyLength不能小于0");
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
        }
    }
}
