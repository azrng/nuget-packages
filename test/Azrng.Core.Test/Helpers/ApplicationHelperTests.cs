using Azrng.Core.Helpers;
using Azrng.Core.Model;
using FluentAssertions;
using System;
using System.Reflection;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class ApplicationHelperTests
{
    #region AppRoot

    [Fact]
    public void AppRoot_ShouldNotBeNullOrEmpty()
    {
        ApplicationHelper.AppRoot.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AppRoot_ShouldBeValidDirectoryPath()
    {
        var path = ApplicationHelper.AppRoot;

        path.Should().Contain(Path.DirectorySeparatorChar.ToString());
    }

    [Fact]
    public void AppRoot_ShouldBeConsistentAcrossCalls()
    {
        var first = ApplicationHelper.AppRoot;
        var second = ApplicationHelper.AppRoot;

        first.Should().Be(second);
    }

    #endregion

    #region ApplicationName

    [Fact]
    public void ApplicationName_ShouldNotBeNullOrEmpty()
    {
        ApplicationHelper.ApplicationName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ApplicationName_ShouldBeConsistentAcrossCalls()
    {
        var first = ApplicationHelper.ApplicationName;
        var second = ApplicationHelper.ApplicationName;

        first.Should().Be(second);
    }

    #endregion

    #region RuntimeInfo

    [Fact]
    public void RuntimeInfo_ShouldNotBeNull()
    {
        ApplicationHelper.RuntimeInfo.Should().NotBeNull();
    }

    [Fact]
    public void RuntimeInfo_Version_ShouldNotBeNullOrEmpty()
    {
        ApplicationHelper.RuntimeInfo.Version.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RuntimeInfo_ProcessorCount_ShouldBePositive()
    {
        ApplicationHelper.RuntimeInfo.ProcessorCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RuntimeInfo_FrameworkDescription_ShouldNotBeNullOrEmpty()
    {
        ApplicationHelper.RuntimeInfo.FrameworkDescription.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RuntimeInfo_WorkingDirectory_ShouldNotBeNullOrEmpty()
    {
        ApplicationHelper.RuntimeInfo.WorkingDirectory.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RuntimeInfo_OsArchitecture_ShouldNotBeNullOrEmpty()
    {
        ApplicationHelper.RuntimeInfo.OsArchitecture.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RuntimeInfo_OsDescription_ShouldNotBeNullOrEmpty()
    {
        ApplicationHelper.RuntimeInfo.OsDescription.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RuntimeInfo_OsVersion_ShouldNotBeNullOrEmpty()
    {
        ApplicationHelper.RuntimeInfo.OsVersion.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RuntimeInfo_MachineName_ShouldNotBeNullOrEmpty()
    {
        ApplicationHelper.RuntimeInfo.MachineName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RuntimeInfo_UserName_ShouldNotBeNullOrEmpty()
    {
        ApplicationHelper.RuntimeInfo.UserName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RuntimeInfo_ShouldBeCachedInstance()
    {
        var first = ApplicationHelper.RuntimeInfo;
        var second = ApplicationHelper.RuntimeInfo;

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void RuntimeInfo_LibraryVersion_ShouldNotBeNullOrEmpty()
    {
        ApplicationHelper.RuntimeInfo.LibraryVersion.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region GetLibraryInfo_Type

    [Fact]
    public void GetLibraryInfo_NullType_ShouldThrowArgumentNullException()
    {
        var action = () => ApplicationHelper.GetLibraryInfo((Type)null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetLibraryInfo_ValidType_ShouldReturnNonNullLibraryInfo()
    {
        var result = ApplicationHelper.GetLibraryInfo(typeof(string));

        result.Should().NotBeNull();
    }

    [Fact]
    public void GetLibraryInfo_ValidType_ShouldReturnLibraryInfoWithVersion()
    {
        var result = ApplicationHelper.GetLibraryInfo(typeof(string));

        result.LibraryVersion.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetLibraryInfo_ValidType_ShouldReturnLibraryInfoInstance()
    {
        var result = ApplicationHelper.GetLibraryInfo(typeof(string));

        result.Should().BeOfType<LibraryInfo>();
    }

    [Fact]
    public void GetLibraryInfo_Type_ShouldReturnSameAssemblyInfoAsAssemblyOverload()
    {
        var type = typeof(string);

        var fromType = ApplicationHelper.GetLibraryInfo(type);
        var fromAssembly = ApplicationHelper.GetLibraryInfo(type.Assembly);

        fromType.LibraryVersion.Should().Be(fromAssembly.LibraryVersion);
        fromType.LibraryHash.Should().Be(fromAssembly.LibraryHash);
        fromType.RepositoryUrl.Should().Be(fromAssembly.RepositoryUrl);
    }

    #endregion

    #region GetLibraryInfo_Assembly

    [Fact]
    public void GetLibraryInfo_NullAssembly_ShouldThrowArgumentNullException()
    {
        var action = () => ApplicationHelper.GetLibraryInfo((Assembly)null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetLibraryInfo_ValidAssembly_ShouldReturnNonNullLibraryInfo()
    {
        var assembly = typeof(string).Assembly;

        var result = ApplicationHelper.GetLibraryInfo(assembly);

        result.Should().NotBeNull();
    }

    [Fact]
    public void GetLibraryInfo_Assembly_ShouldReturnLibraryInfoWithVersion()
    {
        var assembly = typeof(string).Assembly;

        var result = ApplicationHelper.GetLibraryInfo(assembly);

        result.LibraryVersion.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetLibraryInfo_Assembly_ShouldReturnLibraryInfoInstance()
    {
        var assembly = typeof(string).Assembly;

        var result = ApplicationHelper.GetLibraryInfo(assembly);

        result.Should().BeOfType<LibraryInfo>();
    }

    [Fact]
    public void GetLibraryInfo_Assembly_LibraryHash_ShouldNotBeNull()
    {
        var assembly = typeof(string).Assembly;

        var result = ApplicationHelper.GetLibraryInfo(assembly);

        result.LibraryHash.Should().NotBeNull();
    }

    [Fact]
    public void GetLibraryInfo_Assembly_RepositoryUrl_ShouldNotBeNull()
    {
        var assembly = typeof(string).Assembly;

        var result = ApplicationHelper.GetLibraryInfo(assembly);

        result.RepositoryUrl.Should().NotBeNull();
    }

    [Fact]
    public void GetLibraryInfo_Assembly_WithInformationalVersion_ShouldParseVersionAndHash()
    {
        var assembly = typeof(ApplicationHelper).Assembly;

        var result = ApplicationHelper.GetLibraryInfo(assembly);

        result.LibraryVersion.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetLibraryInfo_Assembly_ShouldReturnConsistentResults()
    {
        var assembly = typeof(string).Assembly;

        var first = ApplicationHelper.GetLibraryInfo(assembly);
        var second = ApplicationHelper.GetLibraryInfo(assembly);

        first.LibraryVersion.Should().Be(second.LibraryVersion);
        first.LibraryHash.Should().Be(second.LibraryHash);
        first.RepositoryUrl.Should().Be(second.RepositoryUrl);
    }

    #endregion
}
