using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class SystemHelperTests
{
    [Fact]
    public void GetLocalHostName_ReturnsNonEmptyString()
    {
        var result = SystemHelper.GetLocalHostName();

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetLocalHostName_ReturnsConsistentValue()
    {
        var result1 = SystemHelper.GetLocalHostName();
        var result2 = SystemHelper.GetLocalHostName();

        result1.Should().Be(result2);
    }

    [Fact]
    public void GetAllIpAddress_ReturnsNonEmptyList()
    {
        var result = SystemHelper.GetAllIpAddress();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void GetAllIpAddress_ContainsAtLeastOneNonLoopback()
    {
        var result = SystemHelper.GetAllIpAddress();

        result.Should().Contain(ip => ip != "127.0.0.1" && ip != "::1");
    }

    [Fact]
    public void GetIpv4Address_ReturnsValidIpv4Format()
    {
        var result = SystemHelper.GetIpv4Address();

        if (result != null)
        {
            result.Should().Contain(".");
            System.Net.IPAddress.TryParse(result, out var addr).Should().BeTrue();
            addr!.AddressFamily.Should().Be(System.Net.Sockets.AddressFamily.InterNetwork);
        }
    }

    [Fact]
    public void GetIpv6Address_ReturnsValidIpv6Format()
    {
        var result = SystemHelper.GetIpv6Address();

        if (result != null)
        {
            System.Net.IPAddress.TryParse(result, out var addr).Should().BeTrue();
            addr!.AddressFamily.Should().Be(System.Net.Sockets.AddressFamily.InterNetworkV6);
        }
    }

    [Fact]
    public void GetAllIpAddress_ContainsIpv4IfAvailable()
    {
        var ipv4 = SystemHelper.GetIpv4Address();
        var allIps = SystemHelper.GetAllIpAddress();

        if (ipv4 != null)
        {
            allIps.Should().Contain(ipv4);
        }
    }

    [Fact]
    public void WindowsShell_EchoCommand_ReturnsOutput()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var result = SystemHelper.WindowsShell("echo hello");

        result.Should().Contain("hello");
    }

    [Fact]
    public void WindowsShell_InvalidCommand_ReturnsEmptyOrError()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var result = SystemHelper.WindowsShell("nonexistent-command-xyz");

        result.Should().NotBeNull();
    }

    [Fact]
    public void WindowsShell_EmptyCommand_DoesNotThrow()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var act = () => SystemHelper.WindowsShell("");

        act.Should().NotThrow();
    }

    [Fact]
    public void LinuxShell_EchoCommand_ReturnsOutput()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        var result = SystemHelper.LinuxShell("echo hello");

        result.Should().Contain("hello");
    }

    [Fact]
    public void LinuxShell_InvalidCommand_ReturnsEmptyOrError()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        var result = SystemHelper.LinuxShell("nonexistent-command-xyz");

        result.Should().NotBeNull();
    }

    [Fact]
    public void LinuxShell_EmptyCommand_DoesNotThrow()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        var act = () => SystemHelper.LinuxShell("");

        act.Should().NotThrow();
    }
}
