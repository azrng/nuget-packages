using Azrng.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// JWT Bearer 认证服务扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private const string UnauthorizedResponseMessage = "{\"isSuccess\":false,\"message\":\"您无权访问该接口，请确保已经登录\",\"code\":\"401\"}";

        /// <summary>
        /// 添加 JWT Bearer 认证
        /// </summary>
        /// <param name="builder">认证构建器</param>
        /// <param name="jwtConfigAction">JWT Token 配置</param>
        /// <param name="jwtBearerEventsAction">JwtBearerEvents 自定义配置（可选，用于扩展默认配置，不会覆盖默认事件）</param>
        public static AuthenticationBuilder AddJwtBearerAuthentication(
            this AuthenticationBuilder builder,
            Action<JwtTokenConfig>? jwtConfigAction = null,
            Action<JwtBearerEvents>? jwtBearerEventsAction = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var config = new JwtTokenConfig();
            jwtConfigAction?.Invoke(config);

            // 验证密钥长度和复杂度
            if (string.IsNullOrWhiteSpace(config.JwtSecretKey))
                throw new ArgumentException("JWT 密钥不能为空");

            if (config.JwtSecretKey.Length < 32)
                throw new ArgumentException("JWT 密钥长度必须至少为 32 位字符，以确保安全性");

            // 检查密钥复杂度（避免全为相同字符或简单模式）
            if (config.JwtSecretKey.Distinct().Count() < 8)
                throw new ArgumentException("JWT 密钥复杂度不足，请使用包含多种字符的密钥");

            builder.Services.AddScoped<IBearerAuthService, JwtBearerAuthService>();
            if (jwtConfigAction is not null)
                builder.Services.Configure(jwtConfigAction);

            #region 添加验证/认证服务

            builder.AddJwtBearer(o => // 认证
            {
                o.Challenge = JwtBearerDefaults.AuthenticationScheme;
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                                              {
                                                  // 是否开启签名认证
                                                  ValidateIssuerSigningKey = true,
                                                  // 使用 UTF-8 编码以支持更广泛的字符集
                                                  IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.JwtSecretKey)),

                                                  // 发行人验证，这里要和 token 类中 Claim 类型的发行人保持一致
                                                  ValidateIssuer = true,
                                                  ValidIssuer = config.JwtIssuer, // 发行人

                                                  // 接收人验证
                                                  ValidateAudience = true,
                                                  ValidAudience = config.JwtAudience, // 订阅人

                                                  RequireExpirationTime = true,
                                                  ValidateLifetime = true,
                                              };

                // 保存默认的 Events，如果用户自定义了 Events，可以基于默认的进行扩展
                var defaultEvents = new JwtBearerEvents
                                    {
                                        // 如果 jwt 过期，那么就先走这个失败的方法，再走 OnChallenge
                                        OnAuthenticationFailed = content => // 过期时候的场景，会给返回头增加标识
                                        {
                                            if (content.Exception.GetType() == typeof(SecurityTokenExpiredException))
                                            {
                                                content.Response.Headers.Add("Token-Expired", "true");
                                            }

                                            return Task.CompletedTask;
                                        },

                                        // 验证失败自定义返回类
                                        OnChallenge = async context =>
                                        {
                                            // 跳过默认的处理逻辑，返回下面的模型数据
                                            context.HandleResponse();

                                            context.Response.ContentType = "application/json;charset=utf-8";
                                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                                            await context.Response.WriteAsync(UnauthorizedResponseMessage);
                                        }
                                    };

                // 先设置默认 Events
                o.Events = defaultEvents;

                // 允许用户自定义配置（在默认配置基础上进行扩展）
                jwtBearerEventsAction?.Invoke(o.Events);
            });

            #endregion 添加验证/认证服务

            return builder;
        }
    }
}