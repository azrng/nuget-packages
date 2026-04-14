using Azrng.SettingConfig;
using Azrng.SettingConfig.Dto;
using Azrng.SettingConfig.Interface;
using Azrng.SettingConfig.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics.CodeAnalysis;

namespace Azrng.SettingConfig.Test;

internal static class TestInfrastructure
{
    public static DashboardOptions CreateOptions(Action<DashboardOptions>? configure = null)
    {
        var options = new DashboardOptions
        {
            DbConnectionString = "Host=localhost;Username=test;Password=test;Database=setting_test",
            PageTitle = "Test Title",
            PageDescription = "Test Description",
            ApiRoutePrefix = "/api/test-setting"
        };

        configure?.Invoke(options);
        return options;
    }

    public static DashboardContext CreateDashboardContext(HttpContext httpContext, DashboardOptions? options = null)
    {
        var contextType = typeof(DashboardOptions).Assembly.GetType("Azrng.SettingConfig.Dto.AspNetCoreDashboardContext", throwOnError: true)!;
        return (DashboardContext)Activator.CreateInstance(contextType, options ?? CreateOptions(), httpContext)!;
    }

    public static IDashboardAuthorizationFilter CreateLocalRequestsOnlyAuthorizationFilter()
    {
        var filterType = typeof(DashboardOptions).Assembly.GetType("Azrng.SettingConfig.Service.LocalRequestsOnlyAuthorizationFilter", throwOnError: true)!;
        return (IDashboardAuthorizationFilter)Activator.CreateInstance(filterType, nonPublic: true)!;
    }
}

internal sealed class TestWebHostEnvironment : IWebHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";

    public string ApplicationName { get; set; } = "Azrng.SettingConfig.Test";

    public string WebRootPath { get; set; } = AppContext.BaseDirectory;

    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}

internal sealed class AllowAllAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize([NotNull] DashboardContext context) => true;
}

internal sealed class DenyAllAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize([NotNull] DashboardContext context) => false;
}

internal sealed class TestConnectInterface : IConnectInterface
{
    public Task<(bool IsOk, string ErrMsg)> ItemValidate(UpdateConfigDetailsRequest request)
        => Task.FromResult((true, string.Empty));

    public Task<(bool IsOk, string ErrMsg)> EditSuccessHandle(string key, string value)
        => Task.FromResult((true, string.Empty));

    public Task<(bool IsOk, string ErrMsg)> DeleteSuccessHandle(string key, string value)
        => Task.FromResult((true, string.Empty));
}

internal sealed class FakeDataSourceProvider : IDataSourceProvider
{
    public int InitCallCount { get; private set; }

    public Task<bool> InitAsync()
    {
        InitCallCount++;
        return Task.FromResult(true);
    }

    public Task<List<GetSettingInfoDto>?> GetPageListAsync(int pageIndex, int pageSize, string? keyword, string? version)
        => Task.FromResult<List<GetSettingInfoDto>?>([]);

    public Task<int> GetConfigCount() => Task.FromResult(0);

    public Task<GetConfigDetailsResult?> GetConfigDetails(int configId) => Task.FromResult<GetConfigDetailsResult?>(null);

    public Task<GetConfigInfoDto?> GetConfigInfoAsync(int configId) => Task.FromResult<GetConfigInfoDto?>(null);

    public Task<string> GetConfigKeyAsync(int configVersionId) => Task.FromResult(string.Empty);

    public Task<bool> UpdateConfigVersionAsync(int versionId, string value, string description, string updateUserId)
        => Task.FromResult(true);

    public Task<List<GetConfigVersionListResult>?> GetConfigHistoryListAsync(string key)
        => Task.FromResult<List<GetConfigVersionListResult>?>([]);

    public Task<GetConfigVersionListResult?> GetHistoryInfoAsync(int historyId)
        => Task.FromResult<GetConfigVersionListResult?>(null);

    public Task<bool> DeleteConfigAsync(int configId) => Task.FromResult(true);

    public Task<bool> RestoreConfigAsync(int historyId) => Task.FromResult(true);

    public Task<string> GetConfigValueAsync(string key) => Task.FromResult(string.Empty);

    public Task<bool> UpdateConfigValueAsync(string key, string value, string? updateUserId = null)
        => Task.FromResult(true);

    public Task<bool> AddConfigListAsync(List<AddSettingInfoDto> addSettingInfos) => Task.FromResult(true);
}

internal sealed class FakeDashboardContext : DashboardContext
{
    public FakeDashboardContext(DashboardOptions options, HttpContext httpContext) : base(options)
    {
        Request = new AspNetCoreDashboardRequest(httpContext);
        Response = new AspNetCoreDashboardResponse(httpContext);
    }
}
