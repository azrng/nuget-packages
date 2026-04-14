using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using Xunit;

namespace Azrng.Swashbuckle.Test;

public class ApplicationBuilderExtensionsTests
{
    [Fact]
    public void SetUrlEditable_ShouldAppendEditableSpanScript()
    {
        var options = new SwaggerUIOptions
        {
            HeadContent = "<meta charset=\"utf-8\">"
        };

        options.SetUrlEditable();

        options.HeadContent.Should().Contain("<meta charset=\"utf-8\">");
        options.HeadContent.Should().Contain("contentEditable: true");
        options.HeadContent.Should().Contain("window.ui.React.createElement");
    }

    [Fact]
    public void UseDefaultSwagger_WithMissingEnvironmentVariableAndDevelopmentOnly_ShouldNotThrow()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddEndpointsApiExplorer();
        services.AddLogging();
        var environment = new TestWebHostEnvironment();
        services.AddSingleton<IWebHostEnvironment>(environment);
        services.AddSingleton<IHostEnvironment>(environment);
        services.AddDefaultSwaggerGen();
        using var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        var original = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);

            var act = () => app.UseDefaultSwagger(onlyDevelopmentEnabled: true);

            act.Should().NotThrow();
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", original);
        }
    }

    [Fact]
    public void UseDefaultSwagger_WithProductionEnvironmentAndDevelopmentOnly_ShouldReturnSameBuilder()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddEndpointsApiExplorer();
        services.AddLogging();
        var environment = new TestWebHostEnvironment();
        services.AddSingleton<IWebHostEnvironment>(environment);
        services.AddSingleton<IHostEnvironment>(environment);
        services.AddDefaultSwaggerGen();
        using var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        var original = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

            var result = app.UseDefaultSwagger(onlyDevelopmentEnabled: true);

            result.Should().BeSameAs(app);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", original);
        }
    }

    [Fact]
    public void UseDefaultSwagger_WithCustomActions_ShouldInvokeProvidedCallbacks()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddEndpointsApiExplorer();
        services.AddLogging();
        var environment = new TestWebHostEnvironment();
        services.AddSingleton<IWebHostEnvironment>(environment);
        services.AddSingleton<IHostEnvironment>(environment);
        services.AddDefaultSwaggerGen();
        using var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);
        SwaggerOptions? capturedSwaggerOptions = null;
        SwaggerUIOptions? capturedUiOptions = null;

        var result = app.UseDefaultSwagger(
            setupAction: options =>
            {
                options.RouteTemplate = "docs/{documentName}.json";
                capturedSwaggerOptions = options;
            },
            action: options =>
            {
                options.RoutePrefix = "docs";
                capturedUiOptions = options;
            });

        result.Should().BeSameAs(app);
        capturedSwaggerOptions.Should().NotBeNull();
        capturedSwaggerOptions!.RouteTemplate.Should().Be("docs/{documentName}.json");
        capturedUiOptions.Should().NotBeNull();
        capturedUiOptions!.RoutePrefix.Should().Be("docs");
    }
}

internal sealed class TestWebHostEnvironment : IWebHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";

    public string ApplicationName { get; set; } = "Azrng.Swashbuckle.Test";

    public string WebRootPath { get; set; } = AppContext.BaseDirectory;

    public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } =
        new Microsoft.Extensions.FileProviders.NullFileProvider();

    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

    public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
        new Microsoft.Extensions.FileProviders.NullFileProvider();
}
