using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Azrng.SettingConfig.Test;

public class DashboardOptionsAndFiltersTests
{
    [Fact]
    public void DashboardOptions_Defaults_ShouldIncludeLocalOnlyAuthorizationFilter()
    {
        var options = new DashboardOptions();

        options.RoutePrefix.Should().Be("systemSetting");
        options.ApiRoutePrefix.Should().Be("/api/setting");
        options.Authorization.Should().ContainSingle();
        options.Authorization!.Single().GetType().Name.Should().Be("LocalRequestsOnlyAuthorizationFilter");
    }

    [Fact]
    public void ParamVerify_WithValidConfiguration_ShouldNotThrow()
    {
        var options = TestInfrastructure.CreateOptions();

        var act = () => options.ParamVerify();

        act.Should().NotThrow();
    }

    [Fact]
    public void ParamVerify_WithoutConnectionString_ShouldThrowArgumentNullException()
    {
        var options = TestInfrastructure.CreateOptions(x => x.DbConnectionString = null);

        var act = () => options.ParamVerify();

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName(nameof(DashboardOptions.DbConnectionString));
    }

    [Fact]
    public void ParamVerify_WithInvalidCacheTime_ShouldThrowArgumentOutOfRangeException()
    {
        var options = TestInfrastructure.CreateOptions(x => x.ConfigCacheTime = 0);

        var act = () => options.ParamVerify();

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName(nameof(DashboardOptions.ConfigCacheTime));
    }

    [Fact]
    public void LocalRequestsOnlyAuthorizationFilter_ShouldAllowLoopbackAndMatchingLocalAddress()
    {
        var filter = TestInfrastructure.CreateLocalRequestsOnlyAuthorizationFilter();
        var loopbackContext = CreateDashboardContext("127.0.0.1", "192.168.1.10");
        var sameAddressContext = CreateDashboardContext("192.168.1.20", "192.168.1.20");

        filter.Authorize(loopbackContext).Should().BeTrue();
        filter.Authorize(sameAddressContext).Should().BeTrue();
    }

    [Fact]
    public void LocalRequestsOnlyAuthorizationFilter_ShouldRejectInvalidOrDifferentRemoteAddress()
    {
        var filter = TestInfrastructure.CreateLocalRequestsOnlyAuthorizationFilter();
        var invalidIpContext = CreateDashboardContext("not-an-ip", "127.0.0.1");
        var differentAddressContext = CreateDashboardContext("192.168.1.30", "192.168.1.20");

        filter.Authorize(invalidIpContext).Should().BeFalse();
        filter.Authorize(differentAddressContext).Should().BeFalse();
    }

    private static DashboardContext CreateDashboardContext(string remoteIp, string? localIp)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.TryParse(remoteIp, out var remoteAddress) ? remoteAddress : IPAddress.None;
        httpContext.Connection.LocalIpAddress = string.IsNullOrWhiteSpace(localIp)
            ? null
            : IPAddress.Parse(localIp);

        if (remoteIp == "not-an-ip")
        {
            httpContext.Connection.RemoteIpAddress = null;
            httpContext.Request.Headers["X-Forwarded-For"] = remoteIp;
        }

        return TestInfrastructure.CreateDashboardContext(httpContext);
    }
}
