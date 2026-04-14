using System.Security.Claims;
using System.Text.Encodings.Web;
using Azrng.AspNetCore.Authorization.Default;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Azrng.AspNetCore.Authorization.Default.Test;

public class PathBasedAuthorizationTests
{
    [Fact]
    public async Task AddPathBasedAuthorization_ShouldRegisterPolicyProviderAndDefaultPolicy()
    {
        using var provider = CreateServiceProvider(_ => true, "/api/login");

        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();
        var namedPolicy = await policyProvider.GetPolicyAsync(ServiceCollectionExtensions.DefaultPolicyName);
        var defaultPolicy = await policyProvider.GetDefaultPolicyAsync();
        var fallbackPolicy = await policyProvider.GetFallbackPolicyAsync();

        namedPolicy.Should().NotBeNull();
        defaultPolicy.Should().NotBeNull();
        fallbackPolicy.Should().BeNull();
        namedPolicy!.Requirements.Should().ContainSingle();
        namedPolicy.Requirements.Single().Should().BeOfType<PermissionRequirement>()
            .Which.AllowAnonymousPaths.Should().ContainSingle().Which.Should().Be("/api/login");
        defaultPolicy.Requirements.Should().ContainSingle();
        defaultPolicy.Requirements.Single().Should().BeOfType<PermissionRequirement>();
    }

    [Fact]
    public async Task AuthorizeAsync_ShouldSucceed_ForAnonymousPath()
    {
        using var provider = CreateServiceProvider(_ => false, "/api/login");
        var httpContext = CreateHttpContext(provider, "/api/login");
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await provider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(user, null, ServiceCollectionExtensions.DefaultPolicyName);

        result.Succeeded.Should().BeTrue();
        httpContext.Items.Should().NotContainKey("permission-path");
    }

    [Fact]
    public async Task AuthorizeAsync_ShouldFail_ForUnauthenticatedUserOnProtectedPath()
    {
        using var provider = CreateServiceProvider(_ => true, "/api/login");
        CreateHttpContext(provider, "/api/secure");
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await provider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(user, null, ServiceCollectionExtensions.DefaultPolicyName);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task AuthorizeAsync_ShouldFail_WhenPermissionServiceDeniesAccess()
    {
        using var provider = CreateServiceProvider(_ => false, "/api/login");
        CreateHttpContext(provider, "/api/secure");
        var user = CreateAuthenticatedUser();

        var result = await provider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(user, null, ServiceCollectionExtensions.DefaultPolicyName);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task AuthorizeAsync_ShouldSucceed_WhenPermissionServiceGrantsAccess()
    {
        using var provider = CreateServiceProvider(path => path == "/api/secure", "/api/login");
        var httpContext = CreateHttpContext(provider, "/api/secure");
        var user = CreateAuthenticatedUser();

        var result = await provider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(user, null, ServiceCollectionExtensions.DefaultPolicyName);

        result.Succeeded.Should().BeTrue();
        httpContext.Items["permission-path"].Should().Be("/api/secure");
    }

    private static ServiceProvider CreateServiceProvider(Func<string, bool> hasPermission, params string[] allowAnonymousPaths)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new PermissionState(hasPermission));
        services.AddAuthentication(TestAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.SchemeName, _ => { });
        services.AddPathBasedAuthorization<TestPermissionVerifyService>(allowAnonymousPaths);

        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext CreateHttpContext(ServiceProvider provider, string path)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = provider
        };
        context.Request.Path = path;
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext = context;
        return context;
    }

    private static ClaimsPrincipal CreateAuthenticatedUser()
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "42"),
                new Claim(ClaimTypes.Name, "alice")
            },
            authenticationType: TestAuthenticationHandler.SchemeName);

        return new ClaimsPrincipal(identity);
    }

    private sealed class PermissionState(Func<string, bool> evaluator)
    {
        public Func<string, bool> Evaluator { get; } = evaluator;
    }

    private sealed class TestPermissionVerifyService(PermissionState state, IHttpContextAccessor accessor) : IPermissionVerifyService
    {
        public Task<bool> HasPermission(string path)
        {
            accessor.HttpContext!.Items["permission-path"] = path;
            return Task.FromResult(state.Evaluator(path));
        }
    }

    private sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "TestScheme";

        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var principal = CreateAuthenticatedUser();
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
