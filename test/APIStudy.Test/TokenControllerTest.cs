namespace APIStudy.Test;

/// <summary>
/// 集成测试
/// </summary>
public class TokenControllerTest : BaseControllerTest<Startup>
{
    private readonly HttpClient _client;

    public TokenControllerTest(CustomWebApplicationFactory<Startup> factory)
        : base(factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/api/token/gettoken")]
    public async void Test1(string url)
    {
        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
    }
}


//public class MdmClientTest : BaseControllerTest<Startup>
//{
//    private readonly IServiceProvider _serviceProvider;

//    public MdmClientTest(CustomWebApplicationFactory<Startup> factory)
//        : base(factory)
//    {
//        _serviceProvider = factory.Services;
//    }

//    /// <summary>
//    /// 获取人员信息
//    /// </summary>
//    [Fact]
//    public async void GetConceptList()
//    {
//        using var scope = _serviceProvider.CreateScope();
//        var _mdmHttpClient = scope.ServiceProvider.GetRequiredService<IMdmHttpClient>();
//        var list = await _mdmHttpClient.GetMdmConceptListAsync(false, 1, 10000, true, "", "").ConfigureAwait(false);
//        Assert.True(list.Concept.Count > 0);
//    }
//}