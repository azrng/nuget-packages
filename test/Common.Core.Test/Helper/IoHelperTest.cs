using Xunit.Abstractions;

namespace Common.Core.Test.Helper;

public class IoHelperTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public IoHelperTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// 获取应用程序数据目录
    /// </summary>
    [Fact]
    public void GetApplicationDataPath()
    {
        var result = IOHelper.ApplicationDataPath;
        _testOutputHelper.WriteLine(result);
    }

    /// <summary>
    /// 获取桌面文件路径
    /// </summary>
    [Fact]
    public void GetDesktopPath()
    {
        var result = IOHelper.DesktopPath;
        _testOutputHelper.WriteLine(result);
    }
}