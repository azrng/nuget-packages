using System.Globalization;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace Common.Core.Test.Helper;

public class CommonHelperTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CommonHelperTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }



    /// <summary>
    /// 生成长度不定的随机数
    /// </summary>
    [Fact]
    public void GenRandomString_ReturnOk()
    {
        var result = CommonHelper.GenerateRandomNumber();
        _testOutputHelper.WriteLine(result);
    }


}