using Xunit;
using Xunit.Abstractions;

namespace Azrng.Core.DefaultJson.Test;

public class DateTimeTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IJsonSerializer _jsonSerializer;

    public DateTimeTest(ITestOutputHelper testOutputHelper, IJsonSerializer jsonSerializer)
    {
        _testOutputHelper = testOutputHelper;
        _jsonSerializer = jsonSerializer;
    }

    /// <summary>
    /// 格式化时间转字符串
    /// </summary>
    [Fact]
    public void DataTimeToString()
    {
        var origin = new UserInfo()
                     {
                         Id = "10",
                         Time = new DateTime(2024, 6, 29, 20, 50,
                             13)
                     };

        //SysTextJsonSerializer.ObjectToJsonOptions.Converters.Add(new DateTimeToStringConverter());
        var result = _jsonSerializer.ToJson(origin);
        _testOutputHelper.WriteLine(result);
    }

    /// <summary>
    /// 格式化时间转字符串
    /// </summary>
    [Fact]
    public void CustomDataTimeToString()
    {
        var origin = new UserInfo()
                     {
                         Id = "10",
                         Time = new DateTime(2024, 6, 29, 20, 50,
                             13)
                     };

       // SysTextJsonSerializer.ObjectToJsonOptions.Converters.Add(new DateTimeToStringConverter("yyyy/MM/dd HH:mm:ss"));
        var result = _jsonSerializer.ToJson(origin);
        _testOutputHelper.WriteLine(result);
    }

    /// <summary>
    /// 格式化时间转字符串
    /// </summary>
    [Fact]
    public void DateTimeOffsetToString()
    {
        var origin = new UserInfo()
                     {
                         Id = "10",
                         Time2 = new DateTimeOffset(2024, 6, 29, 20, 50,
                             13, TimeSpan.FromMinutes(2))
                     };

        //SysTextJsonSerializer.ObjectToJsonOptions.Converters.Add(new DateTimeOffsetToStringConverter());
        var result = _jsonSerializer.ToJson(origin);
        _testOutputHelper.WriteLine(result);
    }

    /// <summary>
    /// 格式化时间转字符串
    /// </summary>
    [Fact]
    public void CustomDateTimeOffsetToString()
    {
        var origin = new UserInfo()
                     {
                         Id = "10",
                         Time2 = new DateTimeOffset(2024, 6, 29, 20, 50,
                             13, TimeSpan.FromMinutes(2))
                     };

        //SysTextJsonSerializer.ObjectToJsonOptions.Converters.Add(new DateTimeOffsetToStringConverter("yyyy/MM/dd HH:mm:ss"));
        var result = _jsonSerializer.ToJson(origin);
        _testOutputHelper.WriteLine(result);
    }
}

file class UserInfo
{
    public string Id { get; set; }

    public DateTime Time { get; set; }

    public DateTimeOffset Time2 { get; set; }
}