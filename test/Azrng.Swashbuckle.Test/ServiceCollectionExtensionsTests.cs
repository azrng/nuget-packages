using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Xunit;

namespace Azrng.Swashbuckle.Test;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDefaultSwaggerGen_WithCustomTitle_ShouldGenerateSwaggerDocument()
    {
        var services = CreateServices();

        services.AddDefaultSwaggerGen(title: "Custom API");

        using var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<ISwaggerProvider>();
        var document = provider.GetSwagger("v1");

        document.Info.Title.Should().Be("Custom API");
        document.Info.Version.Should().Be("v1");
    }

    [Fact]
    public void AddDefaultSwaggerGen_WithExplicitOpenApiInfo_ShouldUseProvidedDocumentInfo()
    {
        var services = CreateServices();

        services.AddDefaultSwaggerGen(new OpenApiInfo
        {
            Title = "Orders API",
            Version = "2026-04",
            Description = "Order endpoints"
        });

        using var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<ISwaggerProvider>();
        var document = provider.GetSwagger("v1");

        document.Info.Title.Should().Be("Orders API");
        document.Info.Version.Should().Be("2026-04");
        document.Info.Description.Should().Be("Order endpoints");
    }

    [Fact]
    public void AddDefaultSwaggerGen_WithJwtEnabled_ShouldAddBearerSecurityDefinition()
    {
        var services = CreateServices();

        services.AddDefaultSwaggerGen(showJwtToken: true);

        using var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<ISwaggerProvider>();
        var document = provider.GetSwagger("v1");

        document.Components.SecuritySchemes.Should().ContainKey("Bearer");
        var scheme = document.Components.SecuritySchemes["Bearer"];
        scheme.Name.Should().Be("Authorization");
        scheme.Type.Should().Be(SecuritySchemeType.ApiKey);
        scheme.In.Should().Be(ParameterLocation.Header);
        document.SecurityRequirements.Should().NotBeEmpty();
    }

    [Fact]
    public void AddDefaultSwaggerGen_ShouldRunAdditionalConfigurationAction()
    {
        var services = CreateServices();

        services.AddDefaultSwaggerGen(action: options =>
        {
            options.SwaggerDoc("internal", new OpenApiInfo
            {
                Title = "Internal API",
                Version = "v2"
            });
        });

        using var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<ISwaggerProvider>();
        var document = provider.GetSwagger("internal");

        document.Info.Title.Should().Be("Internal API");
        document.Info.Version.Should().Be("v2");
    }

    private static ServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddEndpointsApiExplorer();
        services.AddLogging();
        var environment = new TestWebHostEnvironment();
        services.AddSingleton<IWebHostEnvironment>(environment);
        services.AddSingleton<IHostEnvironment>(environment);
        return services;
    }
}
