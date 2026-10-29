# Azrng.S3Store

这是一个基于 AWS S3 协议的对象存储封装库，支持兼容 S3 协议的各种对象存储服务，如 AWS S3、MinIO、阿里云 OSS、腾讯云 COS 等。

## 功能特性

- 支持兼容 S3 协议的各种对象存储服务
- 简化的 API 接口，易于使用
- 支持桶（Bucket）管理操作
- 支持文件上传、下载、删除、对象存在性与元数据查询等操作
- 支持下载直接落盘（临时文件原子替换）
- 支持预签名 URL 生成
- 支持桶策略配置
- 支持 HTTPS 连接
- 支持多框架：.NET 6.0 / 7.0 / 8.0 / 10.0

## 安装

通过 NuGet 安装:

```
Install-Package Azrng.S3Store
```

或通过 .NET CLI:

```
dotnet add package Azrng.S3Store
```

## 使用方法

### 注册服务

在 Program.cs 中注册服务：

```csharp
// 方式一：使用连接字符串
services.AddS3Store("http://ACCESS_KEY:SECRET_KEY@SERVER_ADDRESS:PORT");

// 示例：
services.AddS3Store("http://admin:password@localhost:9000");

// 方式二：使用配置委托
services.AddS3Store(options =>
{
    options.Url = "http://localhost:9000";
    options.AccessKey = "admin";
    options.SecretKey = "password";
});
```

### 在服务中使用

注入 [IStore]() 接口并在代码中使用：

```csharp
public class FileService
{
    private readonly IStore _store;

    public FileService(IStore store)
    {
        _store = store;
    }

    // 上传文件
    public async Task<bool> UploadFileAsync(string bucketName, string fileName, string filePath)
    {
        return await _store.UploadFileAsync(bucketName, fileName, filePath);
    }

    // 下载文件
    public async Task<Stream> DownloadFileAsync(string bucketName, string fileName)
    {
        return await _store.DownLoadFileAsync(bucketName, fileName);
    }

    // 获取文件预签名 URL
    public async Task<string> GetFileUrlAsync(string bucketName, string fileName)
    {
        return await _store.GetFileUrlAsync(bucketName, fileName);
    }

    // 删除文件
    public async Task<bool> DeleteFileAsync(string bucketName, string fileName)
    {
        return await _store.DeleteFileAsync(bucketName, fileName);
    }

    // 创建桶
    public async Task<bool> CreateBucketAsync(string bucketName)
    {
        return await _store.CreateBucketIfNotExistsAsync(bucketName);
    }

    // 删除桶
    public async Task<bool> DeleteBucketAsync(string bucketName)
    {
        return await _store.DeleteBucketAsync(bucketName);
    }
}
```

## API 参考

### 桶操作

- [CheckBucketExistAsync]()(string bucketName): 检查桶是否存在
- [CreateBucketIfNotExistsAsync]()(string bucketName): 创建桶（如果不存在）
- [DeleteBucketAsync]()(string bucketName): 删除桶
- [CheckBucketFolderExistAsync]()(string bucketName, string folderPath): 检查桶内文件夹是否存在

### 文件操作

- [UploadFileAsync]()(string bucketName, string fileName, string filePath, string? fileContentType = null): 上传文件（失败抛异常，HTTP 非 200 返回 false）
- [UploadFileAsync]()(string bucketName, string fileName, Stream steamData, string fileContentType): 上传文件流
- [DownLoadFileAsync]()(string bucketName, string fileName): 下载文件（对象不存在返回 null，真实故障抛异常）
- [DownloadToFileAsync]()(string bucketName, string fileName, string localPath, CancellationToken): 下载文件到本地路径（对象不存在返回 false，临时文件原子替换落盘）
- [DoesObjectExistAsync]()(string bucketName, string fileName): 检查对象是否存在（HEAD 查询）
- [GetObjectMetadataAsync]()(string bucketName, string fileName): 获取对象元数据 Size/ETag/ContentType/LastModified（不存在返回 null）
- [DeleteFileAsync]()(string bucketName, string fileName): 删除文件

### URL 和策略操作

- [GetFileUrlAsync]()(string bucketName, string fileName, int expiresInt = 7 * 24 * 3600): 获取文件预签名 URL
- [ConfigBucketPolicyAsync]()(string bucketName, BucketReadAccess access): 配置桶匿名访问策略（ReadOnly 仅公开读 / ReadWrite 公开可读写）
- [ConfigBucketReadAndWritePolicyAsync]()(string bucketName): 配置桶读写策略（任何人可读写删，公开下载场景请改用 ReadOnly）
- [GetBucketPolicyAsync]()(string bucketName): 获取桶策略

## 配置选项

### S3StoreConfig

[S3StoreConfig]() 类提供了以下配置选项：

- `Url`: S3 服务地址
- `AccessKey`: 访问密钥
- `SecretKey`: 密钥
- `AllowInsecureTls`: 使用 https 时是否接受不受信任的 TLS 证书，默认 `true`（保持历史行为）；生产环境使用受信证书时建议显式设为 `false` 启用标准证书校验

### GlobalConfig

[GlobalConfig]() 类提供了以下配置选项：

- `UseHttps`: 是否使用 HTTPS 连接

## 适用场景

- 需要与兼容 S3 协议的对象存储服务交互的应用
- 需要简化对象存储操作的项目
- 微服务架构中需要文件存储功能的服务
- 需要支持多种对象存储后端的应用

## 注意事项

1. 确保配置正确的访问密钥和密钥
2. 确保 S3 服务地址正确且可访问
3. 根据实际需求设置合适的过期时间
4. 在生产环境中建议使用 HTTPS 连接
5. 注意桶和文件名的命名规范
6. 大文件操作时注意内存使用情况
7. 使用 https 且证书不受信任时默认接受所有证书（`AllowInsecureTls = true` 保持历史行为），生产环境使用受信证书时建议显式关闭
8. `ConfigBucketReadAndWritePolicyAsync` 会让任何人可读取、覆盖、删除桶内对象，公开下载类桶请改用 `ConfigBucketPolicyAsync(bucketName, BucketReadAccess.ReadOnly)`

## 变更记录

### 1.1.0

- 新增 `DoesObjectExistAsync`：单对象 HEAD 存在性查询
- 新增 `GetObjectMetadataAsync`：返回 `S3ObjectMetadata`（Size/ETag/ContentType/LastModified），对象不存在返回 null
- 新增 `DownloadToFileAsync`：下载直接落盘，先写临时文件再原子替换，对象不存在返回 false
- 新增 `ConfigBucketPolicyAsync` 与 `BucketReadAccess`：支持仅公开读（ReadOnly）与公开可读写（ReadWrite）两档匿名策略
- `S3StoreConfig` 新增 `AllowInsecureTls`（默认 true 保持历史行为），显式 false 时启用标准 TLS 证书校验
- 行为修正：`DownLoadFileAsync` 对象不存在改为返回 null（此前会把所有失败统一抛为 `ArgumentNullException`）；网络/权限等真实故障改为原样抛出 SDK 异常（`AmazonServiceException`/`AmazonClientException`），调用方可据此区分「未命中」与「故障」
- 行为修正：`DeleteBucketAsync`/`DeleteFileAsync`/策略配置的成功判定从 HTTP 200 改为 2xx（S3 对 Delete/PutBucketPolicy 成功返回 204 NoContent，此前这些方法成功时也返回 false）
- 注意：`IStore` 接口新增成员，自行实现该接口的下游需要补齐实现

## 支持的对象存储服务

该库支持所有兼容 S3 协议的对象存储服务，包括但不限于：

- AWS S3
- MinIO
- 阿里云 OSS
- 腾讯云 COS
- 华为云 OBS
- 七牛云 Kodo