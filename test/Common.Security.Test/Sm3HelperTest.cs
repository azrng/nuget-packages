using Common.Security.Enums;
using Xunit.Abstractions;

namespace Common.Security.Test;

public class Sm3HelperTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Sm3HelperTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// 数值类型hash Hex
    /// </summary>
    [Fact]
    public void NumberHash_Hex_ReturnHexOk()
    {
        var str = "123456";
        var result = Sm3Helper.GetSm3Hash(str);
        _testOutputHelper.WriteLine(result);
        Assert.Equal("207CF410532F92A47DEE245CE9B11FF71F578EBD763EB3BBEA44EBD043D018FB", result,
            StringComparer.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// 中文哈希 Hex
    /// </summary>
    [Fact]
    public void StrHash_Hex_ReturnHexOk()
    {
        var str = "你好呀，测试";
        var result = Sm3Helper.GetSm3Hash(str);
        _testOutputHelper.WriteLine(result);
        Assert.Equal("B434CFBE7E1F60E6D3CD9F29B88B715CD10EA65779717501DF5EB54F5812DFB5", result,
            StringComparer.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// 数值类型hash Base64
    /// </summary>
    [Fact]
    public void NumberHash_Base64_ReturnHexOk()
    {
        var str = "123456";
        var result = Sm3Helper.GetSm3Hash(str, OutType.Base64);
        _testOutputHelper.WriteLine(result);
        Assert.Equal("IHz0EFMvkqR97iRc6bEf9x9Xjr12PrO76kTr0EPQGPs=", result, StringComparer.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// 中文哈希 Base64
    /// </summary>
    [Fact]
    public void StrHash_Base64_ReturnHexOk()
    {
        var str = "你好呀，测试";
        var result = Sm3Helper.GetSm3Hash(str, OutType.Base64);
        _testOutputHelper.WriteLine(result);
        Assert.Equal("tDTPvn4fYObTzZ8puItxXNEOpld5cXUB3161T1gS37U=", result, StringComparer.CurrentCultureIgnoreCase);
    }
}