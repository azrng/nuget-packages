using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

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

        public JwtBearerOptionsSetup(
            IOptions<JwtTokenConfig> jwtOptions,
            Action<JwtBearerEvents>? jwtBearerEventsAction)
        {
            _jwtOptions = jwtOptions;
            _jwtBearerEventsAction = jwtBearerEventsAction;
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

            if (_jwtBearerEventsAction is not null)
            {
                o.Events ??= new JwtBearerEvents();
                _jwtBearerEventsAction.Invoke(o.Events);
            }
        }
    }
}
