# Common.YuQueSdk

> 专为 .NET 设计的语雀 SDK，提供完整的 API 封装和依赖注入支持

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-0.0.1-beta2-green.svg)](https://www.nuget.org/packages/Common.YuQueSdk)

## 📖 项目简介

`Common.YuQueSdk` 是一个功能完善的语雀（YuQue）API SDK，为 .NET 应用程序提供便捷的语雀服务集成能力。本项目基于 Refit 构建，提供强类型的 API 调用体验。

### 核心功能

- ✅ **用户管理** - 获取用户信息、用户组列表
- ✅ **仓库操作** - 获取用户仓库列表
- ✅ **文档管理** - 获取文档详情、保存文档
- ✅ **主题树** - 获取文档主题树结构
- ✅ **依赖注入** - 原生支持 Microsoft.Extensions.DependencyInjection
- ✅ **类型安全** - 基于 Refit 的强类型 API

### 解决的问题

- 简化语雀 API 集成复杂度
- 提供类型安全的 API 调用
- 支持依赖注入的现代化开发模式
- 封装认证和请求处理逻辑

## 🛠️ 技术栈

| 组件 | 版本 | 说明 |
|------|------|------|
| Refit.HttpClientFactory | 6.3.2 | 类型安全的 REST 库 |
| Refit.Newtonsoft.Json | 6.3.2 | JSON 序列化 |
| Microsoft.Extensions.* | 6.0.0+ | 依赖注入和配置 |
| .NET | 6.0+ | 目标框架 |

## 🚀 快速开始

### 环境要求

- .NET 6.0 或更高版本
- 语雀账号和 Personal Access Token

### 安装

```bash
dotnet add package Common.YuQueSdk
```

### 配置与使用

#### 1. 配置 appsettings.json

```json
{
  "YuQue": {
    "Token": "your_yuque_personal_access_token",
    "BaseUrl": "https://www.yuque.com/api/v2"
  }
}
```

#### 2. 注册服务

```csharp
// Program.cs 或 Startup.cs
services.AddYuQueSdk(Configuration.GetSection("YuQue"));
```

#### 3. 注入并使用

```csharp
public class DocumentService
{
    private readonly IYuqueHelper _yuqueHelper;

    public DocumentService(IYuqueHelper yuqueHelper)
    {
        _yuqueHelper = yuqueHelper;
    }

    public async Task<Document> GetDocumentAsync(string docId)
    {
        // 获取当前用户信息
        var user = await _yuqueHelper.GetUserAsync();

        // 获取文档详情
        var docDetail = await _yuqueHelper.GetDocDetailsAsync(user.Id, docId);

        return docDetail.Data;
    }
}
```

## 📚 API 文档

### 核心接口

#### IYuqueHelper - 主要 API

```csharp
public interface IYuqueHelper
{
    // 用户操作
    Task<YuqueResult<GetUserResult>> GetUserAsync();

    // 仓库操作
    Task<YuqueResult<List<GetUserRepositoryResult>>> GetUserRepositoriesAsync();

    // 文档操作
    Task<YuqueResult<GetDocsDetailsResponse>> GetDocDetailsAsync(string login, string namespaceId, string id);
    Task<YuqueResult<SaveDocResult>> SaveDocAsync(string login, string namespaceId, string id, SaveDocRequest request);

    // 其他操作...
}
```

#### IYuqueExtensionHelper - 扩展 API

```csharp
public interface IYuqueExtensionHelper
{
    // 获取主题树
    Task<TopicTree> GetTopicTreeAsync(string repoId);

    // 其他扩展方法...
}
```

### 数据模型

#### YuQueConfig - 配置模型

```csharp
public class YuQueConfig
{
    public string Token { get; set; }
    public string BaseUrl { get; set; }
}
```

#### 文档相关模型

- `GetDocsDetailsResponse` - 文档详情响应
- `GetSampleDocsDetailsResponse` - 示例文档详情
- `SaveDocResult` - 保存文档结果
- `DocDetail` - 文档详情
- `DocDetailData` - 文档数据

## 💡 典型应用场景

### 1. 同步语雀文档到本地

```csharp
public async Task SyncDocumentsAsync()
{
    var user = await _yuqueHelper.GetUserAsync();
    var repos = await _yuqueHelper.GetUserRepositoriesAsync();

    foreach (var repo in repos.Data)
    {
        var docs = await _yuqueExtensionHelper.GetTopicTreeAsync(repo.Id);
        // 处理文档...
    }
}
```

### 2. 自动更新文档

```csharp
public async Task UpdateDocumentAsync(string docId, string newContent)
{
    var user = await _yuqueHelper.GetUserAsync();

    var request = new SaveDocRequest
    {
        Title = "更新的标题",
        Slug = "updated-slug",
        Format = "markdown",
        Body = newContent
    };

    await _yuqueHelper.SaveDocAsync(user.Id, "namespace", docId, request);
}
```

### 3. 构建知识库索引

```csharp
public async Task BuildIndexAsync()
{
    var repos = await _yuqueHelper.GetUserRepositoriesAsync();
    var index = new List<DocumentIndex>();

    foreach (var repo in repos.Data)
    {
        var tree = await _yuqueExtensionHelper.GetTopicTreeAsync(repo.Id);
        index.AddRange(BuildIndexFromTree(tree));
    }

    return index;
}
```

## 🔧 高级配置

### 自定义 HttpClient 配置

```csharp
services.AddYuQueSdk(Configuration.GetSection("YuQue"), options =>
{
    options.HttpClientTimeout = TimeSpan.FromSeconds(30);
    options.RetryCount = 3;
});
```

### 自定义请求头

```csharp
services.AddYuQueSdk(Configuration.GetSection("YuQue"), options =>
{
    options.AdditionalHeaders = new Dictionary<string, string>
    {
        {"X-Custom-Header", "value"}
    };
});
```

## 📖 返回数据格式

所有 API 返回统一的 `YuqueResult<T>` 格式：

```csharp
public class YuqueResult<T>
{
    public int Code { get; set; }  // 状态码
    public string Message { get; set; }  // 消息
    public T Data { get; set; }  // 数据
}
```

## ⚠️ 注意事项

1. **Token 安全** - 请妥善保管您的 Personal Access Token，不要提交到代码仓库
2. **API 限流** - 语雀 API 有调用频率限制，建议添加适当的缓存和重试机制
3. **版本兼容性** - 本 SDK 基于 .NET 6.0+ 开发

## 🤝 贡献指南

当前版本为 beta 阶段，功能正在逐步完善中。

欢迎提交 Issue 和 Pull Request！

## 📄 许可证

[MIT License](LICENSE)

## 🔗 相关链接

- [语雀官方 API 文档](https://www.yuque.com/yuque/developer)
- [Refit 官方文档](https://github.com/reactiveui/refit)
- [项目文档](https://azrng.github.io/nuget-docs)
- [GitHub 仓库](https://github.com/azrng/nuget-packages)
