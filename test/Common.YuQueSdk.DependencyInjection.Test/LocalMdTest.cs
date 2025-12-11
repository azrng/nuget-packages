using System.Text.RegularExpressions;

namespace Common.YuQueSdk.DependencyInjection.Test;

/// <summary>
/// 本地md的一些操作
/// </summary>
public class LocalMdTest
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LocalMdTest(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// 提取md中的图片并保存到指定目录
    /// </summary>
    [Fact]
    public async Task CopyImage_Return()
    {
        // 原始md文件地址
        const string rootPath = "D:\\temp\\docs\\软件集";
        // 原始md文件本地图片存放地址
        const string imageDir = "D:\\Gitee\\kbms-origin\\src\\.vuepress\\public\\common";

        // 图片新存放地址
        const string copyImageDir = "D:\\Gitee\\kbms-origin\\src\\.vuepress\\public";

        // 新产生的md文件新地址
        const string copyDir = "D:\\temp\\docs\\11111";

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Referer", "https://www.yuque.com/");
        client.DefaultRequestHeaders.Add(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36"
        );

        await DirectoriesHandler(rootPath, rootPath, imageDir, copyDir, copyImageDir, client);
    }

    /// <summary>
    /// 单个文件拷贝
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task CopySingleMdImage_ReturnOk()
    {
        // 原始md文件地址
        const string originMdPath = "D:\\Gitee\\kbms-origin\\src\\temp\\k3s.md";
        // 本地图片存放地址
        const string imageDir = "D:\\Gitee\\kbms-origin\\src\\temp\\";

        // 保存目录
        const string SaveMdPath = "D:\\Gitee\\kbms-origin\\src\\temp";

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("host", "img-blog.csdnimg.cn");
        //client.DefaultRequestHeaders.Add("Referer", "https://www.cnblogs.com/"); // https://www.cnblogs.com/   https://www.yuque.com/
        client.DefaultRequestHeaders.Add(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36"
        );
        await FileImageHanderAsync(originMdPath, SaveMdPath, imageDir, imageDir, imageDir, client);
    }

    /// <summary>
    /// 目录处理
    /// </summary>
    /// <param name="rootPath"></param>
    private async Task DirectoriesHandler(
        string rootPath,
        string originRootPath,
        string imageDir,
        string copyDir,
        string copyImageDir,
        HttpClient client
    )
    {
        foreach (var path in Directory.GetDirectories(rootPath))
        {
            if (path.EndsWith("vuepress"))
                continue;

            await DirectoriesHandler(path, originRootPath, imageDir, copyDir, copyImageDir, client);
        }

        foreach (var filePath in Directory.GetFiles(rootPath))
        {
            await FileImageHanderAsync(
                filePath,
                originRootPath,
                imageDir,
                copyDir,
                copyImageDir,
                client
            );
        }
    }

    /// <summary>
    /// 文件图片处理
    /// </summary>
    /// <param name="mdPath"></param>
    /// <param name="imageDir"></param>
    /// <param name="copyDir"></param>
    /// <param name="copyImageDir"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    private async Task FileImageHanderAsync(
        string mdPath,
        string originRootPath,
        string imageDir,
        string copyDir,
        string copyImageDir,
        HttpClient client
    )
    {
        var fileContent = File.ReadAllText(mdPath);
        // 图片正则
        const string pattern = @"!\[.+?\]\(([^)]+)\)";
        // 存放图片的文件夹名称
        const string imageFolder = "common";
        if (!Directory.Exists(Path.Combine(copyImageDir, imageFolder)))
        {
            Directory.CreateDirectory(Path.Combine(copyImageDir, imageFolder));
        }

        // 拷贝本地图片
        foreach (Match match in Regex.Matches(fileContent, pattern))
        {
            var imageUrl = match.Groups[1].Value;
            var imageName = string.Empty;
            if (imageUrl.StartsWith("https://"))
            {
                // 拷贝网络图片
                imageName = Path.GetFileName(imageUrl);
                var imageNameArray = imageName.Split("#");
                imageName = imageNameArray[0];
                var copyNewImagePath = Path.Combine(copyImageDir, imageFolder, imageName);
                if (!File.Exists(copyNewImagePath))
                {
                    var bytes = await client.GetByteArrayAsync(imageUrl);
                    await File.WriteAllBytesAsync(copyNewImagePath, bytes);
                }
            }
            else
            {
                if (imageUrl.StartsWith("data:image/gif"))
                {
                    fileContent = fileContent.Replace(match.ToString(), "");
                    continue;
                }
                // 拷贝本地图片
                imageName = Path.GetFileName(imageUrl.Substring(1));
                var imagePath = Path.Combine(imageDir, imageUrl.Substring(1));
                var copyNewImagePath = Path.Combine(copyDir, imageFolder, imageName);
                File.Copy(imagePath, copyNewImagePath, true);
            }
            var imageNewPath = $"/common/{imageName}";
            fileContent = fileContent.Replace(imageUrl, imageNewPath);
        }
        // 拷贝文件
        var resultPath = mdPath.Replace(originRootPath, copyDir);

        var fileDirectoryPath = Path.GetDirectoryName(resultPath);
        if (!Directory.Exists(fileDirectoryPath))
            Directory.CreateDirectory(fileDirectoryPath);

        File.WriteAllText(resultPath, fileContent);
        if (!File.Exists(resultPath))
        {
            Console.WriteLine("出错了");
        }
    }
}
