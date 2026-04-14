using System.Data;
using Azrng.SettingConfig.Dto;
using Azrng.SettingConfig.Interface;
using Azrng.SettingConfig.Service;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Xunit;

namespace Azrng.SettingConfig.Test;

public class ServiceRegistrationTests
{
    [Fact]
    public void AddSettingConfig_ShouldRegisterExpectedServicesAndApplyOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddSettingConfig<TestConnectInterface>(options =>
        {
            options.DbConnectionString = "Host=localhost;Username=test;Password=test;Database=setting_test";
            options.RoutePrefix = "systemSetting";
            options.ApiRoutePrefix = "/api/custom-setting/";
            options.PageTitle = "Custom Title";
            options.PageDescription = "Custom Description";
        });

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var options = provider.GetRequiredService<IOptions<DashboardOptions>>().Value;
        var dbConnection = scope.ServiceProvider.GetRequiredService<IDbConnection>();
        var connectInterface = scope.ServiceProvider.GetRequiredService<IConnectInterface>();
        var dataSourceProvider = scope.ServiceProvider.GetRequiredService<IDataSourceProvider>();
        var configSettingService = scope.ServiceProvider.GetRequiredService<IConfigSettingService>();
        var externalProvideService = scope.ServiceProvider.GetRequiredService<IConfigExternalProvideService>();
        var distributedCache = provider.GetRequiredService<IDistributedCache>();
        var routeAttribute = new Attributes.SettingMatchRouteAttribute();

        options.ApiRoutePrefix.Should().Be("/api/custom-setting/");
        options.PageTitle.Should().Be("Custom Title");
        dbConnection.Should().BeOfType<NpgsqlConnection>();
        connectInterface.Should().BeOfType<TestConnectInterface>();
        dataSourceProvider.GetType().Name.Should().Be("PgsqlDataSourceProvider");
        configSettingService.GetType().Name.Should().Be("ConfigSettingService");
        externalProvideService.GetType().Name.Should().Be("ConfigExternalProvideService");
        distributedCache.Should().NotBeNull();
        routeAttribute.Template.Should().Be("/api/custom-setting");
    }

    [Fact]
    public async Task DefaultConnectInterface_ShouldReturnSuccessfulCallbacks()
    {
        var connectInterface = new DefaultConnectInterface();
        var request = new UpdateConfigDetailsRequest
        {
            ConfigId = 1,
            Description = "desc",
            Value = "value"
        };

        var validate = await connectInterface.ItemValidate(request);
        var edit = await connectInterface.EditSuccessHandle("key", "value");
        var delete = await connectInterface.DeleteSuccessHandle("key", "value");

        validate.Should().Be((true, string.Empty));
        edit.Should().Be((true, string.Empty));
        delete.Should().Be((true, string.Empty));
    }
}
