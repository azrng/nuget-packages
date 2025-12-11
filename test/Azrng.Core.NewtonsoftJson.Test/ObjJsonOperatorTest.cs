using Azrng.Core.DefaultJson.Test.Models;
using Azrng.Core.NewtonsoftJson.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Azrng.Core.NewtonsoftJson.Test;

public class ObjJsonOperatorTest
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ITestOutputHelper _testOutputHelper;

    public ObjJsonOperatorTest(IJsonSerializer jsonSerializer, ITestOutputHelper testOutputHelper)
    {
        _jsonSerializer = jsonSerializer;
        _testOutputHelper = testOutputHelper;
    }

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

    [Fact]
    public void ObjCloneTest()
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
        var result = JsonHelper.Clone(userInfo);
        Assert.NotNull(result);
    }
}