using System.Text;

namespace Common.Core.Test.Extension;

public class StringExtensionTest
{
    #region 基础类型转换

    /// <summary>
    /// 是否是int类型
    /// </summary>
    /// <param name="str"></param>
    [Theory]
    [InlineData("5")]
    [InlineData("6")]
    public void IsIntFormat_ReturnTrue(string str)
    {
        Assert.True(str.IsIntFormat());
    }

    /// <summary>
    /// 不是int类型
    /// </summary>
    /// <param name="str"></param>
    [Theory]
    [InlineData("aa")]
    [InlineData("bb")]
    [InlineData(null)]
    public void IsNotIntFormat_ReturnFalse(string str)
    {
        Assert.False(str.IsIntFormat());
    }

    /// <summary>
    /// null转int类型返回0
    /// </summary>
    [Fact]
    public void NullToInt_Return0()
    {
        // 准备
        string source = null;

        // 行为
        var result = source.ToInt();

        // 断言
        Assert.Equal(0, result);
    }

    /// <summary>
    /// 字符串转float
    /// </summary>
    [Fact]
    public void StringToFloat_ReturnOk()
    {
        // 准备
        var source = "1.2";

        // 行为
        var result = source.ToFloat();

        // 断言
        Assert.True(source.IsFloatFormat());
        Assert.Equal(1.2f, result);
    }

    /// <summary>
    /// 字符串转double
    /// </summary>
    [Fact]
    public void StringToDouble_ReturnOk()
    {
        // 准备
        var source = "1.22";

        // 行为
        var result = source.ToDouble();

        // 断言
        Assert.True(source.IsDoubleFormat());
        Assert.Equal(1.22d, result);
    }


    /// <summary>
    /// 字符串转double
    /// </summary>
    [Fact]
    public void StringToLong_ReturnOk()
    {
        // 准备
        var source = "123456789";

        // 行为
        var result = source.ToInt64();

        // 断言
        Assert.True(source.IsInt64Format());
        Assert.Equal(123456789, result);
    }

    /// <summary>
    /// 是decimal类型返回true
    /// </summary>
    /// <param name="str"></param>
    [Theory]
    [InlineData("5.5")]
    [InlineData("6.6")]
    public void IsDecimalFormat_ReturnTrue(string str)
    {
        Assert.True(str.IsDecimalFormat());
    }

    /// <summary>
    /// 不是decimal类型返回true
    /// </summary>
    /// <param name="str"></param>
    [Theory]
    [InlineData("aa")]
    [InlineData("bb")]
    public void IsNotDecimalFormat_ReturnFalse(string str)
    {
        Assert.False(str.IsDecimalFormat());
    }

    /// <summary>
    /// 不是decimal类型返回true
    /// </summary>
    [Fact]
    public void NullDecimalFormat_ReturnFalse()
    {
        // 准备
        string source = null;

        // 行为
        var result = source.ToDecimal();

        // 断言
        Assert.Equal(0m, result);
    }

    #endregion

    /// <summary>
    /// null值获取默认值，返回default
    /// </summary>
    [Fact]
    public void NullGetDefault_ReturnDefault()
    {
        string currValue = null;
        var defaultValue = "default";
        var result = currValue.GetOrDefault(defaultValue);
        Assert.Equal(defaultValue, result);
    }

    /// <summary>
    /// 普通值获取默认值返回原始的值
    /// </summary>
    [Fact]
    public void CustomerValueGetDefault_ReturnSource()
    {
        string currValue = "customer";
        var defaultValue = "default";
        var result = currValue.GetOrDefault(defaultValue);
        Assert.NotEqual(defaultValue, result);
        Assert.Equal(currValue, result);
    }

    /// <summary>
    /// 空格获取默认值返回默认值
    /// </summary>
    [Fact]
    public void EmptyGetDefault_ReturnSource()
    {
        string currValue = " ";
        var defaultValue = "default";
        var result = currValue.GetOrDefault(defaultValue);
        Assert.Equal(defaultValue, result);
    }

    [Theory]
    [InlineData("aa")]
    [InlineData("张安神鼎飞丹砂发的是")]
    [InlineData("1234545787")]
    public void StrToBase64_ReturnTrue(string originStr)
    {
        var base64Str = originStr.ToBase64Encode();
        var str = base64Str.FromBase64Decode();
        Assert.Equal(originStr, str);
    }

    #region 进制转换

    /// <summary>
    /// 十六进制转二进制测试
    /// </summary>
    [Fact]
    public void HexToBytes()
    {
        // Arrange
        var origin = "123456";
        var bytes = Encoding.UTF8.GetBytes(origin);
        var hexStr = bytes.ToHexString();

        // Act
        var result = hexStr.ToBytesFromHexString();

        // Assert
        Assert.Equal(bytes, result);
    }

    /// <summary>
    /// 字符串转二进制测试
    /// </summary>
    [Fact]
    public void StringToBytes()
    {
        // Arrange
        var origin = "123456789";
        var bytes = Encoding.UTF8.GetBytes(origin);

        // Act
        var result = origin.ToBytes();

        // Assert
        Assert.Equal(bytes, result);
    }

    #endregion
}