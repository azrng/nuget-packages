using Xunit.Abstractions;

namespace Common.Core.Test.Extension;

public class ObjectExtensionsTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ObjectExtensionsTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// 类转url参数形式  原样拼接
    /// </summary>
    [Fact]
    public void Class_UrlParameter_IgnoreFormat_ReturnOk()
    {
        // 准备
        var userInfo = new UserInfoDto { Id = 1, Name = "Test", };

        // 测试
        var result = userInfo.ToUrlParameter();

        // 断言
        Assert.Equal("Id=1&Name=Test", result);
    }

    /// <summary>
    /// 类转url参数形式  转小写拼接
    /// </summary>
    [Fact]
    public void Class_UrlParameter_NotIgnoreFormat_ReturnOk()
    {
        // 准备
        var userInfo = new UserInfoDto { Id = 1, Name = "Test", };

        // 测试
        var result = userInfo.ToUrlParameter(true);

        // 断言
        Assert.Equal("id=1&name=Test", result);
    }

    [Fact]
    public void ClassToExpand_ReturnOk()
    {
        var userInfo = new UserInfoDto { Id = 1, Name = "Test", };

        dynamic result = userInfo.ToExpandoObject();
        _testOutputHelper.WriteLine(result.Name);
        Assert.True(userInfo.Name == result.Name);
    }
}

file class UserInfoDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}