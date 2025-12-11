using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using System.Reactive.Linq;

namespace Azrng.MinioStore;

/// <summary>
/// minio 存储实现
/// </summary>
public class S3Store : IStore
{
    private readonly IMinioClient _minioClient;
    private readonly ILogger<S3Store> _logger;
    private readonly GlobalConfig _globalConfig;

    public S3Store(ILogger<S3Store> logger, IMinioClient minioClient, GlobalConfig globalConfig)
    {
        _logger = logger;
        _minioClient = minioClient;
        _globalConfig = globalConfig;
    }

    public Task<bool> CheckBucketExistAsync(string bucketName)
    {
        return _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
    }

    public async Task<bool> CheckBucketFolderExistAsync(string bucketName, string folderPath)
    {
        var exist = await CheckBucketExistAsync(bucketName);
        if (!exist)
            return false;

        // 要检查的文件夹路径。请注意末尾的斜杠是必需的，以确保只匹配文件夹而不会误判同名的文件。
        var response = await _minioClient.ListObjectsAsync(new ListObjectsArgs()
                                                           .WithBucket(bucketName)
                                                           .WithPrefix(folderPath + "/"));
        return response.Size != 0;
    }

    public async Task<bool> CreateBucketIfNotExistsAsync(string bucketName)
    {
        var exist = await CheckBucketExistAsync(bucketName);
        if (exist)
            return true;

        await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName)).ConfigureAwait(false);
        return true;
    }

    public async Task<string> GetFileUrlAsync(string bucketName, string fileName, int expiresInt = 7 * 24 * 3600)
    {
        if (string.IsNullOrEmpty(bucketName))
            throw new ArgumentNullException(nameof(bucketName));
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentNullException(nameof(fileName));

        var existBucket = await CheckBucketExistAsync(bucketName);
        if (!existBucket)
            return string.Empty;

        var response = await _minioClient.ListObjectsAsync(new ListObjectsArgs()
                                                           .WithBucket(bucketName)
                                                           .WithPrefix(fileName));

        if (response.Size == 0)
        {
            return string.Empty;
        }

        return await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                                                          .WithBucket(bucketName)
                                                          .WithExpiry(expiresInt)
                                                          .WithObject(fileName));
    }

    public async Task<bool> UploadFileAsync(string bucketName, string fileName, string filePath,
                                            string? fileContentType = null)
    {
        await CreateBucketIfNotExistsAsync(bucketName).ConfigureAwait(false);

        var response = await _minioClient.PutObjectAsync(new PutObjectArgs()
                                                         .WithBucket(bucketName)
                                                         .WithFileName(filePath + fileName)
                                                         .WithContentType(fileContentType))
                                         .ConfigureAwait(false);
        return response.Size > 0;
    }

    public async Task<bool> UploadFileAsync(string bucketName, string fileName, Stream steamData,
                                            string fileContentType)
    {
        await CreateBucketIfNotExistsAsync(bucketName).ConfigureAwait(false);
        var response = await _minioClient.PutObjectAsync(new PutObjectArgs()
                                                         .WithBucket(bucketName)
                                                         .WithStreamData(steamData)
                                                         .WithContentType(fileContentType))
                                         .ConfigureAwait(false);
        return response.Size > 0;
    }

    public async Task<Stream?> DownLoadFileAsync(string bucketName, string fileName)
    {
        var exist = await CheckBucketExistAsync(bucketName).ConfigureAwait(false);
        if (!exist)
            return null;

        // try
        // {
        //     var response = await _minioClient.GetObjectAsync(bucketName, fileName).ConfigureAwait(false);
        //     return response.ResponseStream;
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, $"下载文件报错 message:{ex.Message} stackTrace:{ex.StackTrace}");
        //     throw new ArgumentNullException("文件未找到");
        // }

        throw new NotImplementedException();
    }

    public async Task<bool> DeleteBucketAsync(string bucketName)
    {
        var exist = await CheckBucketExistAsync(bucketName).ConfigureAwait(false);
        if (!exist)
            return true;

        await _minioClient.RemoveBucketAsync(new RemoveBucketArgs()
                              .WithBucket(bucketName))
                          .ConfigureAwait(false);
        return true;
    }

    public async Task<bool> DeleteFileAsync(string bucketName, string fileName)
    {
        var exist = await CheckBucketExistAsync(bucketName).ConfigureAwait(false);
        if (!exist)
            return true;

        await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                                             .WithBucket(bucketName)
                                             .WithObject(fileName))
                          .ConfigureAwait(false);
        return true;
    }

    public async Task<bool> ConfigBucketReadAndWritePolicyAsync(string bucketName)
    {
        var bucketExist = await CheckBucketExistAsync(bucketName);
        if (!bucketExist)
            return false;

        // 设置桶读写的策略
        const string policyTemplateJson =
            "{\"Version\":\"2012-10-17\",\"Statement\":[{\"Effect\":\"Allow\",\"Principal\":{\"AWS\":[\"*\"]},\"Action\":" +
            "[\"s3:GetBucketLocation\",\"s3:ListBucket\",\"s3:ListBucketMultipartUploads\"],\"Resource\":[\"arn:aws:s3:::$bucketName$\"]}," +
            "{\"Effect\":\"Allow\",\"Principal\":{\"AWS\":[\"*\"]},\"Action\":" +
            "[\"s3:GetObject\",\"s3:ListMultipartUploadParts\",\"s3:PutObject\",\"s3:AbortMultipartUpload\",\"s3:DeleteObject\"],\"Resource\":[\"arn:aws:s3:::$bucketName$/*\"]}]}";
        var policyJson = policyTemplateJson.Replace("$bucketName$", bucketName);

        await _minioClient.SetPolicyAsync(new SetPolicyArgs()
                                          .WithBucket(bucketName)
                                          .WithPolicy(policyJson))
                          .ConfigureAwait(false);
        return true;
    }

    public async Task<string> GetBucketPolicyAsync(string bucketName)
    {
        var bucketExist = await CheckBucketExistAsync(bucketName);
        if (!bucketExist)
            return string.Empty;

        return await _minioClient.GetPolicyAsync(new GetPolicyArgs()
                                     .WithBucket(bucketName))
                                 .ConfigureAwait(false);
    }
}