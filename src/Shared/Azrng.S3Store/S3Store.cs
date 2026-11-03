using Amazon.S3;
using Amazon.Runtime;
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

    /// <summary>
    /// 仅公开读：匿名可 GetObject / 列对象 / 查位置，不可写入或删除
    /// </summary>
    private const string ReadOnlyPolicyTemplateJson =
        "{\"Version\":\"2012-10-17\",\"Statement\":[{\"Effect\":\"Allow\",\"Principal\":{\"AWS\":[\"*\"]},\"Action\":" +
        "[\"s3:GetBucketLocation\",\"s3:ListBucket\"],\"Resource\":[\"arn:aws:s3:::$bucketName$\"]}," +
        "{\"Effect\":\"Allow\",\"Principal\":{\"AWS\":[\"*\"]},\"Action\":" +
        "[\"s3:GetObject\"],\"Resource\":[\"arn:aws:s3:::$bucketName$/*\"]}]}";

    /// <summary>
    /// 公开可读写：匿名可 GetObject / PutObject / DeleteObject（任何人可篡改对象，谨慎使用）
    /// </summary>
    private const string ReadWritePolicyTemplateJson =
        "{\"Version\":\"2012-10-17\",\"Statement\":[{\"Effect\":\"Allow\",\"Principal\":{\"AWS\":[\"*\"]},\"Action\":" +
        "[\"s3:GetBucketLocation\",\"s3:ListBucket\",\"s3:ListBucketMultipartUploads\"],\"Resource\":[\"arn:aws:s3:::$bucketName$\"]}," +
        "{\"Effect\":\"Allow\",\"Principal\":{\"AWS\":[\"*\"]},\"Action\":" +
        "[\"s3:GetObject\",\"s3:ListMultipartUploadParts\",\"s3:PutObject\",\"s3:AbortMultipartUpload\",\"s3:DeleteObject\"],\"Resource\":[\"arn:aws:s3:::$bucketName$/*\"]}]}";

    public S3Store(ILogger<S3Store> logger, IAmazonS3 s3Client, GlobalConfig globalConfig)
    {
        _logger = logger;
        _s3Client = s3Client;
        _globalConfig = globalConfig;
    }

    public Task<bool> CheckBucketExistAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        return _globalConfig.UseHttps
            ? Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName)
            : _s3Client.DoesS3BucketExistAsync(bucketName);
    }

    public async Task<bool> CheckBucketFolderExistAsync(string bucketName, string folderPath,
                                                        CancellationToken cancellationToken = default)
    {
        var exist = await CheckBucketExistAsync(bucketName, cancellationToken);
        if (!exist)
            return false;

        // 要检查的文件夹路径。请注意末尾的斜杠是必需的，以确保只匹配文件夹而不会误判同名的文件。
        var request = new ListObjectsRequest { BucketName = bucketName, Prefix = folderPath + "/", MaxKeys = 1 };
        var response = await _s3Client.ListObjectsAsync(request, cancellationToken).ConfigureAwait(false);
        return response.S3Objects.Count != 0;
    }

    public async Task<bool> CreateBucketIfNotExistsAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        var exist = await CheckBucketExistAsync(bucketName, cancellationToken);
        if (exist)
            return true;

        // SDK 的 EnsureBucketExistsAsync 无 ct 重载，桶创建是低频管理操作，允许其不可取消
        await _s3Client.EnsureBucketExistsAsync(bucketName).ConfigureAwait(false);
        return true;
    }

    public async Task<string> GetFileUrlAsync(string bucketName, string fileName, int expiresInt = 7 * 24 * 3600,
                                              string? responseContentType = null, string? responseContentDisposition = null,
                                              CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(bucketName))
            throw new ArgumentNullException(nameof(bucketName));
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentNullException(nameof(fileName));

        // HEAD 精确判存在：ListObjects 前缀匹配会命中同前缀兄弟键（如 a.txt.bak 命中 a.txt），
        // 对不存在的 key 签出 404 的 URL；HEAD 同时比列举少一次往返
        if (!await DoesObjectExistAsync(bucketName, fileName, cancellationToken))
            return string.Empty;

        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = fileName,
            Expires = DateTime.UtcNow.AddSeconds(expiresInt),
            Protocol = _globalConfig.UseHttps ? Protocol.HTTPS : Protocol.HTTP
        };

        // ResponseHeaderOverrides 经签名参数下发，覆盖浏览器下载行为（inline 预览 / attachment 下载）
        if (!string.IsNullOrEmpty(responseContentType))
            request.ResponseHeaderOverrides.ContentType = responseContentType;
        if (!string.IsNullOrEmpty(responseContentDisposition))
            request.ResponseHeaderOverrides.ContentDisposition = responseContentDisposition;

        return _s3Client.GetPreSignedURL(request);
    }

    public async Task<bool> UploadFileAsync(string bucketName, string fileName, string filePath,
                                            string? fileContentType = null,
                                            IDictionary<string, string>? userMetadata = null,
                                            CancellationToken cancellationToken = default)
    {
        if (_globalConfig.AutoCreateBucket)
            await CreateBucketIfNotExistsAsync(bucketName, cancellationToken).ConfigureAwait(false);

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            ContentType = fileContentType,
            FilePath = filePath,
            Key = fileName
        };
        FillMetadata(request, userMetadata);

        var response = await _s3Client.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);
        return IsSuccess(response?.HttpStatusCode);
    }

    public async Task<bool> UploadFileAsync(string bucketName, string fileName, Stream steamData,
                                            string fileContentType,
                                            IDictionary<string, string>? userMetadata = null,
                                            CancellationToken cancellationToken = default)
    {
        if (_globalConfig.AutoCreateBucket)
            await CreateBucketIfNotExistsAsync(bucketName, cancellationToken).ConfigureAwait(false);

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            ContentType = fileContentType,
            InputStream = steamData,
            Key = fileName
        };
        FillMetadata(request, userMetadata);

        var response = await _s3Client.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);
        return IsSuccess(response?.HttpStatusCode);
    }

    private static void FillMetadata(PutObjectRequest request, IDictionary<string, string>? userMetadata)
    {
        if (userMetadata == null)
            return;

        // S3 会给键名自动加 x-amz-meta- 前缀；SDK 存储时统一转小写，读回时键为小写
        foreach (var (key, value) in userMetadata)
            request.Metadata[key] = value;
    }

    public async Task<Stream?> DownLoadFileAsync(string bucketName, string fileName,
                                                 CancellationToken cancellationToken = default)
    {
        var exist = await CheckBucketExistAsync(bucketName, cancellationToken).ConfigureAwait(false);
        if (!exist)
            return null;
        try
        {
            var response = await _s3Client.GetObjectAsync(bucketName, fileName, cancellationToken).ConfigureAwait(false);
            return response.ResponseStream;
        }
        catch (AmazonServiceException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // 对象不存在属于正常未命中，返回 null；其余异常（网络/权限/桶故障）原样传播
            _logger.LogWarning("对象不存在 bucket:{BucketName} key:{FileName}", bucketName, fileName);
            return null;
        }
    }

    public async Task<bool> DownloadToFileAsync(string bucketName, string fileName, string localPath,
                                                CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(localPath))
            throw new ArgumentNullException(nameof(localPath));

        var exist = await CheckBucketExistAsync(bucketName, cancellationToken).ConfigureAwait(false);
        if (!exist)
            return false;

        GetObjectResponse response;
        try
        {
            response = await _s3Client.GetObjectAsync(bucketName, fileName, cancellationToken).ConfigureAwait(false);
        }
        catch (AmazonServiceException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("对象不存在 bucket:{BucketName} key:{FileName}", bucketName, fileName);
            return false;
        }

        using (response)
        {
            // 先写临时文件再原子替换，避免下载中断在 localPath 留下半成品
            var tempPath = localPath + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                var directory = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                await using (var fileStream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    await response.ResponseStream.CopyToAsync(fileStream, 81920, cancellationToken).ConfigureAwait(false);
                }

                File.Move(tempPath, localPath, true);
                return true;
            }
            catch
            {
                TryDeleteFile(tempPath);
                throw;
            }
        }
    }

    public async Task<bool> DoesObjectExistAsync(string bucketName, string fileName,
                                                 CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(bucketName))
            throw new ArgumentNullException(nameof(bucketName));
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentNullException(nameof(fileName));

        try
        {
            await _s3Client.GetObjectMetadataAsync(bucketName, fileName, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (AmazonServiceException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // 桶或对象不存在均视为不存在；其余故障原样传播
            return false;
        }
    }

    public async Task<S3ObjectMetadata?> GetObjectMetadataAsync(string bucketName, string fileName,
                                                                CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(bucketName))
            throw new ArgumentNullException(nameof(bucketName));
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentNullException(nameof(fileName));

        try
        {
            var response = await _s3Client.GetObjectMetadataAsync(bucketName, fileName, cancellationToken).ConfigureAwait(false);
            return new S3ObjectMetadata
            {
                Size = response.ContentLength,
                ETag = response.ETag,
                // HEAD 响应无独立 ContentType/LastModifiedUtc 属性：内容类型取响应头，最后修改时间为本地时区需转 UTC
                ContentType = GetHeaderSafe(response, "Content-Type"),
                LastModifiedUtc = response.LastModified == default ? default : response.LastModified.ToUniversalTime(),
                // UserMetadata 键已由 S3 自动去掉 x-amz-meta- 前缀并统一小写
                UserMetadata = ReadUserMetadataSafe(response)
            };
        }
        catch (AmazonServiceException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    private static string? GetHeaderSafe(GetObjectMetadataResponse response, string headerName)
    {
        try
        {
            return response.Headers?[headerName];
        }
        catch (ArgumentException)
        {
            // 部分 HeadersCollection 实现对缺失 key 抛异常而非返回 null
            return null;
        }
    }

    /// <summary>
    /// 从 HEAD 响应读取用户元数据；response.Metadata 含 Content-Type 等常规头，需按 x-amz-meta- 前缀过滤
    /// </summary>
    private static IReadOnlyDictionary<string, string>? ReadUserMetadataSafe(GetObjectMetadataResponse response)
    {
        var metadata = response.Metadata;
        if (metadata == null || metadata.Count == 0)
            return null;

        Dictionary<string, string>? result = null;
        foreach (var key in metadata.Keys)
        {
            // 用户元数据键统一小写（S3 存储时已强制小写），常规头无此前缀
            if (string.IsNullOrEmpty(key) || !key.StartsWith("x-amz-meta-", StringComparison.OrdinalIgnoreCase))
                continue;

            result ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            result[key["x-amz-meta-".Length..]] = metadata[key] ?? string.Empty;
        }

        return result;
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException)
        {
            // 清理临时文件失败不影响主流程异常传播
        }
    }

    /// <summary>
    /// S3 成功状态按 2xx 判定：Delete/PutBucketPolicy 等操作成功返回 204 NoContent 而非 200
    /// </summary>
    private static bool IsSuccess(HttpStatusCode? statusCode)
    {
        return statusCode.HasValue && (int)statusCode.Value is >= 200 and < 300;
    }

    public async Task<bool> DeleteBucketAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        var exist = await CheckBucketExistAsync(bucketName, cancellationToken).ConfigureAwait(false);
        if (!exist)
            return true;

        var response = await _s3Client.DeleteBucketAsync(bucketName, cancellationToken).ConfigureAwait(false);
        return IsSuccess(response?.HttpStatusCode);
    }

    public async Task<bool> DeleteFileAsync(string bucketName, string fileName,
                                            CancellationToken cancellationToken = default)
    {
        var exist = await CheckBucketExistAsync(bucketName, cancellationToken).ConfigureAwait(false);
        if (!exist)
            return true;

        var response = await _s3Client.DeleteObjectAsync(bucketName, fileName, cancellationToken).ConfigureAwait(false);
        return IsSuccess(response?.HttpStatusCode);
    }

    public Task<bool> ConfigBucketReadAndWritePolicyAsync(string bucketName,
                                                          CancellationToken cancellationToken = default)
    {
        return ConfigBucketPolicyAsync(bucketName, BucketReadAccess.ReadWrite, cancellationToken);
    }

    public async Task<bool> ConfigBucketPolicyAsync(string bucketName, BucketReadAccess access,
                                                    CancellationToken cancellationToken = default)
    {
        var bucketExist = await CheckBucketExistAsync(bucketName, cancellationToken);
        if (!bucketExist)
            return false;

        // ReadOnly 只放开 GetObject/ListBucket，ReadWrite 额外允许匿名写入与删除
        var policyJson = access == BucketReadAccess.ReadOnly
            ? ReadOnlyPolicyTemplateJson.Replace("$bucketName$", bucketName)
            : ReadWritePolicyTemplateJson.Replace("$bucketName$", bucketName);

        var request = new PutBucketPolicyRequest { BucketName = bucketName, Policy = policyJson };

        var response = await _s3Client.PutBucketPolicyAsync(request, cancellationToken).ConfigureAwait(false);
        return IsSuccess(response?.HttpStatusCode);
    }

    public async Task<string> GetBucketPolicyAsync(string bucketName,
                                                   CancellationToken cancellationToken = default)
    {
        var bucketExist = await CheckBucketExistAsync(bucketName, cancellationToken);
        if (!bucketExist)
            return string.Empty;

        var response = await _s3Client.GetBucketPolicyAsync(bucketName, cancellationToken).ConfigureAwait(false);
        return response.Policy;
    }
}
