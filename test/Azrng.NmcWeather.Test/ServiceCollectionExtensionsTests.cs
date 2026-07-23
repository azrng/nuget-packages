using Common.HttpClients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Azrng.NmcWeather.Test;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNmcWeather_ShouldRegisterIHttpHelperWhenMissing()
    {
        var services = new ServiceCollection();

        services.AddNmcWeather();

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IHttpHelper));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(INmcLocationClient));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(INmcWeatherClient));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(INmcWeatherQueryClient));
    }

    [Fact]
    public void AddNmcWeather_ShouldNotDuplicateIHttpHelperWhenAlreadyRegistered()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHttpHelper>(_ => throw new NotSupportedException());

        services.AddNmcWeather(options => options.BaseUrl = "http://custom-host");

        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IHttpHelper));
    }

    [Fact]
    public void AddNmcWeather_ShouldApplyConfiguredOptions()
    {
        var services = new ServiceCollection();

        services.AddNmcWeather(options =>
        {
            options.BaseUrl = "http://custom-host";
            options.ProvincePath = "custom/provinces";
            options.WeatherPath = "custom/weather";
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<NmcWeatherOptions>>().Value;

        Assert.Equal("http://custom-host", options.BaseUrl);
        Assert.Equal("custom/provinces", options.ProvincePath);
        Assert.Equal("custom/weather", options.WeatherPath);
    }

    [Fact]
    public void AddNmcWeather_ShouldRegisterOptionsValidator()
    {
        var services = new ServiceCollection();

        services.AddNmcWeather();

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IValidateOptions<NmcWeatherOptions>));
    }

    [Fact]
    public void Validator_ShouldRejectEmptyBaseUrl()
    {
        var validator = new NmcWeatherOptionsValidator();

        var result = validator.Validate(null, new NmcWeatherOptions { BaseUrl = "", ProvincePath = "/rest/province", WeatherPath = "/rest/weather" });

        Assert.True(result.Failed);
        Assert.Contains("BaseUrl", result.FailureMessage);
    }

    [Fact]
    public void Validator_ShouldRejectEmptyPaths()
    {
        var validator = new NmcWeatherOptionsValidator();

        var result = validator.Validate(null, new NmcWeatherOptions { BaseUrl = "http://www.nmc.cn", ProvincePath = " ", WeatherPath = " " });

        Assert.True(result.Failed);
        Assert.Contains("ProvincePath", result.FailureMessage);
        Assert.Contains("WeatherPath", result.FailureMessage);
    }

    [Fact]
    public void Validator_ShouldPassForValidOptions()
    {
        var validator = new NmcWeatherOptionsValidator();

        var result = validator.Validate(null, new NmcWeatherOptions());

        Assert.True(result.Succeeded);
    }
}
