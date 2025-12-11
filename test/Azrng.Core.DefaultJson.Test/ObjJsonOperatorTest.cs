using Azrng.Core.DefaultJson.Test.Models;
using Xunit;
using Xunit.Abstractions;

namespace Azrng.Core.DefaultJson.Test;

public class ObjJsonOperatorTest
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ITestOutputHelper _testOutputHelper;

    public ObjJsonOperatorTest(IJsonSerializer jsonSerializer, ITestOutputHelper testOutputHelper)
    {
        _jsonSerializer = jsonSerializer;
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// 忽略大小写反序列化
    /// </summary>
    [Fact]
    public void ToObject_IgnoreCaseSwitch()
    {
        var jsonFromJs = "{\"name\": \"张三\", \"age\": 18}";
        var obj = _jsonSerializer.ToObject<User>(jsonFromJs);
        Assert.Equal("张三", obj.Name);
        Assert.Equal(18, obj.Age);
    }

    /// <summary>
    /// 忽略注释反序列化
    /// </summary>
    [Fact]
    public void ToObject_IgnoreComment()
    {
        // 注意3后面有一个尾随逗号
        var nonStandardJson = @"{
    ""FirstName"": ""带注释的JSON"",
    ""Roles"": [
        ""1"",
       ""2"",
        ""3"",
    ]
}";
        var obj = _jsonSerializer.ToObject<UserInfo>(nonStandardJson);
        Assert.Equal("带注释的JSON", obj.FirstName);
        Assert.Equal(3, obj.Roles.Count);
    }

    /// <summary>
    /// 允许从字符串读取数字
    /// </summary>
    [Fact]
    public void ToObject_AllowNumberReadFromString()
    {
        var jsonWithQuotedNumber = @"{""Age"": ""30""}";
        var obj = _jsonSerializer.ToObject<User>(jsonWithQuotedNumber);
        Assert.Equal(30, obj.Age);
    }

    /// <summary>
    /// 序列化的时候忽略null值
    /// </summary>
    [Fact]
    public void ToJson_IgnoreNull()
    {
        var user = new UserInfo { FirstName = "San", UserId = 22, IsAdmin = false };
        var result = _jsonSerializer.ToJson(user);
        _testOutputHelper.WriteLine(result);
    }

    /// <summary>
    /// 序列化忽略循环引用
    /// </summary>
    [Fact]
    public void ToJson_IgnoreCycles()
    {
        var manager = new Employee { Name = "老板" };
        var employee = new Employee { Name = "小钱", Manager = manager };
        manager.DirectReports = new List<Employee> { employee };
        var result = _jsonSerializer.ToJson(manager);
        _testOutputHelper.WriteLine(result);
    }


    /// <summary>
    /// 序列化中文
    /// </summary>
    [Fact]
    public void ToJson_Chinese()
    {
        var escapedJson = _jsonSerializer.ToJson("骚操作");
        _testOutputHelper.WriteLine(escapedJson);
        Assert.Equal("骚操作", escapedJson);
    }

    /// <summary>
    /// 对象序列化测试
    /// </summary>
    [Fact]
    public void ObjToJsonTest()
    {
        var userInfo = new UserInfo
                       {
                           FirstName = "张三",
                           UserId = 10,
                           IsAdmin = false,
                           Roles = new List<string>() { "aa", "bb" },
                           Salary = 10.1,
                           CreatedAt = 1234567899877L
                       };
        var result = _jsonSerializer.ToJson(userInfo);
        _testOutputHelper.WriteLine(result);
        const string str =
            "{\"userId\":10,\"firstName\":\"张三\",\"salary\":10.1,\"isAdmin\":false,\"roles\":[\"aa\",\"bb\"],\"createdAt\":\"1234567899877\"}";
        Assert.Equal(str, result);
    }
}

file class User
{
    public string Name { get; set; }

    public int Age { get; set; }
}

file class Employee
{
    public string Name { get; set; }

    public Employee Manager { get; set; }

    public List<Employee> DirectReports { get; set; }
}