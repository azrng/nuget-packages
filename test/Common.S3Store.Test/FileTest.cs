using Azrng.S3Store;
using System.Text;

namespace Common.S3Store.Test;

/// <summary>
/// 文件单元测试
/// </summary>
public class FileTest
{
    private readonly IStore _store;

    public FileTest(IStore store)
    {
        _store = store;
    }

    /// <summary>
    /// 获取文件url 桶不存在返回空
    /// </summary>
    [Fact]
    public async Task GetFileUrl_BucketNotExist_ReturnEmpty()
    {
        var bucketName = Guid.NewGuid().ToString("N");
        var fileName = "1.png";
        var url = await _store.GetFileUrlAsync(bucketName, fileName);
        Assert.Empty(url);
    }

    /// <summary>
    /// 获取文件url  桶存在 文件不存在返回空
    /// </summary>
    [Fact]
    public async Task GetFileUrl_BucketExist_ReturnEmpty()
    {
        var bucketName = Guid.NewGuid().ToString("N");

        await _store.CreateBucketIfNotExistsAsync(bucketName);
        var fileName = "1.png";
        var url = await _store.GetFileUrlAsync(bucketName, fileName);
        Assert.Empty(url);

        await _store.DeleteBucketAsync(bucketName);
    }

    /// <summary>
    /// 获取文件url
    /// </summary>
    [Fact]
    public async Task GetFileUrl_Exist_ReturnUrl()
    {
        var bucketName = "getfileurlexist";
        var fileName = "33.txt";
        var bytes = Encoding.UTF8.GetBytes("测试文本");
        var memory = new MemoryStream(bytes);
        var result = await _store.UploadFileAsync(bucketName, fileName, memory, "text/plain");
        Assert.True(result);

        var url = await _store.GetFileUrlAsync(bucketName, fileName);
        Assert.NotEmpty(url);

        await _store.DeleteFileAsync(bucketName, fileName);
        await _store.DeleteBucketAsync(bucketName);
    }

    /// <summary>
    /// 上传图片返回ok
    /// </summary>
    [Fact]
    public async Task UploadJpg_ReturnOk()
    {
        var bucketName = Guid.NewGuid().ToString("N");
        var fileName = "33.jpg";

        var path = "D:\\temp\\images\\111.jpg";
        if (!File.Exists(path))
        {
            return;
        }

        var result = await _store.UploadFileAsync(bucketName, fileName, "D:\\temp\\images\\111.jpg");
        Assert.True(result);
        await _store.DeleteFileAsync(bucketName, fileName);
        await _store.DeleteBucketAsync(bucketName);
    }

    /// <summary>
    /// 桶的指定前缀是否存在
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task BucketPrefixVerify_ReturnOk()
    {
        var bucketName = Guid.NewGuid().ToString("N");
        var fileFolder = "TestFolder";

        var startNotExistPrefix = await _store.CheckBucketFolderExistAsync(bucketName, fileFolder);
        Assert.True(!startNotExistPrefix);

        var fileName = fileFolder + "/" + "33.txt";
        var bytes = Encoding.UTF8.GetBytes("测试文本");
        var memory = new MemoryStream(bytes);
        var result = await _store.UploadFileAsync(bucketName, fileName, memory, "text/plain");
        Assert.True(result);

        var exist = await _store.CheckBucketFolderExistAsync(bucketName, fileFolder);
        Assert.True(exist);

        var notExistInvalidPrefix = await _store.CheckBucketFolderExistAsync(bucketName, "Test");
        Assert.True(!notExistInvalidPrefix);

        await _store.DeleteFileAsync(bucketName, fileName);
        await _store.DeleteBucketAsync(bucketName);
    }
}