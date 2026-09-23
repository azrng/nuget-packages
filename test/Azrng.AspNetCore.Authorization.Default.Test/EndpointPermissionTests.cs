using System.Security.Claims;
using Azrng.AspNetCore.Authorization.Default;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Azrng.AspNetCore.Authorization.Default.Test;

public class EndpointPermissionTests
{
    [Fact]
    public async Task EndpointMetadata_ShouldBePassedToPermissionEvaluator()
    {
        var state = new EvaluationState(_ => true);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(state);
        services.AddPermissionAuthorization<TestPermissionEvaluator>();

        using var provider = services.BuildServiceProvider();
        var httpContext = CreateHttpContext(provider, "/API/orders/42", "POST");
        httpContext.SetEndpoint(CreateEndpoint(new PermissionMetadata(
            PermissionMatchMode.Any,
            "orders.read",
            "orders.write")));

        var result = await provider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(CreateAuthenticatedUser(), null, ServiceCollectionExtensions.DefaultPolicyName);

        result.Succeeded.Should().BeTrue();
        state.Context.Should().NotBeNull();
        state.Context!.Path.Should().Be("/api/orders/42");
        state.Context.Method.Should().Be("POST");
        state.Context.RequiredPermissions.Should().ContainSingle();
        state.Context.RequiredPermissions[0].MatchMode.Should().Be(PermissionMatchMode.Any);
        state.Context.RequiredPermissions[0].Permissions.Should()
            .Equal("orders.read", "orders.write");
    }

    [Fact]
    public async Task EvaluatorDenial_ShouldFail_WhenEndpointPermissionIsMissing()
    {
        var state = new EvaluationState(_ => false);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(state);
        services.AddPermissionAuthorization<TestPermissionEvaluator>();

        using var provider = services.BuildServiceProvider();
        var httpContext = CreateHttpContext(provider, "/api/orders", "GET");
        httpContext.SetEndpoint(CreateEndpoint(new PermissionMetadata("orders.read")));

        var result = await provider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(CreateAuthenticatedUser(), null, ServiceCollectionExtensions.DefaultPolicyName);

        result.Succeeded.Should().BeFalse();
        state.Context.Should().NotBeNull();
    }

    [Fact]
    public void RequirePermissionAttribute_ShouldUseDefaultPermissionPolicy()
    {
        var attribute = new RequirePermissionAttribute("orders.read", "orders.write")
        {
            MatchMode = PermissionMatchMode.Any
        };

        attribute.Policy.Should().Be(ServiceCollectionExtensions.DefaultPolicyName);
        attribute.MatchMode.Should().Be(PermissionMatchMode.Any);
        attribute.Permissions.Should().Equal("orders.read", "orders.write");
    }

    private static DefaultHttpContext CreateHttpContext(
        ServiceProvider provider,
        string path,
        string method)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = provider
        };
        context.Request.Path = path;
        context.Request.Method = method;
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext = context;
        return context;
    }

    private static Endpoint CreateEndpoint(params IPermissionMetadata[] metadata)
    {
        return new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(metadata),
            "EndpointPermissionTests");
    }

    private static ClaimsPrincipal CreateAuthenticatedUser()
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, "alice") },
            authenticationType: "Test"));
    }

    private sealed class EvaluationState
    {
        public EvaluationState(Func<PermissionContext, bool> evaluator)
        {
            Evaluator = evaluator;
        }

        public Func<PermissionContext, bool> Evaluator { get; }

        public PermissionContext? Context { get; set; }
    }

    private sealed class TestPermissionEvaluator : IPermissionEvaluator
    {
        private readonly EvaluationState _state;

        public TestPermissionEvaluator(EvaluationState state)
        {
            _state = state;
        }

        public Task<AuthorizationDecision> AuthorizeAsync(
            PermissionContext context,
            CancellationToken cancellationToken = default)
        {
            _state.Context = context;
            return Task.FromResult(_state.Evaluator(context)
                ? AuthorizationDecision.Allow()
                : AuthorizationDecision.Deny());
        }
    }
}
