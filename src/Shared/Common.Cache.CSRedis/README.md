# Common.Cache.CSRedis

> 基于 CSRedisCore 的高性能 Redis 缓存服务，支持依赖注入的现代化缓存解决方案

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-1.0.2-green.svg)](https://www.nuget.org/packages/Common.Cache.CSRedis)

## 📖 项目简介

`Common.Cache.CSRedis` 是一个轻量级、高性能的 Redis 缓服务提供者，专为 .NET 应用程序设计。本项目基于 [CSRedisCore](https://github.com/2881099/csredis) 封装，提供了简洁的 API 和完整的依赖注入支持。

### 核心功能

- ✅ **简单易用** - 提供直观的缓存接口，开箱即用
- ✅ **依赖注入** - 原生支持 Microsoft.Extensions.DependencyInjection
- ✅ **高性能** - 基于 CSRedisCore，性能优异
- ✅ **灵活配置** - 支持连接字符串、实例名称等配置
- ✅ **生产就绪** - 适用于生产环境的稳定版本

### 解决的问题

- 简化 Redis 缓存的集成复杂度
- 提供统一的缓存操作接口
- 支持多种 .NET 版本（.NET Standard 2.1+）

## 🛠️ 技术栈

| 组件 | 版本 | 说明 |
|------|------|------|
| CSRedisCore | 3.6.6 | Redis 客户端核心库 |
| Microsoft.Extensions.DependencyInjection.Abstractions | 5.0.0+ | 依赖注入抽象 |
| .NET | Standard 2.1 | 支持多种 .NET 实现 |

## 🚀 快速开始

### 环境要求

- .NET Standard 2.1 或更高版本
- Redis 服务器（本地或远程）

### 安装

```bash
dotnet add package Common.Cache.CSRedis
```

### 配置与使用

#### 1. 注册缓存服务

在 `Startup.cs` 或 `Program.cs` 中配置：

```csharp
services.AddRedisCacheService(() => new RedisConfig
{
    ConnectionString = "localhost:6379,password=your_password,defaultDatabase=0",
    InstanceName = "myapp_prefix"
});
```

#### 2. 注入并使用

```csharp
public class UserService
{
    private readonly IRedisCache _redisCache;

    public UserService(IRedisCache redisCache)
    {
        _redisCache = redisCache;
    }

    public async Task<User> GetUserAsync(string userId)
    {
        // 尝试从缓存获取
        var cachedUser = await _redisCache.GetAsync<User>($"user:{userId}");
        if (cachedUser != null)
        {
            return cachedUser;
        }

        // 从数据库获取
        var user = await _database.GetUserAsync(userId);

        // 写入缓存
        await _redisCache.SetAsync($"user:{userId}", user, TimeSpan.FromMinutes(30));

        return user;
    }
}
```

## 📚 API 文档

### IRedisCache 接口

```csharp
public interface IRedisCache
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expire = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}
```

### 配置选项

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| ConnectionString | string | 是 | Redis 连接字符串 |
| InstanceName | string | 是 | 键前缀，用于隔离不同应用 |

## 🔧 高级配置

### 连接字符串格式

```
host:port,password=your_password,defaultDatabase=0,prefix=your_prefix
```

### 示例配置

```csharp
// 单机 Redis
"localhost:6379,password=123456,defaultDatabase=0"

// Redis 集群
"localhost:6379,password=123456,defaultDatabase=0,poolsize=50"
```

## 🤝 贡献指南

欢迎提交 Issue 和 Pull Request！

## 📄 许可证

[MIT License](LICENSE)

## 🔗 相关链接

- [CSRedisCore 官方文档](https://github.com/2881099/csredis)
- [项目文档](https://azrng.github.io/nuget-docs)
- [GitHub 仓库](https://github.com/azrng/nuget-packages)
