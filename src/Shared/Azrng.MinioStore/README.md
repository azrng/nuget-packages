# Azrng.MinioStore (Common.MinioStore)

这是一个专门针对 MinIO 对象存储服务的封装库，提供了简化的 API 来操作 MinIO 存储服务。

## 功能特性

- 专为 MinIO 对象存储服务设计
- 简化的 API 接口，易于使用
- 支持桶（Bucket）管理操作
- 支持文件上传、下载、删除等操作
- 支持预签名 URL 生成
- 支持桶策略配置
- 支持 HTTP 和 HTTPS 连接
- 支持多框架：.NET 6.0 / 7.0 / 8.0

## 安装

通过 NuGet 安装:

```
Install-Package Common.MinioStore
```

或通过 .NET CLI:

```
dotnet add package Common.MinioStore
```

## 使用方法

### 注册服务

在 Program.cs 中注册服务：

```csharp
// 方式一：使用连接字符串
services.AddMinioStore("http://ACCESS_KEY:SECRET_KEY@SERVER_ADDRESS:PORT");

// 示例：
services.AddMinioStore("http://admin:password@localhost:9000");

// 方式二：使用配置委托
services.AddMinioStore(options =>
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

## 配置选项

### S3StoreConfig

[S3StoreConfig]() 类提供了以下配置选项：

- `Url`: MinIO 服务地址
- `AccessKey`: 访问密钥
- `SecretKey`: 密钥

### GlobalConfig

[GlobalConfig]() 类提供了以下配置选项：

- `UseHttps`: 是否使用 HTTPS 连接

## 适用场景

- 需要与 MinIO 对象存储服务交互的应用
- 需要简化对象存储操作的项目
- 微服务架构中需要文件存储功能的服务
- 需要自建对象存储服务的场景

## 注意事项

1. 确保配置正确的访问密钥和密钥
2. 确保 MinIO 服务地址正确且可访问
3. 根据实际需求设置合适的过期时间
4. 在生产环境中建议使用 HTTPS 连接
5. 注意桶和文件名的命名规范
6. 大文件操作时注意内存使用情况