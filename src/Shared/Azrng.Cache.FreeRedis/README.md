# Azrng.Cache.FreeRedis

[![NuGet](https://img.shields.io/nuget/v/Azrng.Cache.FreeRedis.svg)](https://www.nuget.org/packages/Azrng.Cache.FreeRedis)
[![Downloads](https://img.shields.io/nuget/dt/Azrng.Cache.FreeRedis.svg)](https://www.nuget.org/packages/Azrng.Cache.FreeRedis)

基于 FreeRedis 的缓存服务提供者，支持依赖注入，提供简单易用的缓存接口。

## 功能特性

- ✅ 支持 FreeRedis 客户端
- ✅ 支持 .NET Standard 2.1
- ✅ 支持依赖注入
- ✅ 线程安全
- ✅ 简单易用的 API

## 安装

```bash
dotnet add package Azrng.Cache.FreeRedis
```

## 快速开始

### 1. 配置服务

在 `Startup.cs` 或 `Program.cs` 中添加服务：

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // 添加 FreeRedis 缓存服务
    services.AddFreeRedisCache(options =>
    {
        options.ConnectionString = "localhost:6379,defaultDatabase=0";
        options.RedisClientName = "Default";
    });
}
```

### 2. 注入使用

```csharp
public class MyService
{
    private readonly ICacheService _cacheService;

    public MyService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task Example()
    {
        // 设置缓存
        await _cacheService.SetAsync("key", "value", TimeSpan.FromMinutes(30));

        // 获取缓存
        var value = await _cacheService.GetAsync<string>("key");

        // 删除缓存
        await _cacheService.RemoveAsync("key");

        // 检查缓存是否存在
        var exists = await _cacheService.ExistsAsync("key");
    }
}
```

## 配置选项

| 配置项 | 类型 | 说明 |
|--------|------|------|
| ConnectionString | string | Redis 连接字符串 |
| RedisClientName | string | Redis 客户端名称 |

## API 文档

### ICacheService

#### SetAsync
```csharp
Task SetAsync<T>(string key, T value, TimeSpan expireTime = default);
```
设置缓存值。

#### GetAsync
```csharp
Task<T> GetAsync<T>(string key);
```
获取缓存值。

#### RemoveAsync
```csharp
Task RemoveAsync(string key);
```
删除指定缓存。

#### ExistsAsync
```csharp
Task<bool> ExistsAsync(string key);
```
检查缓存是否存在。

## 依赖项

- [FreeRedis](https://github.com/2881099/FreeRedis) >= 1.2.15
- [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection) >= 3.1.9
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json) >= 12.0.3

## 许可证

版权归 Azrng 所有

## 相关项目

- [Azrng.Cache.Core](https://www.nuget.org/packages/Azrng.Cache.Core) - 缓存核心接口定义
- [Common.Cache.CSRedis](https://www.nuget.org/packages/Common.Cache.CSRedis) - CSRedis 缓存提供者
