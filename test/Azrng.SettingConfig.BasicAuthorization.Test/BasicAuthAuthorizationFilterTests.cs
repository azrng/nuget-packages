using System.Net;
using System.Reflection;
using System.Text;
using Azrng.SettingConfig;
using Azrng.SettingConfig.BasicAuthorization;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Azrng.SettingConfig.BasicAuthorization.Test;

public class BasicAuthAuthorizationFilterTests
{
    [Fact]
    public void Validate_ShouldHashPasswordAndAcceptCorrectCredentials()
    {
        var user = new BasicAuthAuthorizationUser
        {
            Login = "alice",
            PasswordClear = "secret-password"
        };

        user.Password.Should().NotBeNull();
        user.Password.Should().HaveCount(48);
        user.Validate("alice", "secret-password", loginCaseSensitive: true).Should().BeTrue();
        user.Validate("alice", "wrong", loginCaseSensitive: true).Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldSupportCaseInsensitiveLoginMatching()
    {
        var user = new BasicAuthAuthorizationUser
        {
            Login = "Alice",
            PasswordClear = "secret-password"
        };

        user.Validate("alice", "secret-password", loginCaseSensitive: false).Should().BeTrue();
        user.Validate("alice", "secret-password", loginCaseSensitive: true).Should().BeFalse();
    }

    [Fact]
    public void Validate_ShouldThrow_WhenLoginOrPasswordIsBlank()
    {
        var user = new BasicAuthAuthorizationUser();

        FluentActions.Invoking(() => user.Validate("", "secret", loginCaseSensitive: true))
            .Should().Throw<ArgumentNullException>();

        FluentActions.Invoking(() => user.Validate("alice", "", loginCaseSensitive: true))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Authorize_ShouldRedirectToHttps_WhenSslRedirectIsEnabled()
    {
        var filter = new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
        {
            SslRedirect = true,
            RequireSsl = false
        });

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("example.com");
        httpContext.Request.Path = "/setting";
        var dashboardContext = CreateDashboardContext(httpContext);

        var authorized = filter.Authorize(dashboardContext);

        authorized.Should().BeFalse();
        httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.MovedPermanently);
        httpContext.Response.Headers.Location.ToString().Should().Be("https://example.com:443/setting");
    }

    [Fact]
    public void Authorize_ShouldChallenge_WhenSslIsRequired()
    {
        var filter = new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
        {
            SslRedirect = false,
            RequireSsl = true
        });

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        var dashboardContext = CreateDashboardContext(httpContext);

        var authorized = filter.Authorize(dashboardContext);

        authorized.Should().BeFalse();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        httpContext.Response.Headers.WWWAuthenticate.ToString().Should().Be("Basic realm=\"SettingConfig Dashboard\"");
    }

    [Fact]
    public void Authorize_ShouldChallenge_WhenAuthorizationHeaderIsInvalid()
    {
        var filter = new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
        {
            SslRedirect = false,
            RequireSsl = false
        });

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Basic not-base64";
        var dashboardContext = CreateDashboardContext(httpContext);

        var authorized = filter.Authorize(dashboardContext);

        authorized.Should().BeFalse();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public void Authorize_ShouldAcceptPasswordContainingColon()
    {
        var filter = new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
        {
            SslRedirect = false,
            RequireSsl = false,
            LoginCaseSensitive = false,
            Users =
            [
                new BasicAuthAuthorizationUser
                {
                    Login = "Alice",
                    PasswordClear = "pa:ss:word"
                }
            ]
        });

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Headers.Authorization = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes("alice:pa:ss:word"))}";
        var dashboardContext = CreateDashboardContext(httpContext);

        var authorized = filter.Authorize(dashboardContext);

        authorized.Should().BeTrue();
    }

    private static DashboardContext CreateDashboardContext(HttpContext httpContext)
    {
        var contextType = typeof(DashboardOptions).Assembly
            .GetType("Azrng.SettingConfig.Dto.AspNetCoreDashboardContext", throwOnError: true)!;

        return (DashboardContext)Activator.CreateInstance(
            contextType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: [new DashboardOptions(), httpContext],
            culture: null)!;
    }
}
