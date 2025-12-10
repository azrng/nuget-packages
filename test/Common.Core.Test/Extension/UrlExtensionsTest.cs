using Xunit.Abstractions;

namespace Common.Core.Test.Extension;

public class UrlExtensionsTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UrlExtensionsTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// querystring转字典
    /// </summary>
    [Fact]
    public void QueryStringToDict_ReturnOk()
    {
        // 准备
        var uri = new Uri("https://www.example.com/?name=John&age=30&city=henan");

        // 行为
        var result = UrlHelper.ToDictFromQueryString(uri);
        _testOutputHelper.WriteLine(string.Join(",", result.Keys));

        // 断言
        Assert.True(result.Count == 3);
        Assert.True(result.ContainsKey("name"));
    }
}