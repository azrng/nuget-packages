using System.Text;
using Azrng.SettingConfig.Service;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Azrng.SettingConfig.Test;

public class MiddlewareAndBuilderTests
{
    [Fact]
    public async Task UseSettingDashboard_ShouldInitializeDataSourceProviderAndRegisterMiddleware()
    {
        var fakeProvider = new FakeDataSourceProvider();
        var services = new ServiceCollection();
        services.AddSingleton<IDataSourceProvider>(fakeProvider);
        services.AddSingleton<IOptions<DashboardOptions>>(Options.Create(TestInfrastructure.CreateOptions(x =>
        {
            x.Authorization = [new AllowAllAuthorizationFilter()];
        })));
        services.AddSingleton<ManifestResourceService>();
        services.AddSingleton<IWebHostEnvironment, TestWebHostEnvironment>();
        services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(_ => { }));

        var serviceProvider = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(serviceProvider);
        var app = appBuilder.UseSettingDashboard().Build();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Path = "/systemSetting";
        httpContext.Response.Body = new MemoryStream();

        await app(httpContext);

        fakeProvider.InitCallCount.Should().Be(1);
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status301MovedPermanently);
        httpContext.Response.Headers.Location.ToString().Should().Be("systemSetting/index.html");
    }

    [Fact]
    public async Task Invoke_WithAuthorizedIndexRequest_ShouldReturnHtmlAndSecurityHeaders()
    {
        var middleware = CreateMiddleware(TestInfrastructure.CreateOptions(x =>
        {
            x.Authorization = [new AllowAllAuthorizationFilter()];
            x.PageTitle = "Runtime Title";
            x.PageDescription = "Runtime Description";
            x.ApiRoutePrefix = "/api/runtime";
        }));
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Path = "/systemSetting/index.html";
        httpContext.Response.Body = new MemoryStream();

        await middleware.Invoke(httpContext);
        httpContext.Response.Body.Position = 0;
        var html = Encoding.UTF8.GetString(((MemoryStream)httpContext.Response.Body).ToArray());

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        httpContext.Response.ContentType.Should().Be("text/html;charset=utf-8");
        httpContext.Response.Headers["Content-Security-Policy"].ToString().Should().Contain("default-src 'self'");
        httpContext.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
        httpContext.Response.Headers["X-Frame-Options"].ToString().Should().Be("SAMEORIGIN");
        html.Should().Contain("Runtime Title");
        html.Should().Contain("Runtime Description");
        html.Should().Contain("/api/runtime");
        html.Should().NotContain("%(PageTitle)%");
    }

    [Fact]
    public async Task Invoke_WithUnauthorizedIndexRequest_ShouldReturn401()
    {
        var middleware = CreateMiddleware(TestInfrastructure.CreateOptions(x =>
        {
            x.Authorization = [new DenyAllAuthorizationFilter()];
        }));
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Path = "/systemSetting/index.html";
        httpContext.Response.Body = new MemoryStream();

        await middleware.Invoke(httpContext);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        ((MemoryStream)httpContext.Response.Body).Length.Should().Be(0);
    }

    private static AspNetCoreDashboardMiddleware CreateMiddleware(DashboardOptions options)
    {
        return new AspNetCoreDashboardMiddleware(
            _ => Task.CompletedTask,
            new TestWebHostEnvironment(),
            LoggerFactory.Create(_ => { }),
            Options.Create(options),
            new ManifestResourceService());
    }
}
