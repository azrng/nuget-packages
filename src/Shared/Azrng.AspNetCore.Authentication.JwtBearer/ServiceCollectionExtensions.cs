using Azrng.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// JWT Bearer 认证服务扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        internal const string UnauthorizedResponseMessage = "{\"isSuccess\":false,\"message\":\"您无权访问该接口，请确保已经登录\",\"code\":\"401\"}";

        /// <summary>
        /// 添加 JWT Bearer 认证
        /// </summary>
        /// <param name="builder">认证构建器</param>
        /// <param name="jwtConfigAction">JWT Token 配置</param>
        /// <param name="jwtBearerEventsAction">JwtBearerEvents 自定义配置（可选，用于扩展默认配置，不会覆盖默认事件）</param>
        /// <param name="useDefaultChallengeResponse">是否使用库内置的自定义 401 响应体（默认 true，保持向后兼容）；设为 false 则回退到 ASP.NET Core 默认 401 行为（保留 WWW-Authenticate 头）</param>
        public static AuthenticationBuilder AddJwtBearerAuthentication(
            this AuthenticationBuilder builder,
            Action<JwtTokenConfig>? jwtConfigAction = null,
            Action<JwtBearerEvents>? jwtBearerEventsAction = null,
            bool useDefaultChallengeResponse = true)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            // 注册配置：既允许通过 Action 显式提供，也支持后续从 IConfiguration 绑定
            if (jwtConfigAction is not null)
                builder.Services.Configure(jwtConfigAction);

            // 统一注册配置校验器：无论通过哪条注册路径，首次解析 IOptions<JwtTokenConfig> 时都会强制校验
            // ValidateOnStart 确保应用启动时即暴露配置问题，而非运行时才报错
            builder.Services.AddSingleton<IValidateOptions<JwtTokenConfig>, JwtTokenConfigValidator>();
            builder.Services.AddOptions<JwtTokenConfig>()
                            .Validate(o => !string.IsNullOrWhiteSpace(o.JwtSecretKey)
                                           && o.JwtSecretKey.Length >= 32
                                           && o.JwtSecretKey.Distinct().Count() >= 8,
                                "JWT 密钥不满足安全要求：非空、长度至少 32 位、至少 8 种不同字符")
                            .ValidateOnStart();

            // 服务无状态，使用 Singleton；依赖 IOptionsMonitor 支持配置热更新
            // TryAddSingleton 避免重复注册
            builder.Services.TryAddSingleton<IBearerAuthService, JwtBearerAuthService>();

            // 注册 JwtBearerOptions 配置器：在运行时从已校验的 JwtTokenConfig 填充
            // 中间件与 IBearerAuthService 共享同一份校验规则
            builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>>(sp =>
                new JwtBearerOptionsSetup(
                    sp.GetRequiredService<IOptions<JwtTokenConfig>>(),
                    jwtBearerEventsAction,
                    useDefaultChallengeResponse));
            builder.Services.AddSingleton<IConfigureNamedOptions<JwtBearerOptions>>(sp =>
                new JwtBearerOptionsSetup(
                    sp.GetRequiredService<IOptions<JwtTokenConfig>>(),
                    jwtBearerEventsAction,
                    useDefaultChallengeResponse));

            // 注册 JwtBearer 认证中间件（不在此处配置 TVP/Events，由上面的 ConfigureOptions 负责）
            builder.AddJwtBearer();

            return builder;
        }
    }
}
