namespace AuthenticationApiSample.Auths;

public static class AuthExtension
{
    public static IServiceCollection AddMyAuthentication(this IServiceCollection services,
                                                         IConfiguration configuration)
    {
        services.AddAuthentication(x =>
                {
                    // 在未指定身份验证方案时将使用的默认身份验证方案。如果没有显式指定身份验证方案，系统将使用 DefaultScheme 所指定的方案。
                    x.DefaultScheme = CustomerAuthenticationHandler.CustomerSchemeName;

                    // 定义了用于验证身份的默认身份验证方案。这通常用于处理来自客户端的身份验证请求，例如用户登录时提交的凭证
                    x.DefaultAuthenticateScheme = CustomerAuthenticationHandler.CustomerSchemeName;
                    x.DefaultChallengeScheme = CustomerAuthenticationHandler.CustomerSchemeName;
                    x.DefaultForbidScheme = CustomerAuthenticationHandler.CustomerSchemeName;
                    x.AddScheme<CustomerAuthenticationHandler>(CustomerAuthenticationHandler.CustomerSchemeName,
                        CustomerAuthenticationHandler.CustomerSchemeName);
                })
                .AddJwtBearerAuthentication(jwtBearerEventsAction: (option) =>
                {
                    option.OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        // 如果是 SignalR 请求且包含 access_token，则从查询参数读取
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    };
                })
                .AddBasicAuthentication(options =>
                {
                    options.UserName = "admin";
                    options.Password = "123456";
                });

        return services;
    }
}