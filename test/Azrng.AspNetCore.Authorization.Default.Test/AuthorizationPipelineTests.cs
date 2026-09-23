using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Azrng.AspNetCore.Authorization.Default;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Azrng.AspNetCore.Authorization.Default.Test;

public class AuthorizationPipelineTests
{
    [Fact]
    public async Task ProtectedEndpoint_ShouldReturn401_WhenUserIsNotAuthenticated()
    {
        using var server = CreateServer(_ => true);

        using var response = await server.CreateClient().GetAsync("/secure");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Evaluator_ShouldNotBeInvoked_WhenUserIsNotAuthenticated()
    {
        // 匿名请求即使授权结果注定 401，也不应触发业务评估器（数据库 / 远程 ACL 等昂贵调用）
        using var server = CreateServer(_ => true);

        using var response = await server.CreateClient().GetAsync("/secure");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var state = server.Host.Services.GetRequiredService<PermissionState>();
        Assert.Equal(0, state.CallCount);
    }

    [Fact]
    public async Task Evaluator_ShouldBeInvoked_WhenUserIsAuthenticated()
    {
        using var server = CreateServer(context => context.Path == "/secure");
        using var client = server.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", "alice");

        using var response = await client.GetAsync("/secure");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var state = server.Host.Services.GetRequiredService<PermissionState>();
        Assert.Equal(1, state.CallCount);
    }

    [Fact]
    public async Task ProtectedEndpoint_ShouldReturn403_WhenPermissionIsDenied()
    {
        using var server = CreateServer(_ => false);
        using var client = server.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", "alice");

        using var response = await client.GetAsync("/secure");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_ShouldReturn200_WhenPermissionIsGranted()
    {
        using var server = CreateServer(context => context.Path == "/secure");
        using var client = server.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", "alice");

        using var response = await client.GetAsync("/secure");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static TestServer CreateServer(Func<PermissionContext, bool> permission)
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddLogging();
                services.AddSingleton(new PermissionState(permission));
                services.AddAuthentication(TestAuthenticationHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                        TestAuthenticationHandler.SchemeName,
                        _ => { });
                services.AddPermissionAuthorization<TestPermissionEvaluator>();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/secure", async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        await context.Response.WriteAsync("ok");
                    })
                        .RequireAuthorization();
                });
            });

        return new TestServer(builder);
    }

    private sealed class PermissionState
    {
        public PermissionState(Func<PermissionContext, bool> evaluator)
        {
            Evaluator = evaluator;
        }

        public Func<PermissionContext, bool> Evaluator { get; }

        public int CallCount { get; private set; }

        public void RecordCall()
        {
            CallCount++;
        }
    }

    private sealed class TestPermissionEvaluator : IPermissionEvaluator
    {
        private readonly PermissionState _state;

        public TestPermissionEvaluator(PermissionState state)
        {
            _state = state;
        }

        public Task<AuthorizationDecision> AuthorizeAsync(
            PermissionContext context,
            CancellationToken cancellationToken = default)
        {
            _state.RecordCall();
            return Task.FromResult(_state.Evaluator(context)
                ? AuthorizationDecision.Allow()
                : AuthorizationDecision.Deny());
        }
    }

    private sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Test";

        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("X-Test-User"))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "alice") },
                Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
