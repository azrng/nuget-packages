using Xunit.Abstractions;

namespace Common.Core.Test.Helper;

/// <summary>
/// IP帮助类
/// </summary>
public class IpHelperTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public IpHelperTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void SampleConvert_ReturnOk()
    {
        // 准备
        var source = "192.168.1.156";

        // 行为
        var result = IpHelper.ToLongFromIp(source);
        _testOutputHelper.WriteLine(result.ToString());
        var source2 = IpHelper.ToIpFromLong(result);
        // 断言
        Assert.Equal(source, source2);
    }

    [Theory]
    [InlineData("0.0.0.0")]
    [InlineData("45.104.255.61")]
    [InlineData("255.255.0.0")]
    [InlineData("192.168.0.1")]
    public void IPConvert_ReturnOk(string source)
    {
        var result = IpHelper.ToLongFromIp(source);
    }
}