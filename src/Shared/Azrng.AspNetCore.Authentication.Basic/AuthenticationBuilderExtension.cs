using Azrng.AspNetCore.Authentication.Basic;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Basic认证构造器扩展方法
    /// </summary>
    public static class AuthenticationBuilderExtension
    {
        /// <summary>
        /// 自定义认证
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static AuthenticationBuilder AddBasicAuthentication(
            this AuthenticationBuilder builder,
            Action<BasicOptions> configureOptions)
        {
            return builder.AddBasicAuthentication<DefaultBasicAuthorizeVerify>(configureOptions);
        }

        /// <summary>
        /// 自定义认证
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
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