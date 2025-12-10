using Xunit.Abstractions;

namespace Common.Core.Test.Helper;

public class RmbHelperTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RmbHelperTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// 小数转大写
    /// </summary>
    [Fact]
    public void DoubleRmbToUpper_ReturnOk()
    {
        var origin = 1.2346m;
        var result = RmbHelper.ToRmbUpper(origin);
        _testOutputHelper.WriteLine(result);
        Assert.Equal("壹元贰角叁分", result);
    }

    /// <summary>
    /// 小数转大写  四舍五入
    /// </summary>
    [Fact]
    public void DoubleRmbToUpper2_ReturnOk()
    {
        var origin = 1.247m;
        var result = RmbHelper.ToRmbUpper(origin);
        _testOutputHelper.WriteLine(result);
        Assert.Equal("壹元贰角伍分", result);
    }

    /// <summary>
    /// int数值转大写
    /// </summary>
    [Fact]
    public void IntRmbToUpper_ReturnOk()
    {
        var origin = 985667851m;
        var result = RmbHelper.ToRmbUpper(origin);
        _testOutputHelper.WriteLine(result);
        Assert.Equal(result, "玖亿捌仟伍佰陆拾陆万柒仟捌佰伍拾壹元整");
    }
}