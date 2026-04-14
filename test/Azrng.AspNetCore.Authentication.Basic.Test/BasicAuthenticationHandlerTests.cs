using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Azrng.AspNetCore.Authentication.Basic;
using Azrng.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Azrng.AspNetCore.Authentication.Basic.Test;

public class BasicAuthenticationHandlerTests
{
    [Fact]
    public async Task AuthenticateAsync_ShouldFail_WhenAuthorizationHeaderIsMissing()
    {
        using var provider = CreateServiceProvider();
        var context = CreateHttpContext(provider);

        var result = await provider.GetRequiredService<IAuthenticationService>()
            .AuthenticateAsync(context, BasicAuthentication.AuthenticationSchema);

        result.Succeeded.Should().BeFalse();
        result.Failure?.Message.Should().Be("未标注 Authorization 请求头");
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFail_WhenHeaderIsNotBasic()
    {
        using var provider = CreateServiceProvider();
        var context = CreateHttpContext(provider, "Bearer token");

        var result = await provider.GetRequiredService<IAuthenticationService>()
            .AuthenticateAsync(context, BasicAuthentication.AuthenticationSchema);

        result.Succeeded.Should().BeFalse();
        result.Failure?.Message.Should().Be("Authorization 请求头格式不正确");
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFail_WhenCredentialsAreNotValidBase64()
    {
        using var provider = CreateServiceProvider();
        var context = CreateHttpContext(provider, "Basic not-base64");

        var result = await provider.GetRequiredService<IAuthenticationService>()
            .AuthenticateAsync(context, BasicAuthentication.AuthenticationSchema);

        result.Succeeded.Should().BeFalse();
        result.Failure?.Message.Should().Be("认证头格式无效");
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFail_WhenCredentialsDoNotMatch()
    {
        using var provider = CreateServiceProvider();
        var context = CreateHttpContext(provider, CreateBasicHeader("alice", "wrong-password"));

        var result = await provider.GetRequiredService<IAuthenticationService>()
            .AuthenticateAsync(context, BasicAuthentication.AuthenticationSchema);

        result.Succeeded.Should().BeFalse();
        result.Failure?.Message.Should().Be("无效用户名或密码");
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldCreatePrincipalWithCustomClaims_WhenCredentialsMatch()
    {
        using var provider = CreateServiceProvider(useCustomVerifier: true);
        var context = CreateHttpContext(provider, CreateBasicHeader("alice", "secret-password"));

        var result = await provider.GetRequiredService<IAuthenticationService>()
            .AuthenticateAsync(context, BasicAuthentication.AuthenticationSchema);

        result.Succeeded.Should().BeTrue();
        result.Principal.Should().NotBeNull();
        result.Principal!.FindFirst(ClaimTypes.Name)?.Value.Should().Be("alice");
        result.Principal.FindFirst(ClaimTypes.Role)?.Value.Should().Be("admin");
    }

    [Fact]
    public async Task ChallengeAsync_ShouldWriteUnauthorizedJsonResponse()
    {
        using var provider = CreateServiceProvider();
        var context = CreateHttpContext(provider);

        await provider.GetRequiredService<IAuthenticationService>()
            .ChallengeAsync(context, BasicAuthentication.AuthenticationSchema, null);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.ContentType.Should().Be("application/json");
        var payload = ReadPayload(context);
        payload.Message.Should().Be("您无权访问该接口，请确保已经登录");
        payload.Code.Should().Be("401");
    }

    [Fact]
    public async Task ForbidAsync_ShouldWriteForbiddenJsonResponse()
    {
        using var provider = CreateServiceProvider();
        var context = CreateHttpContext(provider);

        await provider.GetRequiredService<IAuthenticationService>()
            .ForbidAsync(context, BasicAuthentication.AuthenticationSchema, null);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        context.Response.ContentType.Should().Be("application/json");
        var payload = ReadPayload(context);
        payload.Message.Should().Be("您的访问权限不够，请联系管理员");
        payload.Code.Should().Be("403");
    }

    [Fact]
    public void AddBasicAuthentication_ShouldRegisterCustomVerifier()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IJsonSerializer, TestJsonSerializer>();

        services.AddAuthentication(BasicAuthentication.AuthenticationSchema)
            .AddBasicAuthentication<TestBasicAuthorizeVerify>(options =>
            {
                options.UserName = "alice";
                options.Password = "secret-password";
            });

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IBasicAuthorizeVerify>()
            .Should().BeOfType<TestBasicAuthorizeVerify>();
    }

    private static ServiceProvider CreateServiceProvider(bool useCustomVerifier = false)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IJsonSerializer, TestJsonSerializer>();

        var builder = services.AddAuthentication(BasicAuthentication.AuthenticationSchema);
        if (useCustomVerifier)
        {
            builder.AddBasicAuthentication<TestBasicAuthorizeVerify>(options =>
            {
                options.UserName = "alice";
                options.Password = "secret-password";
            });
        }
        else
        {
            builder.AddBasicAuthentication(options =>
            {
                options.UserName = "alice";
                options.Password = "secret-password";
            });
        }

        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext CreateHttpContext(IServiceProvider provider, string? authorizationHeader = null)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = provider
        };
        context.Response.Body = new MemoryStream();

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            context.Request.Headers.Authorization = authorizationHeader;
        }

        return context;
    }

    private static string CreateBasicHeader(string userName, string password)
    {
        var bytes = Encoding.UTF8.GetBytes($"{userName}:{password}");
        return $"Basic {Convert.ToBase64String(bytes)}";
    }

    private static string ReadBody(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        return reader.ReadToEnd();
    }

    private static ErrorPayload ReadPayload(HttpContext context)
    {
        return JsonSerializer.Deserialize<ErrorPayload>(ReadBody(context))!;
    }

    private sealed class TestBasicAuthorizeVerify : IBasicAuthorizeVerify
    {
        public Task<Claim[]> GetCurrentUserClaims(string userName)
        {
            return Task.FromResult(new[]
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, "admin")
            });
        }
    }

    private sealed class TestJsonSerializer : IJsonSerializer
    {
        public T? Clone<T>(T obj) where T : class
        {
            return obj is null ? null : ToObject<T>(ToJson(obj));
        }

        public List<T>? ToList<T>(string json)
        {
            return JsonSerializer.Deserialize<List<T>>(json);
        }

        public string ToJson<T>(T obj) where T : class
        {
            return JsonSerializer.Serialize(obj);
        }

        public T? ToObject<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
    }

    private sealed class ErrorPayload
    {
        public string Message { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;
    }
}
