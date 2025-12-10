using Xunit.Abstractions;

namespace Common.Core.Test.Helper.ApplicationHelperTest;

public class ApplicationHelperTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ApplicationHelperTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void AppName_Test()
    {
        var appName = ApplicationHelper.ApplicationName;
        _testOutputHelper.WriteLine(appName);
        Assert.NotEmpty(appName);
    }

    [Fact]
    public void GetLibraryInfo_Test()
    {
        var assembly = typeof(ApplicationHelper).Assembly;
        var info = ApplicationHelper.GetLibraryInfo(assembly);
        _testOutputHelper.WriteLine(info.LibraryVersion);
        Assert.NotNull(info);
    }

    [Fact]
    public void GetRunnerInfo_Test2()
    {
        var info = ApplicationHelper.RuntimeInfo;
        _testOutputHelper.WriteLine(info.MachineName);
        Assert.NotNull(info);
    }
}