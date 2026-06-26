using Azrng.Core.Model;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Model;

public class RuntimeInfoTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeStringPropertiesToEmpty()
    {
        var info = new RuntimeInfo();

        info.Version.Should().BeEmpty();
        info.FrameworkDescription.Should().BeEmpty();
        info.OsArchitecture.Should().BeEmpty();
        info.OsDescription.Should().BeEmpty();
        info.OsVersion.Should().BeEmpty();
        info.MachineName.Should().BeEmpty();
        info.UserName.Should().BeEmpty();
        info.RuntimeIdentifier.Should().BeEmpty();
        info.WorkingDirectory.Should().BeEmpty();
        info.ProcessPath.Should().BeEmpty();
    }

    [Fact]
    public void DefaultConstructor_ShouldInitializeNumericPropertiesToZero()
    {
        var info = new RuntimeInfo();

        info.ProcessorCount.Should().Be(0);
        info.ProcessId.Should().Be(0);
    }

    [Fact]
    public void DefaultConstructor_ShouldInitializeBoolPropertiesToFalse()
    {
        var info = new RuntimeInfo();

        info.IsServerGc.Should().BeFalse();
        info.IsInContainer.Should().BeFalse();
        info.IsInKubernetes.Should().BeFalse();
    }

    [Fact]
    public void DefaultConstructor_ShouldInheritLibraryInfoDefaults()
    {
        var info = new RuntimeInfo();

        info.LibraryVersion.Should().BeEmpty();
        info.LibraryHash.Should().BeEmpty();
        info.RepositoryUrl.Should().BeEmpty();
    }

    [Fact]
    public void Version_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.Version = "1.0.0";
        info.Version.Should().Be("1.0.0");
    }

    [Fact]
    public void FrameworkDescription_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.FrameworkDescription = ".NET 8.0";
        info.FrameworkDescription.Should().Be(".NET 8.0");
    }

    [Fact]
    public void ProcessorCount_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.ProcessorCount = 8;
        info.ProcessorCount.Should().Be(8);
    }

    [Fact]
    public void OsArchitecture_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.OsArchitecture = "X64";
        info.OsArchitecture.Should().Be("X64");
    }

    [Fact]
    public void OsDescription_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.OsDescription = "Windows 10";
        info.OsDescription.Should().Be("Windows 10");
    }

    [Fact]
    public void OsVersion_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.OsVersion = "10.0.19041";
        info.OsVersion.Should().Be("10.0.19041");
    }

    [Fact]
    public void MachineName_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.MachineName = "SERVER01";
        info.MachineName.Should().Be("SERVER01");
    }

    [Fact]
    public void UserName_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.UserName = "admin";
        info.UserName.Should().Be("admin");
    }

    [Fact]
    public void RuntimeIdentifier_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.RuntimeIdentifier = "win-x64";
        info.RuntimeIdentifier.Should().Be("win-x64");
    }

    [Fact]
    public void IsServerGc_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.IsServerGc = true;
        info.IsServerGc.Should().BeTrue();
    }

    [Fact]
    public void WorkingDirectory_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.WorkingDirectory = @"C:\app";
        info.WorkingDirectory.Should().Be(@"C:\app");
    }

    [Fact]
    public void ProcessId_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.ProcessId = 12345;
        info.ProcessId.Should().Be(12345);
    }

    [Fact]
    public void ProcessPath_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.ProcessPath = @"C:\app\process.exe";
        info.ProcessPath.Should().Be(@"C:\app\process.exe");
    }

    [Fact]
    public void IsInContainer_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.IsInContainer = true;
        info.IsInContainer.Should().BeTrue();
    }

    [Fact]
    public void IsInKubernetes_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.IsInKubernetes = true;
        info.IsInKubernetes.Should().BeTrue();
    }

    [Fact]
    public void LibraryVersion_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.LibraryVersion = "2.0.0";
        info.LibraryVersion.Should().Be("2.0.0");
    }

    [Fact]
    public void LibraryHash_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.LibraryHash = "abc123";
        info.LibraryHash.Should().Be("abc123");
    }

    [Fact]
    public void RepositoryUrl_SetAndGet_ShouldWork()
    {
        var info = new RuntimeInfo();
        info.RepositoryUrl = "https://github.com/example/repo";
        info.RepositoryUrl.Should().Be("https://github.com/example/repo");
    }
}
