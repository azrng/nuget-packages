using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Azrng.AspNetCore.Authentication.JwtBearer;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace Azrng.AspNetCore.Authentication.JwtBearer.Test;

public class JwtBearerAuthServiceTests
{
    private const string ValidSecret = "ComplexSecretKeyForJwtBearerTests123!";

    [Fact]
    public void CreateToken_WithUserIdAndName_ShouldRoundTripThroughValidationAndClaimReaders()
    {
        var service = CreateService();

        var token = service.CreateToken("42", "alice");

        service.ValidateToken(token).Should().BeTrue();
        service.GetJwtNameIdentifier(token).Should().Be("42");

        var info = service.GetJwtInfo(token);
        info.Should().ContainKey(ClaimTypes.NameIdentifier).WhoseValue.Should().Be("42");
        info.Should().ContainKey(ClaimTypes.Name).WhoseValue.Should().Be("alice");
    }

    [Fact]
    public void CreateToken_WithUserIdOnly_ShouldRoundTrip()
    {
        var service = CreateService();

        var token = service.CreateToken("42");

        service.ValidateToken(token).Should().BeTrue();
        service.GetJwtNameIdentifier(token).Should().Be("42");
    }

    [Fact]
    public void CreateToken_WithCustomClaims_ShouldRoundTrip()
    {
        var service = CreateService();
        var claims = new[]
                     {
                         new Claim(ClaimTypes.NameIdentifier, "42"),
                         new Claim("role", "admin"),
                         new Claim("department", "IT")
                     };

        var token = service.CreateToken(claims);

        service.ValidateToken(token).Should().BeTrue();
        var info = service.GetJwtInfo(token);
        info["role"].Should().Be("admin");
        info["department"].Should().Be("IT");
    }

    [Fact]
    public void CreateToken_WithTamperedSignature_ShouldFailValidation()
    {
        var service = CreateService();
        var token = service.CreateToken("42");
        var tamperedToken = $"{token}tampered";

        service.ValidateToken(tamperedToken).Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_ShouldReturnFalse_WhenTokenWasIssuedByAnotherSecret()
    {
        var issuingService = CreateService();
        var validatingService = CreateService(options => options.JwtSecretKey = "AnotherSecretKeyThatIsLongEnough123!");

        var token = issuingService.CreateToken("42");

        validatingService.ValidateToken(token).Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_ShouldReturnFalse_WhenIssuerMismatches()
    {
        var issuingService = CreateService(options => options.JwtIssuer = "issuer-a");
        var validatingService = CreateService(options => options.JwtIssuer = "issuer-b");

        var token = issuingService.CreateToken("42");

        validatingService.ValidateToken(token).Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_ShouldReturnFalse_WhenAudienceMismatches()
    {
        var issuingService = CreateService(options => options.JwtAudience = "aud-a");
        var validatingService = CreateService(options => options.JwtAudience = "aud-b");

        var token = issuingService.CreateToken("42");

        validatingService.ValidateToken(token).Should().BeFalse();
    }

    [Fact]
    public async Task ValidateToken_ShouldReturnFalse_WhenTokenExpired()
    {
        // 关键安全路径：过期 token 必须被拒绝
        var service = CreateService(options => options.ValidTime = TimeSpan.FromMilliseconds(1));

        var token = service.CreateToken("42");
        await Task.Delay(50); // 确保过期

        service.ValidateToken(token).Should().BeFalse();
    }

    [Fact]
    public void GetJwtNameIdentifier_ShouldReturnNull_WhenClaimDoesNotExist()
    {
        var config = CreateConfig();
        var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: config.JwtIssuer,
            audience: config.JwtAudience,
            claims: new[] { new Claim("custom", "value") }));

        var service = CreateService();

        service.GetJwtNameIdentifier(token).Should().BeNull();
    }

    [Fact]
    public void GetJwtInfo_ShouldHandleNullClaimValueWithoutThrowing()
    {
        // 回归 #4：原代码 x.Value.ToString() 在 value 为 null 时 NRE
        var config = CreateConfig();
        // 构造一个包含 null 值 claim 的 token（通过 JwtPayload 直接写）
        var handler = new JwtSecurityTokenHandler();
        var token = handler.WriteToken(new JwtSecurityToken(
            issuer: config.JwtIssuer,
            audience: config.JwtAudience,
            claims: new[] { new Claim("nullClaim", "value", ClaimValueTypes.String) }));

        var service = CreateService();

        var action = () => service.GetJwtInfo(token);

        action.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ValidateToken_ShouldThrow_WhenTokenIsNullOrEmpty(string? token)
    {
        // #3：null/空属编程错误，应抛出而非吞掉返回 false
        var service = CreateService();

        var action = () => service.ValidateToken(token!);

        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetJwtNameIdentifier_ShouldThrow_WhenTokenIsNullOrEmpty(string? token)
    {
        var service = CreateService();

        var action = () => service.GetJwtNameIdentifier(token!);

        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetJwtInfo_ShouldThrow_WhenTokenIsNullOrEmpty(string? token)
    {
        var service = CreateService();

        var action = () => service.GetJwtInfo(token!);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateToken_WithNullClaims_ShouldThrow()
    {
        var service = CreateService();

        var action = () => service.CreateToken((IEnumerable<Claim>)null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CreateToken_WithEmptyUserId_ShouldThrow(string? id)
    {
        var service = CreateService();

        var action = () => service.CreateToken(id!);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddJwtBearerAuthentication_ShouldRejectShortSecret()
    {
        // 配置校验已收口到 IValidateOptions，解析 IOptions<JwtTokenConfig> 时触发
        var builder = CreateBuilder();
        builder.AddJwtBearerAuthentication(options => options.JwtSecretKey = "short");

        var action = () => builder.Services.BuildServiceProvider()
                                       .GetRequiredService<IOptions<JwtTokenConfig>>()
                                       .Value;

        action.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void AddJwtBearerAuthentication_ShouldRejectLowComplexitySecret()
    {
        var builder = CreateBuilder();
        builder.AddJwtBearerAuthentication(options => options.JwtSecretKey = new string('a', 32));

        var action = () => builder.Services.BuildServiceProvider()
                                       .GetRequiredService<IOptions<JwtTokenConfig>>()
                                       .Value;

        action.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void AddJwtBearerAuthentication_ShouldRejectEmptySecret()
    {
        // #1 回归：默认密钥已被清空，未显式配置时应拒绝
        var builder = CreateBuilder();
        builder.AddJwtBearerAuthentication();

        var action = () => builder.Services.BuildServiceProvider()
                                       .GetRequiredService<IOptions<JwtTokenConfig>>()
                                       .Value;

        action.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void AddJwtBearerAuthentication_ShouldRegisterBearerServiceAndValidationParameters()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearerAuthentication(options =>
                {
                    options.JwtSecretKey = ValidSecret;
                    options.JwtIssuer = "test-issuer";
                    options.JwtAudience = "test-audience";
                });

        using var provider = services.BuildServiceProvider(validateScopes: true);

        provider.GetRequiredService<IBearerAuthService>().Should().BeOfType<JwtBearerAuthService>();

        var tokenOptions = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
                                   .Get(JwtBearerDefaults.AuthenticationScheme);

        tokenOptions.TokenValidationParameters.ValidIssuer.Should().Be("test-issuer");
        tokenOptions.TokenValidationParameters.ValidAudience.Should().Be("test-audience");
        tokenOptions.TokenValidationParameters.ValidateIssuerSigningKey.Should().BeTrue();
        tokenOptions.TokenValidationParameters.ValidateLifetime.Should().BeTrue();
        tokenOptions.TokenValidationParameters.ClockSkew.Should().Be(TimeSpan.Zero);
        tokenOptions.Events.Should().NotBeNull();
    }

    [Fact]
    public void AddJwtBearerAuthentication_ShouldRegisterBearerServiceAsSingleton()
    {
        // #7：服务无状态，应注册为 Singleton
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearerAuthentication(options => options.JwtSecretKey = ValidSecret);

        using var provider = services.BuildServiceProvider(validateScopes: true);

        var first = provider.GetRequiredService<IBearerAuthService>();
        var second = provider.GetRequiredService<IBearerAuthService>();

        first.Should().BeSameAs(second, "IBearerAuthService 应为 Singleton");
    }

    [Fact]
    public async Task AddJwtBearerAuthentication_ShouldInvokeCustomEventsOnTopOfDefaults()
    {
        // README 核心卖点：自定义 events 应在默认 events 基础上叠加，而非覆盖
        // 验证：用户自定义的 OnMessageReceived 生效，且默认 OnAuthenticationFailed 仍标记 Token-Expired
        var services = new ServiceCollection();
        services.AddLogging();

        var customReceiverCalled = false;
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearerAuthentication(
                    options => options.JwtSecretKey = ValidSecret,
                    events => events.OnMessageReceived = _ =>
                    {
                        customReceiverCalled = true;
                        return Task.CompletedTask;
                    });

        using var provider = services.BuildServiceProvider(validateScopes: true);
        var tokenOptions = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
                                   .Get(JwtBearerDefaults.AuthenticationScheme);

        // 自定义 OnMessageReceived 已挂载
        tokenOptions.Events!.OnMessageReceived.Should().NotBeNull();
        await tokenOptions.Events.OnMessageReceived!(new MessageReceivedContext(
            new DefaultHttpContext(),
            new AuthenticationScheme(JwtBearerDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme, typeof(JwtBearerHandler)),
            new JwtBearerOptions()));

        customReceiverCalled.Should().BeTrue("自定义 OnMessageReceived 应被调用");

        // 默认 OnAuthenticationFailed 仍存在，过期异常会添加 Token-Expired 头
        var failedContext = new AuthenticationFailedContext(
            new DefaultHttpContext(),
            new AuthenticationScheme(JwtBearerDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme, typeof(JwtBearerHandler)),
            new JwtBearerOptions())
        {
            Exception = new SecurityTokenExpiredException { Expires = DateTime.UtcNow }
        };
        await tokenOptions.Events.OnAuthenticationFailed!(failedContext);
        failedContext.Response.Headers.TryGetValue("Token-Expired", out var expired).Should().BeTrue();
        expired.ToString().Should().Be("true");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AddJwtBearerAuthentication_UseDefaultChallengeResponse_ShouldControlCustom401Body(bool useDefault)
    {
        // #8：开关控制是否使用内置自定义 401 响应体
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearerAuthentication(
                    options => options.JwtSecretKey = ValidSecret,
                    useDefaultChallengeResponse: useDefault);

        using var provider = services.BuildServiceProvider(validateScopes: true);
        var tokenOptions = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
                                   .Get(JwtBearerDefaults.AuthenticationScheme);

        var context = new JwtBearerChallengeContext(
            new DefaultHttpContext(),
            new AuthenticationScheme(JwtBearerDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme, typeof(JwtBearerHandler)),
            new JwtBearerOptions(),
            new AuthenticationProperties());

        await tokenOptions.Events!.OnChallenge!(context);

        if (useDefault)
        {
            context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            context.Response.ContentType.Should().Contain("application/json");
        }
        else
        {
            // 关闭开关时不接管响应，状态码保持默认 200（HandleResponse 未被调用）
            context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }

    private static AuthenticationBuilder CreateBuilder()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
    }

    private static IBearerAuthService CreateService(Action<JwtTokenConfig>? configure = null)
    {
        var config = CreateConfig();
        configure?.Invoke(config);
        // 构造一个返回固定配置的 IOptionsMonitor mock
        var monitor = new Mock<IOptionsMonitor<JwtTokenConfig>>();
        monitor.SetupGet(m => m.CurrentValue).Returns(config);
        monitor.Setup(m => m.Get(It.IsAny<string>())).Returns(config);
        return new JwtBearerAuthService(monitor.Object);
    }

    private static JwtTokenConfig CreateConfig()
    {
        return new JwtTokenConfig
               {
                   JwtSecretKey = ValidSecret,
                   JwtIssuer = "test-issuer",
                   JwtAudience = "test-audience",
                   ValidTime = TimeSpan.FromMinutes(30)
               };
    }
}
