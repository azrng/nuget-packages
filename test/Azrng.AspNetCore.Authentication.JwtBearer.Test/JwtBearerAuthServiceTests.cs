using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Azrng.AspNetCore.Authentication.JwtBearer;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Azrng.AspNetCore.Authentication.JwtBearer.Test;

public class JwtBearerAuthServiceTests
{
    [Fact]
    public void CreateToken_ShouldRoundTripThroughValidationAndClaimReaders()
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
    public void ValidateToken_ShouldReturnFalse_WhenTokenIsTampered()
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
    public void AddJwtBearerAuthentication_ShouldRejectShortSecret()
    {
        var services = new ServiceCollection();
        var builder = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

        var action = () => builder.AddJwtBearerAuthentication(options => options.JwtSecretKey = "short");

        action.Should().Throw<ArgumentException>()
            .WithMessage("*至少为 32 位字符*");
    }

    [Fact]
    public void AddJwtBearerAuthentication_ShouldRejectLowComplexitySecret()
    {
        var services = new ServiceCollection();
        var builder = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

        var action = () => builder.AddJwtBearerAuthentication(options => options.JwtSecretKey = new string('a', 32));

        action.Should().Throw<ArgumentException>()
            .WithMessage("*复杂度不足*");
    }

    [Fact]
    public void AddJwtBearerAuthentication_ShouldRegisterBearerServiceAndValidationParameters()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearerAuthentication(options =>
            {
                options.JwtSecretKey = "ComplexSecretKeyForJwtBearerTests123!";
                options.JwtIssuer = "test-issuer";
                options.JwtAudience = "test-audience";
            });

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IBearerAuthService>().Should().BeOfType<JwtBearerAuthService>();

        var tokenOptions = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        tokenOptions.TokenValidationParameters.ValidIssuer.Should().Be("test-issuer");
        tokenOptions.TokenValidationParameters.ValidAudience.Should().Be("test-audience");
        tokenOptions.TokenValidationParameters.ValidateIssuerSigningKey.Should().BeTrue();
        tokenOptions.Events.Should().NotBeNull();
    }

    private static IBearerAuthService CreateService(Action<JwtTokenConfig>? configure = null)
    {
        var config = CreateConfig();
        configure?.Invoke(config);
        return new JwtBearerAuthService(Options.Create(config));
    }

    private static JwtTokenConfig CreateConfig()
    {
        return new JwtTokenConfig
        {
            JwtSecretKey = "ComplexSecretKeyForJwtBearerTests123!",
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            ValidTime = TimeSpan.FromMinutes(30)
        };
    }
}
