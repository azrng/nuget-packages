using Azrng.Core.Extension;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Common.HttpClients.Test;

/// <summary>
/// apifox https://echo.apifox.com/api-39492114
/// </summary>
public class ApifoxClientTest
{
    private readonly IHttpHelper _httpHelper;
    private readonly ITestOutputHelper _testOutputHelper;

    private readonly string Host = "https://echo.apifox.com";

    public ApifoxClientTest(IHttpHelper httpHelper, ITestOutputHelper testOutputHelper)
    {
        _httpHelper = httpHelper;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Get_ReturnOk()
    {
        var result = await _httpHelper.GetAsync<string>(Host + "/get?q1=11&q2=22");
        Assert.NotNull(result);
        _testOutputHelper.WriteLine("请求结束");
        _testOutputHelper.WriteLine(result);
    }

    // /// <summary>
    // /// 调用接口抛出404异常
    // /// </summary>
    // [Fact]
    // public async Task Get_ReturnOk_Throw404()
    // {
    //     var ex = await Assert.ThrowsAsync<HttpRequestException>(async () =>
    //     {
    //         await _httpHelper.GetAsync<string>(Host + "/aaget2?q1=11&q2=22");
    //     });
    //
    //     Assert.True(ex is not null);
    // }

    [Fact]
    public async Task Post_ReturnOk()
    {
        var content = "{\"q\":\"123456\",\"a\":\"222\"}";
        var result = await _httpHelper.PostAsync<string>(Host + "/post", content);
        Assert.NotNull(result);
        _testOutputHelper.WriteLine("请求结束");
        _testOutputHelper.WriteLine(result);
    }

    [Fact]
    public async Task Put_ReturnOk()
    {
        var content = "{\"q\":\"123456\",\"a\":\"222\"}";
        var result = await _httpHelper.PutAsync<string>(Host + "/put", content);
        Assert.NotNull(result);
        _testOutputHelper.WriteLine("请求结束");
        _testOutputHelper.WriteLine(result);
    }

    // [Fact]
    // public async Task Delete_ReturnOk()
    // {
    //     var content = "{\"q\":\"123456\",\"a\":\"222\"}";
    //     var result = await _httpHelper.DeleteAsync<string>(Host + "/put", content);
    //     Assert.NotNull(result);
    //     _testOutputHelper.WriteLine("请求结束");
    //     _testOutputHelper.WriteLine(result);
    // }

    [Fact]
    public async Task PostFormData_ReturnOk()
    {
        var content = new Dictionary<string, string> { { "q", "123456" }, { "a", "222" } };
        var result = await _httpHelper.PostFormDataAsync<string>(Host + "/post", content);
        Assert.NotNull(result);
        _testOutputHelper.WriteLine("请求结束");
        _testOutputHelper.WriteLine(result);
    }

    [Fact]
    public async Task GetImages_ReturnOk()
    {
        var result = await _httpHelper.GetStreamAsync(Host + "/image/jpeg");
        Assert.NotNull(result);
        _testOutputHelper.WriteLine("请求结束");
        using var ms = new MemoryStream();
        await result.CopyToAsync(ms);
        _testOutputHelper.WriteLine(ms.Length.ToString());
    }

    /// <summary>
    /// 上传文件测试
    /// </summary>
    [Fact]
    public async Task PostFile_ReturnOK()
    {
        var file = Encoding.UTF8.GetBytes("测试数据").ToStream();
        var result = await _httpHelper.PostFormDataAsync<string>(Host + "/post", "file", file, "file.txt");
        Assert.NotNull(result);
        _testOutputHelper.WriteLine("请求结束");
        _testOutputHelper.WriteLine(result);
    }
}