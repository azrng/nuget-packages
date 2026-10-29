using Azrng.S3Store;
using System.Text;

namespace Common.S3Store.Test;

/// <summary>
/// 对象级 API 单元测试（1.1.0 新增）。
/// 对象操作固定使用 zhangsantest 桶，key 带 test-{guid} 前缀且仅清理对象；
/// 桶策略测试使用临时桶，避免覆盖共享桶的策略。
/// </summary>
public class ObjectApiTest
{
    private const string BucketName = "zhangsantest";

    private readonly IStore _store;

    public ObjectApiTest(IStore store)
    {
        _store = store;
    }

    /// <summary>
    /// 对象存在性：上传前不存在，上传后存在，删除后不存在
    /// </summary>
    [Fact]
    public async Task DoesObjectExist_ReturnOk()
    {
        var fileName = $"test-{Guid.NewGuid():N}/repo/main/abc.zip";

        Assert.False(await _store.DoesObjectExistAsync(BucketName, fileName));

        var bytes = Encoding.UTF8.GetBytes("snapshot");
        Assert.True(await _store.UploadFileAsync(BucketName, fileName, new MemoryStream(bytes), "application/zip"));
        Assert.True(await _store.DoesObjectExistAsync(BucketName, fileName));

        await _store.DeleteFileAsync(BucketName, fileName);
        Assert.False(await _store.DoesObjectExistAsync(BucketName, fileName));
    }

    /// <summary>
    /// 对象存在性：桶不存在返回 false 且不抛异常
    /// </summary>
    [Fact]
    public async Task DoesObjectExist_BucketNotExist_ReturnFalse()
    {
        var bucketName = Guid.NewGuid().ToString("N");
        Assert.False(await _store.DoesObjectExistAsync(bucketName, "any.txt"));
    }

    /// <summary>
    /// 对象元数据：命中返回大小与 ETag，未命中返回 null
    /// </summary>
    [Fact]
    public async Task GetObjectMetadata_ReturnOk()
    {
        var fileName = $"test-{Guid.NewGuid():N}/meta.txt";

        Assert.Null(await _store.GetObjectMetadataAsync(BucketName, fileName));

        var bytes = Encoding.UTF8.GetBytes("元数据内容");
        await _store.UploadFileAsync(BucketName, fileName, new MemoryStream(bytes), "text/plain");
        try
        {
            var metadata = await _store.GetObjectMetadataAsync(BucketName, fileName);
            Assert.NotNull(metadata);
            Assert.Equal(bytes.Length, metadata!.Size);
            Assert.False(string.IsNullOrEmpty(metadata.ETag));
            Assert.NotEqual(default, metadata.LastModifiedUtc);
        }
        finally
        {
            await _store.DeleteFileAsync(BucketName, fileName);
        }
    }

    /// <summary>
    /// 下载文件流：对象不存在返回 null（不再抛异常）
    /// </summary>
    [Fact]
    public async Task DownLoadFile_ObjectNotExist_ReturnNull()
    {
        var stream = await _store.DownLoadFileAsync(BucketName, $"test-{Guid.NewGuid():N}/not-exist.zip");
        Assert.Null(stream);
    }

    /// <summary>
    /// 下载到本地：命中落盘成功，未命中返回 false 且不创建文件
    /// </summary>
    [Fact]
    public async Task DownloadToFile_ReturnOk()
    {
        var fileName = $"test-{Guid.NewGuid():N}/snap/main/sha.zip";
        var localPath = Path.Combine(Path.GetTempPath(), $"s3store-test-{Guid.NewGuid():N}.zip");
        try
        {
            // 未命中：返回 false 且不创建本地文件
            Assert.False(await _store.DownloadToFileAsync(BucketName, fileName, localPath));
            Assert.False(File.Exists(localPath));

            var bytes = Encoding.UTF8.GetBytes(new string('x', 1024));
            await _store.UploadFileAsync(BucketName, fileName, new MemoryStream(bytes), "application/zip");

            Assert.True(await _store.DownloadToFileAsync(BucketName, fileName, localPath));
            Assert.Equal(bytes, await File.ReadAllBytesAsync(localPath));
        }
        finally
        {
            if (File.Exists(localPath))
                File.Delete(localPath);
            await _store.DeleteFileAsync(BucketName, fileName);
        }
    }

    /// <summary>
    /// 只公开读策略：策略 JSON 不含匿名写入/删除动作（临时桶，避免覆盖共享桶策略）
    /// </summary>
    [Fact]
    public async Task ConfigBucketPolicy_ReadOnly_ReturnOk()
    {
        var bucketName = Guid.NewGuid().ToString("N");
        await _store.CreateBucketIfNotExistsAsync(bucketName);

        Assert.True(await _store.ConfigBucketPolicyAsync(bucketName, BucketReadAccess.ReadOnly));
        var policy = await _store.GetBucketPolicyAsync(bucketName);
        Assert.Contains("s3:GetObject", policy);
        Assert.DoesNotContain("s3:PutObject", policy);
        Assert.DoesNotContain("s3:DeleteObject", policy);

        await _store.DeleteBucketAsync(bucketName);
    }

    /// <summary>
    /// 读写策略：等价于 ReadWrite 级别，含匿名写入动作（临时桶）
    /// </summary>
    [Fact]
    public async Task ConfigBucketPolicy_ReadWrite_ReturnOk()
    {
        var bucketName = Guid.NewGuid().ToString("N");
        await _store.CreateBucketIfNotExistsAsync(bucketName);

        Assert.True(await _store.ConfigBucketPolicyAsync(bucketName, BucketReadAccess.ReadWrite));
        var policy = await _store.GetBucketPolicyAsync(bucketName);
        Assert.Contains("s3:PutObject", policy);
        Assert.Contains("s3:DeleteObject", policy);

        await _store.DeleteBucketAsync(bucketName);
    }
}
