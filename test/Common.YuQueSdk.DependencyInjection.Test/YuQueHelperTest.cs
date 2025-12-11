using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Common.YuQueSdk.Test;

/// <summary>
/// 用户知识库测试
/// </summary>
public class YuQueHelperTest
{
    private readonly IYuQueHelper _helper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<YuQueHelperTest> _logger;
    private readonly ITestOutputHelper _testOutputHelper;

    public YuQueHelperTest(
        IYuQueHelper helper,
        IHttpClientFactory httpClientFactory,
        ILogger<YuQueHelperTest> logger,
        ITestOutputHelper testOutputHelper
    )
    {
        _helper = helper;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// 获取用户仓库列表
    /// </summary>
    [Fact]
    public async Task GetUserRepositoryList_ReturnOk()
    {
        var login = "azrng";

        // 可以输出
        _testOutputHelper.WriteLine("aaaaaaaaaaaaaaa");

        // 没有输出
        _logger.LogInformation($"输出测试内容：{login}");

        var response = await _helper.GetRepoListByLoginNameAsync(login);
        Assert.True(response.Data.Any());
    }

    /// <summary>
    /// 获取用户指定仓库内文档列表
    /// </summary>
    [Fact]
    public async Task GetList_ReturnOk()
    {
        var repositoryId = 24840528;
        var response = await _helper.GetRepoTopicListAsync(repositoryId);
        Assert.True(response.Data.Any());
    }

    /// <summary>
    /// 获取单篇文档的详细信息
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetDocContent_ReturnOk()
    {
        var response = await _helper.GetReposDocsAsync(10874582, "op4g0u");
        Assert.True(response.IsSuccess);
    }

    /// <summary>
    /// 获取文档内容并提取内容保存到本地
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetDocContent_SaveImg_ReturnOk()
    {
        var path = "d://temp";
        var response = await _helper.GetReposDocsAsync(10874582, "op4g0u");
        Assert.True(response.IsSuccess);
        var body = response.Data.Body;

        var imgPath = Path.Combine(path, "images");
        if (!Directory.Exists(imgPath))
        {
            Directory.CreateDirectory(imgPath);
        }

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Referer", "https://www.yuque.com/");
        client.DefaultRequestHeaders.Add(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36"
        );

        var regex = new Regex("!\\[.*?\\]\\((.*?)\\)");
        var matches = regex.Matches(body);
        foreach (Match item in matches)
        {
            // 获取图片地址
            var imageUrl = item.Groups[1].Value;

            // 下载图片并保存
            var imageName = Path.GetFileName(imageUrl);
            // 如果已经存在相同的名字，那么就更换名字  或者地址后缀都不是正常后缀就重新起名字
            if (
                File.Exists(Path.Combine(imgPath, imageName))
                || !imageName.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase)
                || !imageName.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase)
            )
            {
                imageName = Guid.NewGuid() + ".jpg";
            }

            // 下载图片并保存
            var bytes = await client.GetByteArrayAsync(imageUrl);
            await File.WriteAllBytesAsync(Path.Combine(imgPath, imageName), bytes);

            // 替换图片连接为本地路径
            var relativePath = "/images/" + imageName;
            body = body.Replace(imageUrl, relativePath);
        }

        await File.WriteAllTextAsync(Path.Combine(path, "image.md"), body);
    }
}
