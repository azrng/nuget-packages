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
        var currValue = "customer";
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
        var currValue = " ";
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

    #region 类型判断

    /// <summary>
    /// 测试 IsBool 方法 - 是否是bool类型
    /// </summary>
    [Theory]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData("是", true)]
    [InlineData("否", false)]
    [InlineData("yes", true)]
    [InlineData("no", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("  ", false)]
    public void IsBool_ReturnOk(string str, bool expected)
    {
        var result = str.IsBool();
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 测试 IsDateFormat 方法 - 是否是日期类型
    /// </summary>
    [Theory]
    [InlineData("2023-01-01", true)]
    [InlineData("2023/01/01", true)]
    [InlineData("2023-01-01 12:00:00", true)]
    [InlineData("abc", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void IsDateFormat_ReturnOk(string str, bool expected)
    {
        var result = str.IsDateFormat();
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 测试 ToDateTime 方法 - 字符串转可空日期类型
    /// </summary>
    [Theory]
    [InlineData("2023-01-01", 2023, 1, 1)]
    [InlineData("2023-12-31", 2023, 12, 31)]
    [InlineData("abc", null, null, null)]
    [InlineData(null, null, null, null)]
    [InlineData("", null, null, null)]
    public void ToDateTime_ReturnOk(string str, int? year, int? month, int? day)
    {
        var result = str.ToDateTime();
        if (year.HasValue)
        {
            Assert.NotNull(result);
            Assert.Equal(year.Value, result.Value.Year);
            Assert.Equal(month.Value, result.Value.Month);
            Assert.Equal(day.Value, result.Value.Day);
        }
        else
        {
            Assert.Null(result);
        }
    }

    /// <summary>
    /// 测试 ToGuid 方法 - 字符串转Guid
    /// </summary>
    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
    [InlineData("abc", "00000000-0000-0000-0000-000000000000")]
    [InlineData(null, "00000000-0000-0000-0000-000000000000")]
    [InlineData("", "00000000-0000-0000-0000-000000000000")]
    public void ToGuid_ReturnOk(string str, string expectedGuidStr)
    {
        var result = str.ToGuid();
        var expectedGuid = Guid.Parse(expectedGuidStr);
        Assert.Equal(expectedGuid, result);
    }

    /// <summary>
    /// 测试 HasChinese 方法 - 判断是否包含中文
    /// </summary>
    [Theory]
    [InlineData("你好", true)]
    [InlineData("Hello 你好", true)]
    [InlineData("Hello", false)]
    [InlineData("123", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void HasChinese_ReturnOk(string str, bool expected)
    {
        var result = str.HasChinese();
        Assert.Equal(expected, result);
    }

    #endregion

    #region 值判断

    /// <summary>
    /// 测试 IsNotNullOrWhiteSpace 方法 - 判断字符串不是 null、空和空白字符
    /// </summary>
    [Theory]
    [InlineData("abc", true)]
    [InlineData("  a  ", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void IsNotNullOrWhiteSpace_ReturnOk(string str, bool expected)
    {
        var result = str.IsNotNullOrWhiteSpace();
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 测试 IsNullOrWhiteSpace 方法 - 判断字符串是 null、空和空白字符
    /// </summary>
    [Theory]
    [InlineData("abc", false)]
    [InlineData("  a  ", false)]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    public void IsNullOrWhiteSpace_ReturnOk(string str, bool expected)
    {
        var result = str.IsNullOrWhiteSpace();
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 测试 IsNullOrEmpty 方法 - 判断字符串是 null、空
    /// </summary>
    [Theory]
    [InlineData("abc", false)]
    [InlineData("   ", false)]
    [InlineData(null, true)]
    [InlineData("", true)]
    public void IsNullOrEmpty_ReturnOk(string str, bool expected)
    {
        var result = str.IsNullOrEmpty();
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 测试 IsNotNullOrEmpty 方法 - 判断字符串不是 null、空
    /// </summary>
    [Theory]
    [InlineData("abc", true)]
    [InlineData("   ", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void IsNotNullOrEmpty_ReturnOk(string str, bool expected)
    {
        var result = str.IsNotNullOrEmpty();
        Assert.Equal(expected, result);
    }

    #endregion

    #region 字符串操作

    /// <summary>
    /// 测试 ToStrArray 方法 - 字符串分割成字符串数组
    /// </summary>
    [Theory]
    [InlineData("a,b,c", ",", new[]
                              {
                                  "a",
                                  "b",
                                  "c"
                              })]
    [InlineData("a|b|c", "|", new[]
                              {
                                  "a",
                                  "b",
                                  "c"
                              })]
    [InlineData("a,b,", ",", new[]
                             {
                                 "a",
                                 "b"
                             })]
    [InlineData("a,,b", ",", new[]
                             {
                                 "a",
                                 "b"
                             })]
    [InlineData("a", ",", new[]
                          {
                              "a"
                          })]
    public void ToStrArray_ReturnOk(string str, string separator, string[] expected)
    {
        var result = str.ToStrArray(separator);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 测试 GetByIndex 方法 - 获取特定位置的字符串
    /// </summary>
    [Theory]
    [InlineData("abc", 0, "a")]
    [InlineData("abc", 1, "b")]
    [InlineData("abc", 2, "c")]
    [InlineData("abc", -1, "")]
    [InlineData("abc", 3, "")]
    [InlineData("abc", 10, "")]
    public void GetByIndex_ReturnOk(string str, int index, string expected)
    {
        var result = str.GetByIndex(index);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 测试 EqualsNoCase 方法 - 忽略大小写的字符串比较
    /// </summary>
    [Theory]
    [InlineData("Hello", "hello", true)]
    [InlineData("HELLO", "hello", true)]
    [InlineData("Hello", "Hello", true)]
    [InlineData("abc", "def", false)]
    [InlineData(null, null, false)]
    [InlineData("", "", true)]
    public void EqualsNoCase_ReturnOk(string str1, string str2, bool expected)
    {
        var result = str1.EqualsNoCase(str2);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 测试 RemoveControlChars 方法 - 移除控制字符
    /// </summary>
    [Fact]
    public void RemoveControlChars_ReturnOk()
    {
        var str = "Hello\u0001World\u0002";
        var result = str.RemoveControlChars();
        Assert.Equal("HelloWorld", result);
    }

    /// <summary>
    /// 测试 RemoveControlChars 方法 - null 返回 null
    /// </summary>
    [Fact]
    public void RemoveControlChars_Null_ReturnNull()
    {
        string str = null;
        var result = str.RemoveControlChars();
        Assert.Null(result);
    }

    /// <summary>
    /// 测试 ToUpperFirst 方法 - 字符串首字母大写
    /// </summary>
    [Theory]
    [InlineData("hello", "Hello")]
    [InlineData("HELLO", "HELLO")]
    [InlineData("hELLO", "HELLO")]
    [InlineData("a", "A")]
    [InlineData(null, null)]
    [InlineData("", "")]
    public void ToUpperFirst_ReturnOk(string str, string expected)
    {
        var result = str.ToUpperFirst();
        Assert.Equal(expected, result);
    }

    #endregion

    #region Base64

    /// <summary>
    /// 测试 ToBytesByBase64 方法 - base64转byte[]
    /// </summary>
    [Fact]
    public void ToBytesByBase64_ReturnOk()
    {
        var base64Str = "SGVsbG8=";
        var result = base64Str.ToBytesByBase64();
        Assert.Equal(5, result.Length);
        Assert.Equal((byte)'H', result[0]);
        Assert.Equal((byte)'e', result[1]);
    }

    #endregion

    /// <summary>
    /// 测试 ToVersionNumber 方法 - 版本号转数值
    /// </summary>
    [Theory]
    [InlineData("1.1.1", 10101)]
    [InlineData("1.2.3", 10203)]
    [InlineData("1.10.1", 11001)]
    [InlineData("v1.2.3", 10203)]
    [InlineData("1.2.3.txt", 10203)]
    [InlineData("v1.2.3.txt", 10203)]
    [InlineData("1.2.3.4", 1020304)]
    [InlineData("1.2", 0)]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    public void ToVersionNumber_ReturnOk(string version, int expected)
    {
        var result = version.ToVersionNumber();
        Assert.Equal(expected, result);
    }
}