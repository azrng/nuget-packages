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
using Moq;
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

    // ===== 匿名路径匹配安全回归（StartsWithSegments 路径段前缀匹配） =====

    [Theory]
    // 配置 /api/login，完全匹配与子路径放行
    [InlineData("/api/login", "/api/login", true)]
    [InlineData("/api/login", "/api/login/callback", true)]
    // 子串命中但非路径段前缀 —— 必须拒绝（修复前的安全缺陷）
    [InlineData("/api/login", "/admin/api/login/delete", false)]
    [InlineData("/api/login", "/api/login-export", false)]
    [InlineData("/api/login", "/api/loginator", false)]
    // 父路径配置不应当隐式放行兄弟路径
    [InlineData("/api/login", "/api/logout", false)]
    // 大小写不敏感（README 宣称的行为）
    [InlineData("/api/login", "/API/LOGIN", true)]
    [InlineData("/api/login", "/Api/Login/Callback", true)]
    public async Task AuthorizeAsync_AnonymousPathMatching_ShouldUseSegmentPrefix(
        string configured, string requested, bool expectedSucceed)
    {
        // 权限服务恒拒绝，验证唯一放行路径是匿名匹配
        using var provider = CreateServiceProvider(_ => false, configured);
        CreateHttpContext(provider, requested);
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await provider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(user, null, ServiceCollectionExtensions.DefaultPolicyName);

        result.Succeeded.Should().Be(expectedSucceed,
            $"配置 '{configured}'，请求 '{requested}'，期望 {(expectedSucceed ? "放行" : "拒绝")}");
    }

    [Fact]
    public async Task AuthorizeAsync_ShouldSucceed_WhenMultipleAllowAnonymousPathsAndOneMatchesSegment()
    {
        using var provider = CreateServiceProvider(_ => false, "/api/login", "/health", "/swagger");
        CreateHttpContext(provider, "/swagger/index.html");
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await provider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(user, null, ServiceCollectionExtensions.DefaultPolicyName);

        result.Succeeded.Should().BeTrue();
    }

    // ===== 认证分支回归 =====

    [Fact]
    public async Task AuthorizeAsync_ShouldFail_WhenNoDefaultAuthenticateScheme()
    {
        // 模拟认证方案提供器存在、但没有默认认证方案（GetDefaultAuthenticateSchemeAsync 返回 null）
        var schemeProviderMock = new Mock<IAuthenticationSchemeProvider>();
        schemeProviderMock
            .Setup(x => x.GetDefaultAuthenticateSchemeAsync())
            .ReturnsAsync((AuthenticationScheme?)null);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new PermissionState(_ => true));
        services.AddSingleton(schemeProviderMock.Object);
        services.AddPathBasedAuthorization<TestPermissionVerifyService>("/api/login");
        using var provider = services.BuildServiceProvider();
        CreateHttpContext(provider, "/api/secure");
        var user = CreateAuthenticatedUser();

        var result = await provider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(user, null, ServiceCollectionExtensions.DefaultPolicyName);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task AuthorizeAsync_ShouldSucceed_ForProtectedPath_WhenAuthenticatedAndPermitted()
    {
        using var provider = CreateServiceProvider(path => path == "/api/users/123", "/api/login");
        var httpContext = CreateHttpContext(provider, "/api/users/123");
        var user = CreateAuthenticatedUser();

        var result = await provider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(user, null, ServiceCollectionExtensions.DefaultPolicyName);

        result.Succeeded.Should().BeTrue();
        // 传给权限服务的路径应为小写形式（接口契约约定）
        httpContext.Items["permission-path"].Should().Be("/api/users/123");
    }

    // ===== Requirement 不可变性回归 =====

    [Fact]
    public void PermissionRequirement_AllowAnonymousPaths_ShouldBeImmutable()
    {
        var requirement = new PermissionRequirement("/api/login", "/api/health");

        requirement.AllowAnonymousPaths.Should().BeAssignableTo<IReadOnlyCollection<string>>();
        var snapshot = requirement.AllowAnonymousPaths.ToArray();
        snapshot.Should().Equal("/api/login", "/api/health");
    }

    [Fact]
    public void PermissionRequirement_Constructor_ShouldThrow_OnNullPaths()
    {
        Action act = () => new PermissionRequirement(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PermissionRequirement_ShouldAllowEmptyPathsArray()
    {
        var requirement = new PermissionRequirement();
        requirement.AllowAnonymousPaths.Should().BeEmpty();
    }

    // ===== 向后兼容回归 =====

    [Fact]
    public async Task AddMyAuthorization_ShouldStillWork_AsObsoleteAlias()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new PermissionState(_ => true));
        services.AddAuthentication(TestAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.SchemeName, _ => { });
#pragma warning disable CS0618 // 测试已废弃的别名方法
        services.AddMyAuthorization<TestPermissionVerifyService>("/api/login");
#pragma warning restore CS0618
        using var provider = services.BuildServiceProvider();

        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();
        var namedPolicy = await policyProvider.GetPolicyAsync(ServiceCollectionExtensions.DefaultPolicyName);
        namedPolicy.Should().NotBeNull();
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
