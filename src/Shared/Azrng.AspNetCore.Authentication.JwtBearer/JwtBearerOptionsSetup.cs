using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Azrng.AspNetCore.Authentication.JwtBearer
{
    /// <summary>
    /// 在运行时填充 <see cref="JwtBearerOptions"/>，确保中间件与
    /// <see cref="IBearerAuthService"/> 共享同一份已校验的配置与 Token 校验参数
    /// </summary>
    /// <remarks>
    /// 通过 <see cref="IConfigureOptions{TOptions}"/> 在 DI 容器解析
    /// <see cref="JwtBearerOptions"/> 时填充，避免在配置阶段调用
    /// <c>BuildServiceProvider</c>（反模式），也避免硬编码事件逻辑。
    /// </remarks>
    internal class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
    {
        public const string ConfigurationName = "Bearer";

        private readonly IOptions<JwtTokenConfig> _jwtOptions;
        private readonly Action<JwtBearerEvents>? _jwtBearerEventsAction;
        private readonly bool _useDefaultChallengeResponse;

        public JwtBearerOptionsSetup(
            IOptions<JwtTokenConfig> jwtOptions,
            Action<JwtBearerEvents>? jwtBearerEventsAction,
            bool useDefaultChallengeResponse)
        {
            _jwtOptions = jwtOptions;
            _jwtBearerEventsAction = jwtBearerEventsAction;
            _useDefaultChallengeResponse = useDefaultChallengeResponse;
        }

        /// <inheritdoc />
        public void Configure(string? name, JwtBearerOptions options)
        {
            // 仅处理默认的 Bearer 方案，避免影响用户注册的其他 JwtBearer 方案
            if (name is not null && name != ConfigurationName)
                return;

            Configure(options);
        }

        /// <inheritdoc />
        public void Configure(JwtBearerOptions o)
        {
            var config = _jwtOptions.Value;
            var securityKey = JwtTokenValidationParametersFactory.CreateSecurityKey(config.JwtSecretKey);

            o.Challenge = JwtBearerDefaults.AuthenticationScheme;
            o.RequireHttpsMetadata = false;
            o.TokenValidationParameters = JwtTokenValidationParametersFactory.Create(config, securityKey);

            o.Events = new JwtBearerEvents
                       {
                           // Token 过期时先于 OnChallenge 触发
                           OnAuthenticationFailed = context =>
                           {
                               // 使用 is 匹配派生类，避免精确类型匹配漏掉子类
                               if (context.Exception is SecurityTokenExpiredException)
                               {
                                   // Append 避免重复添加同名头抛 ArgumentException
                                   context.Response.Headers.Append("Token-Expired", "true");
                               }

                               return Task.CompletedTask;
                           },

                           OnChallenge = async context =>
                           {
                               if (!_useDefaultChallengeResponse)
                                   return; // 回退到 ASP.NET Core 默认 401 行为

                               // 跳过默认处理逻辑，返回内置模型
                               context.HandleResponse();

                               context.Response.ContentType = "application/json;charset=utf-8";
                               context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                               await context.Response.WriteAsync(ServiceCollectionExtensions.UnauthorizedResponseMessage);
                           }
                       };

            // 允许用户在默认事件基础上扩展（不会覆盖上面的默认事件）
            _jwtBearerEventsAction?.Invoke(o.Events);
        }
    }
}
