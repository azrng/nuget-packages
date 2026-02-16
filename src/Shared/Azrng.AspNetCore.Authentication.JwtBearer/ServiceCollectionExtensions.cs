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
        /// 添加jwt Bearer认证
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="action">jwt配置</param>
        public static AuthenticationBuilder AddJwtBearerAuthentication(this AuthenticationBuilder builder,
                                                                       Action<JwtTokenConfig> action = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var config = new JwtTokenConfig();
            action?.Invoke(config);
            if (config.JwtSecretKey.Length < 16)
                throw new ArgumentException("密钥必须请大于16位");

            builder.Services.AddScoped<IBearerAuthService, JwtBearerAuthService>();
            if (action is not null)
                builder.Services.Configure(action);

            #region 添加验证/认证服务

            builder.AddJwtBearer(o => //认证
            {
                o.Challenge = JwtBearerDefaults.AuthenticationScheme;
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                                              {
                                                  // 是否开启签名认证
                                                  ValidateIssuerSigningKey = true,
                                                  IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.JwtSecretKey)),

                                                  // 发行人验证，这里要和token类中Claim类型的发行人保持一致
                                                  ValidateIssuer = true,
                                                  ValidIssuer = config.JwtIssuer, //发行人

                                                  // 接收人验证
                                                  ValidateAudience = true,
                                                  ValidAudience = config.JwtAudience, //订阅人

                                                  RequireExpirationTime = true,
                                                  ValidateLifetime = true,

                                                  // ClockSkew = TimeSpan.Zero
                                              };

                o.Events = new JwtBearerEvents
                           {
                               //如果jwt过期  那么就先走这个失败的方法，再走OnChallenge
                               OnAuthenticationFailed = content => //过期时候的场景，会给返回头增加标识
                               {
                                   if (content.Exception.GetType() == typeof(SecurityTokenExpiredException))
                                   {
                                       content.Response.Headers.Add("Token-Expired", "true");
                                   }

                                   return Task.CompletedTask;
                               },

                               //验证失败自定义返回类
                               OnChallenge = async context =>
                               {
                                   // 跳过默认的处理逻辑，返回下面的模型数据
                                   context.HandleResponse();

                                   context.Response.ContentType = "application/json;charset=utf-8";
                                   context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                                   await context.Response.WriteAsync(UnauthorizedResponseMessage);
                               }
                           };
            });

            #endregion 添加验证/认证服务

            return builder;
        }
    }
}