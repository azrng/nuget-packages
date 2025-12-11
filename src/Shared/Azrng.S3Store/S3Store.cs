using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Azrng.S3Store;

/// <summary>
/// minio 存储实现
/// </summary>
public class S3Store : IStore
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Store> _logger;
    private readonly GlobalConfig _globalConfig;

    public S3Store(ILogger<S3Store> logger, IAmazonS3 s3Client, GlobalConfig globalConfig)
    {
        _logger = logger;
        _s3Client = s3Client;
        _globalConfig = globalConfig;
    }

    public Task<bool> CheckBucketExistAsync(string bucketName)
    {
        return _globalConfig.UseHttps
            ? Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName)
            : _s3Client.DoesS3BucketExistAsync(bucketName);
    }

    public async Task<bool> CheckBucketFolderExistAsync(string bucketName, string folderPath)
    {
        var exist = await CheckBucketExistAsync(bucketName);
        if (!exist)
            return false;

        // 要检查的文件夹路径。请注意末尾的斜杠是必需的，以确保只匹配文件夹而不会误判同名的文件。
        var request = new ListObjectsRequest { BucketName = bucketName, Prefix = folderPath + "/", MaxKeys = 1 };
        var response = await _s3Client.ListObjectsAsync(request).ConfigureAwait(false);
        return response.S3Objects.Count != 0;
    }

    public async Task<bool> CreateBucketIfNotExistsAsync(string bucketName)
    {
        var exist = await CheckBucketExistAsync(bucketName);
        if (exist)
            return true;

        await _s3Client.EnsureBucketExistsAsync(bucketName).ConfigureAwait(false);
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

        var request = new ListObjectsRequest { BucketName = bucketName, Prefix = fileName, MaxKeys = 1 };
        var response = await _s3Client.ListObjectsAsync(request).ConfigureAwait(false);
        if (response.S3Objects.Count == 0)
        {
            return string.Empty;
        }

        return _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                                         {
                                             BucketName = bucketName,
                                             Key = fileName,
                                             Expires = DateTime.Now.AddSeconds(expiresInt),
                                             Protocol = _globalConfig.UseHttps ? Protocol.HTTPS : Protocol.HTTP
                                         });
    }

    public async Task<bool> UploadFileAsync(string bucketName, string fileName, string filePath,
                                            string? fileContentType = null)
    {
        await CreateBucketIfNotExistsAsync(bucketName).ConfigureAwait(false);

        var response = await _s3Client.PutObjectAsync(new PutObjectRequest
                                                      {
                                                          BucketName = bucketName,
                                                          ContentType = fileContentType,
                                                          FilePath = filePath,
                                                          Key = fileName
                                                      })
                                      .ConfigureAwait(false);
        return response?.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> UploadFileAsync(string bucketName, string fileName, Stream steamData,
                                            string fileContentType)
    {
        await CreateBucketIfNotExistsAsync(bucketName).ConfigureAwait(false);
        var response = await _s3Client.PutObjectAsync(
                                          new PutObjectRequest
                                          {
                                              BucketName = bucketName,
                                              ContentType = fileContentType,
                                              InputStream = steamData,
                                              Key = fileName
                                          })
                                      .ConfigureAwait(false);
        return response?.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<Stream?> DownLoadFileAsync(string bucketName, string fileName)
    {
        var exist = await CheckBucketExistAsync(bucketName).ConfigureAwait(false);
        if (!exist)
            return null;
        try
        {
            var response = await _s3Client.GetObjectAsync(bucketName, fileName).ConfigureAwait(false);
            return response.ResponseStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"下载文件报错 message:{ex.Message} stackTrace:{ex.StackTrace}");
            throw new ArgumentNullException("文件未找到");
        }
    }

    public async Task<bool> DeleteBucketAsync(string bucketName)
    {
        var exist = await CheckBucketExistAsync(bucketName).ConfigureAwait(false);
        if (!exist)
            return true;

        var response = await _s3Client.DeleteBucketAsync(bucketName).ConfigureAwait(false);
        return response?.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> DeleteFileAsync(string bucketName, string fileName)
    {
        var exist = await CheckBucketExistAsync(bucketName).ConfigureAwait(false);
        if (!exist)
            return true;

        var response = await _s3Client.DeleteObjectAsync(bucketName, fileName).ConfigureAwait(false);
        return response?.HttpStatusCode == HttpStatusCode.OK;
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

        var request = new PutBucketPolicyRequest { BucketName = bucketName, Policy = policyJson };

        var response = await _s3Client.PutBucketPolicyAsync(request).ConfigureAwait(false);
        return response?.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<string> GetBucketPolicyAsync(string bucketName)
    {
        var bucketExist = await CheckBucketExistAsync(bucketName);
        if (!bucketExist)
            return string.Empty;

        var response = await _s3Client.GetBucketPolicyAsync(bucketName).ConfigureAwait(false);
        return response.Policy;
    }
}