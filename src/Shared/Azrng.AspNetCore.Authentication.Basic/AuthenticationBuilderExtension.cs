using Azrng.AspNetCore.Authentication.Basic;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Basic 认证构造器扩展方法
    /// </summary>
    public static class AuthenticationBuilderExtension
    {
        /// <summary>
        /// 添加 Basic 认证（使用默认的 Claims 验证器）
        /// </summary>
        /// <param name="builder">认证构建器</param>
        /// <param name="configureOptions">Basic 认证配置</param>
        /// <returns>认证构建器</returns>
        /// <remarks>
        /// 此方法使用默认的 <see cref="DefaultBasicAuthorizeVerify"/>，仅返回包含用户名的 Claim
        /// 如需自定义 Claims，请使用 <see cref="AddBasicAuthentication{T}(AuthenticationBuilder, Action{BasicOptions})"/> 泛型方法
        /// </remarks>
        public static AuthenticationBuilder AddBasicAuthentication(
            this AuthenticationBuilder builder,
            Action<BasicOptions> configureOptions)
        {
            return builder.AddBasicAuthentication<DefaultBasicAuthorizeVerify>(configureOptions);
        }

        /// <summary>
        /// 添加 Basic 认证（使用自定义的 Claims 验证器）
        /// </summary>
        /// <typeparam name="T">自定义的 <see cref="IBasicAuthorizeVerify"/> 实现类型</typeparam>
        /// <param name="builder">认证构建器</param>
        /// <param name="configureOptions">Basic 认证配置</param>
        /// <returns>认证构建器</returns>
        /// <remarks>
        /// 使用此方法可以自定义用户认证后的 Claims 生成逻辑
        /// </remarks>
        /// <example>
        /// 示例：使用自定义验证器
        /// <code>
        /// services.AddAuthentication()
        ///     .AddBasicAuthentication&lt;MyCustomVerify&gt;(options =>
        ///     {
        ///         options.UserName = "admin";
        ///         options.Password = "123456";
        ///     });
        /// </code>
        /// </example>
        public static AuthenticationBuilder AddBasicAuthentication<T>(
            this AuthenticationBuilder builder,
            Action<BasicOptions> configureOptions) where T : class, IBasicAuthorizeVerify
        {
            builder.Services.AddScoped<IBasicAuthorizeVerify, T>();
            builder.Services.Configure(configureOptions);

            return builder.AddScheme<BasicOptions, BasicAuthenticationHandler>(BasicAuthentication.AuthenticationSchema, configureOptions);
        }
    }
}