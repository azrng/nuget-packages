# Azrng.MinioStore 架构与原理说明

## 一、项目概述

`Azrng.MinioStore` 是一个针对 MinIO 对象存储服务的封装库，基于 S3 协议实现对象存储功能。该项目通过抽象的接口设计，简化了 MinIO 的使用，并提供了依赖注入集成能力。

### 核心特性
- 基于 S3 协议的对象存储操作
- 简化的 API 接口设计
- 支持桶（Bucket）管理
- 支持文件上传、下载、删除
- 预签名 URL 生成
- 桶策略配置（读写权限）
- 支持 HTTP/HTTPS 连接
- 多框架支持（.NET 6.0 / 7.0 / 8.0）

---

## 二、整体架构设计

项目采用**门面模式**和**依赖注入**设计，自下而上分为以下几层：

```
┌─────────────────────────────────────────────────────────────┐
│                      Application Layer                       │
│                   (使用 IStore 接口进行操作)                  │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                    Registration Layer                        │
│            ServiceCollectionExtensions                       │
│              (服务注册与依赖注入配置)                         │
└──────────────────────────────┬──────────────────────────────┘
                               │
                ┌──────────────┴──────────────┐
                ▼                             ▼
┌──────────────────────────┐    ┌──────────────────────────────┐
│    IMinioClient          │    │      S3Store (IStore)        │
│   (第三方 Minio 库)      │◄───┤    (核心存储实现类)          │
└──────────────────────────┘    └──────────────────────────────┘
                                        │
                                        ▼
                              ┌──────────────────────────────┐
                              │      MinIO Server            │
                              │   (对象存储服务器)           │
                              └──────────────────────────────┘
```

---

## 三、核心组件说明

### 3.1 存储接口 (IStore.cs)

**职责**: 定义对象存储的抽象契约，隐藏 MinIO 客户端复杂性

**核心方法**:

| 方法 | 功能描述 |
|------|----------|
| `CheckBucketExistAsync` | 检查桶是否存在 |
| `CheckBucketFolderExistAsync` | 检查桶内文件夹是否存在 |
| `CreateBucketIfNotExistsAsync` | 创建桶（如果不存在） |
| `GetFileUrlAsync` | 获取预签名文件 URL |
| `UploadFileAsync` | 上传文件（支持路径和流） |
| `DownLoadFileAsync` | 下载文件流 |
| `DeleteBucketAsync` | 删除桶 |
| `DeleteFileAsync` | 删除文件 |
| `ConfigBucketReadAndWritePolicyAsync` | 配置桶读写策略 |
| `GetBucketPolicyAsync` | 获取桶策略 |

**设计理念**:
- **异步优先**: 所有方法返回 `Task` 或 `Task<T>`，避免阻塞
- **可空类型**: `DownLoadFileAsync` 返回 `Stream?`，优雅处理不存在情况
- **重载支持**: `UploadFileAsync` 支持文件路径和 Stream 两种方式

---

### 3.2 存储实现 (S3Store.cs)

**职责**: 实现 `IStore` 接口，封装 MinIO 客户端操作

**依赖注入构造函数**:
```csharp
public S3Store(
    ILogger<S3Store> logger,      // 日志记录器
    IMinioClient minioClient,      // MinIO 客户端
    GlobalConfig globalConfig      // 全局配置
)
```

#### 3.2.1 桶操作实现原理

```
检查桶存在:
  BucketExistsArgs
    → IMinioClient.BucketExistsAsync()
    → MinIO Server
    → 返回 boolean

创建桶:
  MakeBucketArgs
    → IMinioClient.MakeBucketAsync()
    → MinIO Server 创建桶

删除桶:
  RemoveBucketArgs
    → IMinioClient.RemoveBucketAsync()
    → MinIO Server 删除桶
```

**关键代码** ([S3Store.cs:24-27](S3Store.cs#L24-L27)):
```csharp
public Task<bool> CheckBucketExistAsync(string bucketName)
{
    return _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
}
```

#### 3.2.2 文件操作实现原理

**文件上传流程**:
```
1. 调用 UploadFileAsync()
2. 自动创建桶 (CreateBucketIfNotExistsAsync)
3. 构造 PutObjectArgs:
   - WithFilePath() / WithStreamData()
   - WithContentType()
4. 调用 PutObjectAsync() 上传
5. 根据 Size > 0 判断成功
```

**文件删除流程**:
```
1. 检查桶存在
2. 构造 RemoveObjectArgs
3. 调用 RemoveObjectAsync()
4. 返回成功
```

**关键代码** ([S3Store.cs:91-101](S3Store.cs#L91-L101)):
```csharp
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
```

#### 3.2.3 预签名 URL 生成原理

**预签名 URL** 是带有临时访问凭证的 URL，无需认证即可访问文件。

**生成流程**:
```
1. 验证参数（bucketName、fileName 非空）
2. 检查桶是否存在
3. 列出对象确认文件存在 (ListObjectsArgs)
4. 生成签名 URL (PresignedGetObjectArgs):
   - WithBucket()
   - WithObject()
   - WithExpiry(过期时间，默认 7 天)
5. 返回 URL 格式:
   http://endpoint/bucket/object?expires=...&signature=...
```

**关键代码** ([S3Store.cs:52-76](S3Store.cs#L52-L76)):
```csharp
public async Task<string> GetFileUrlAsync(string bucketName, string fileName, int expiresInt = 7 * 24 * 3600)
{
    // 参数验证
    if (string.IsNullOrEmpty(bucketName))
        throw new ArgumentNullException(nameof(bucketName));
    if (string.IsNullOrEmpty(fileName))
        throw new ArgumentNullException(nameof(fileName));

    // 检查桶和文件存在
    var existBucket = await CheckBucketExistAsync(bucketName);
    if (!existBucket)
        return string.Empty;

    var response = await _minioClient.ListObjectsAsync(new ListObjectsArgs()
                                                       .WithBucket(bucketName)
                                                       .WithPrefix(fileName));

    if (response.Size == 0)
        return string.Empty;

    // 生成预签名 URL
    return await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                                                      .WithBucket(bucketName)
                                                      .WithExpiry(expiresInt)
                                                      .WithObject(fileName));
}
```

#### 3.2.4 桶策略配置原理

**桶策略 (Bucket Policy)** 是 AWS S3 风格的 JSON 策略，用于控制访问权限。

**策略模板** ([S3Store.cs:155-160](S3Store.cs#L155-L160)):
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {"AWS": ["*"]},
      "Action": [
        "s3:GetBucketLocation",
        "s3:ListBucket",
        "s3:ListBucketMultipartUploads"
      ],
      "Resource": ["arn:aws:s3:::{bucketName}"]
    },
    {
      "Effect": "Allow",
      "Principal": {"AWS": ["*"]},
      "Action": [
        "s3:GetObject",
        "s3:ListMultipartUploadParts",
        "s3:PutObject",
        "s3:AbortMultipartUpload",
        "s3:DeleteObject"
      ],
      "Resource": ["arn:aws:s3:::{bucketName}/*"]
    }
  ]
}
```

**配置流程**:
```
1. 检查桶存在
2. 替换策略模板中的 $bucketName$ 占位符
3. 调用 SetPolicyAsync() 应用策略
4. 返回成功
```

**策略说明**:
- 第一个 Statement: 允许列出桶内容
- 第二个 Statement: 允许对桶内对象进行读写删除操作
- `Principal: {"AWS": ["*"]}`: 允许所有用户访问（公开读写）

---

### 3.3 配置管理

#### 3.3.1 S3StoreConfig

**职责**: 管理 MinIO 连接配置

```csharp
public class S3StoreConfig
{
    [Required]
    public string Url { get; set; }          // MinIO 服务地址

    [Required]
    public string AccessKey { get; set; }    // 访问密钥

    [Required]
    public string SecretKey { get; set; }    // 密钥

    public void ParamVerify()                // 参数验证
    {
        if (string.IsNullOrWhiteSpace(AccessKey))
            throw new ArgumentNullException(nameof(AccessKey));
        if (string.IsNullOrWhiteSpace(SecretKey))
            throw new ArgumentNullException(nameof(SecretKey));
        if (string.IsNullOrWhiteSpace(Url))
            throw new ArgumentNullException(nameof(Url));
    }
}
```

#### 3.3.2 GlobalConfig

**职责**: 全局配置选项

```csharp
public class GlobalConfig
{
    public bool UseHttps { get; set; }  // 是否使用 HTTPS
}
```

---

### 3.4 服务注册扩展 (ServiceCollectionExtensions.cs)

**职责**: 提供依赖注入扩展方法，简化服务注册

#### 3.4.1 连接字符串方式注册

**格式**: `http(s)://ACCESS_KEY:SECRET_KEY@SERVER_ADDRESS:PORT`

**解析逻辑** ([ServiceCollectionExtensions.cs:21-25](ServiceCollectionExtensions.cs#L21-L25)):
```csharp
var match = Regex.Match(connectionString,
    @"^(https?://)([^:]+):([^@]+)@([^:]+:\d+)$");
// 示例: http://admin:password@localhost:9000
// Group[1]: http://
// Group[2]: admin (AccessKey)
// Group[3]: password (SecretKey)
// Group[4]: localhost:9000
```

**使用示例**:
```csharp
services.AddMinioStore("http://admin:password@localhost:9000");
```

#### 3.4.2 配置委托方式注册

**使用示例** ([ServiceCollectionExtensions.cs:48-67](ServiceCollectionExtensions.cs#L48-L67)):
```csharp
services.AddMinioStore(options =>
{
    options.Url = "http://localhost:9000";
    options.AccessKey = "admin";
    options.SecretKey = "password";
});
```

**注册流程**:
```
1. 解析配置（连接字符串 / 委托）
2. 调用 S3StoreConfig.ParamVerify() 验证参数
3. 检测 Url 判断 UseHttps
4. 注册 MinIO 客户端:
   services.AddMinio(configureClient => {
       configureClient.WithEndpoint(url)
                      .WithCredentials(accessKey, secretKey)
                      .WithSSL(useHttps)
                      .Build();
   });
5. 注册服务:
   - services.AddScoped<IStore, S3Store>()
   - services.AddSingleton<GlobalConfig>(...)
```

---

## 四、MinIO S3 协议交互原理

### 4.1 通信架构

```
┌─────────────┐      HTTP/HTTPS       ┌────────────────┐
│   Client    │◄─────────────────────►│   MinIO Server │
│  (S3Store)  │    S3 Protocol API    │   (Endpoint)   │
└─────────────┘                       └────────────────┘
```

**MinIO** 是 S3 兼容的对象存储服务，完全兼容 AWS S3 API。

### 4.2 认证机制

**签名认证**:
- 使用 Access Key 和 Secret Key
- 按照 AWS Signature V4 算法生成签名
- 每次请求携带签名信息

**预签名 URL**:
- 服务端生成包含签名的临时 URL
- 客户端可直接访问，无需额外认证
- URL 包含过期时间限制

### 4.3 异步流处理

MinIO 客户端使用 `System.Reactive.Linq` (Rx) 处理响应式流:

```csharp
var response = await _minioClient.ListObjectsAsync(new ListObjectsArgs()
                                                   .WithBucket(bucketName)
                                                   .WithPrefix(fileName));
// response 为 IObservable<Item> 类型的响应式流
// response.Size 获取对象数量
```

---

## 五、工作流程

### 5.1 文件上传流程

```
业务代码调用
IStore.UploadFileAsync(bucketName, fileName, filePath)
    ↓
S3Store.UploadFileAsync()
    ↓
检查桶是否存在 (CreateBucketIfNotExistsAsync)
    ↓
[桶不存在] → 创建桶
[桶存在] → 跳过
    ↓
构造 PutObjectArgs
    ↓
调用 MinioClient.PutObjectAsync()
    ↓
MinIO 客户端上传文件到服务器
    ↓
返回上传结果 (response.Size > 0)
```

### 5.2 文件下载流程

```
业务代码调用
IStore.DownLoadFileAsync(bucketName, fileName)
    ↓
S3Store.DownLoadFileAsync()
    ↓
检查桶存在
    ↓
[桶不存在] → 返回 null
[桶存在] → 继续
    ↓
抛出 NotImplementedException (功能待实现)
```

### 5.3 预签名 URL 生成流程

```
业务代码调用
IStore.GetFileUrlAsync(bucketName, fileName, expiresInt)
    ↓
参数验证（非空检查）
    ↓
检查桶存在
    ↓
[桶不存在] → 返回空字符串
[桶存在] → 继续
    ↓
列出对象确认文件存在
    ↓
[文件不存在] → 返回空字符串
[文件存在] → 继续
    ↓
构造 PresignedGetObjectArgs
    ↓
调用 MinioClient.PresignedGetObjectAsync()
    ↓
返回预签名 URL
```

### 5.4 桶策略配置流程

```
业务代码调用
IStore.ConfigBucketReadAndWritePolicyAsync(bucketName)
    ↓
检查桶存在
    ↓
[桶不存在] → 返回 false
[桶存在] → 继续
    ↓
替换策略模板中的 $bucketName$ 占位符
    ↓
构造 SetPolicyArgs
    ↓
调用 MinioClient.SetPolicyAsync()
    ↓
返回 true
```

---

## 六、设计模式

### 6.1 门面模式 (Facade Pattern)

`IStore` 接口作为门面，隐藏了 MinIO 客户端的复杂性:
- 简化了 API 调用
- 统一了操作接口
- 降低了耦合度

### 6.2 依赖注入模式 (Dependency Injection)

通过 `IServiceCollection` 管理依赖:
- `IStore` 注册为 Scoped 生命周期
- `IMinioClient` 由 Minio 库管理
- `GlobalConfig` 注册为 Singleton

### 6.3 策略模式 (Strategy Pattern)

桶策略配置支持不同访问策略:
- 读写策略
- 只读策略
- 自定义策略

### 6.4 工厂模式 (Factory Pattern)

`ServiceCollectionExtensions` 作为工厂:
- 解析连接字符串
- 创建配置对象
- 注册服务到 DI 容器

---

## 七、生命周期管理

| 组件 | 生命周期 | 说明 |
|------|----------|------|
| `IStore` / `S3Store` | Scoped | 每个 HTTP 请求一个实例 |
| `IMinioClient` | Singleton | 由 Minio 库管理，全局单例 |
| `GlobalConfig` | Singleton | 全局配置，应用生命周期单例 |

**设计考量**:
- MinIO 客户端为重量级对象，适合单例
- S3Store 含有 ILogger，适合 Scoped 以便请求追踪
- GlobalConfig 为不可变配置，适合单例

---

## 八、安全考虑

### 8.1 凭证管理

- AccessKey 和 SecretKey 通过配置传入
- 连接字符串方式应在加密存储后使用
- 生产环境建议使用配置中心或密钥管理服务

### 8.2 传输安全

- 支持 HTTPS (`WithSSL(useHttps)`)
- `GlobalConfig.UseHttps` 控制传输协议
- 生产环境强烈建议使用 HTTPS

### 8.3 访问控制

- 通过桶策略 (Bucket Policy) 控制访问权限
- 支持预签名 URL 临时访问
- 策略遵循 AWS IAM 策略格式

### 8.4 参数验证

- `S3StoreConfig.ParamVerify()` 确保必要参数非空
- 使用 `[Required]` 特性标记必填属性
- 方法入口进行参数非空检查

---

## 九、扩展性

### 9.1 兼容其他 S3 存储

由于使用标准 S3 协议，可兼容:
- **AWS S3**: 亚马逊云存储
- **阿里云 OSS**: 对象存储服务
- **腾讯云 COS**: 云对象存储
- **华为云 OBS**: 对象存储服务
- **其他**: 任何 S3 兼容的存储服务

### 9.2 自定义桶策略

通过修改 `ConfigBucketReadAndWritePolicyAsync` 中的 JSON 策略:
```csharp
// 只读策略示例
const string readOnlyPolicy = @"{
  ""Version"": ""2012-10-17"",
  ""Statement"": [{
    ""Effect"": ""Allow"",
    ""Principal"": {""AWS"": [""*""]},
    ""Action"": [""s3:GetObject""],
    ""Resource"": [""arn:aws:s3:::{bucket}/*""]
  }]
}";
```

### 9.3 日志集成

通过 `ILogger<S3Store>` 支持结构化日志:
```csharp
public S3Store(ILogger<S3Store> logger, ...)
{
    _logger = logger;
    // 可记录操作日志、错误日志等
}
```

---

## 十、技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| Minio | 6.0.2 | MinIO 官方 .NET 客户端 |
| Microsoft.Extensions.DependencyInjection | - | 依赖注入框架 |
| System.Reactive | - | 响应式扩展 (Rx) |
| .NET | 6.0 / 7.0 / 8.0 | 多框架支持 |

---

## 十一、文件组织结构

```
Azrng.MinioStore/
├── IStore.cs                      # 存储接口定义
├── S3Store.cs                     # MinIO 存储实现
├── S3StoreConfig.cs               # 连接配置类
├── GlobalConfig.cs                # 全局配置类
├── ServiceCollectionExtensions.cs # 依赖注入扩展
├── README.md                      # 使用说明
└── ARCHITECTURE.md                # 架构文档（本文件）
```

---

## 十二、已知限制

### 12.1 下载功能未实现

**位置**: [S3Store.cs:120](S3Store.cs#L120)

```csharp
public async Task<Stream?> DownLoadFileAsync(string bucketName, string fileName)
{
    // 原有实现被注释
    // throw new NotImplementedException();
}
```

**原因**: 待完善功能

**解决方案**: 可取消注释并启用原有实现

### 12.2 单例 MinIO 客户端

MinIO 客户端在注册时被设为单例，适合大多数场景但不适用于多租户场景。

**多租户场景**: 需要为每个租户创建独立的 MinIO 客户端实例

### 12.3 固定的策略模板

桶策略使用固定的 JSON 模板，灵活性有限。

**解决方案**: 可扩展接口支持自定义策略

---

## 十三、版本信息

| 项目 | 值 |
|------|-----|
| **当前版本** | 1.0.0-beta2 |
| **NuGet 包 ID** | Common.MinioStore |
| **项目文档** | https://azrng.github.io/nuget-docs |
| **支持框架** | .NET 6.0 / 7.0 / 8.0 |

---

## 十四、设计原则

1. **接口抽象**: 通过 `IStore` 接口隐藏实现细节
2. **依赖注入**: 所有组件通过 DI 容器管理
3. **异步优先**: 所有 I/O 操作使用异步方法
4. **参数验证**: 入口处进行参数非空验证
5. **灵活性**: 支持多种配置方式和注册方式
6. **可测试性**: 通过接口 Mock，便于单元测试