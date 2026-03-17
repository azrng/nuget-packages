# Azrng.SettingConfig

> 一个轻量级、高性能的业务配置管理解决方案，为 ASP.NET Core 应用提供可视化配置管理界面。

[![NuGet](https://img.shields.io/badge/NuGet-1.4.0-green.svg)](https://www.nuget.org/packages/SettingConfig)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0%20%7C%209.0%20%7C%2010.0-purple.svg)](https://dotnet.microsoft.com/download)

## ✨ 特性

- 🚀 **零依赖前端** - 完全使用原生 JavaScript 和现代 CSS，无第三方库依赖
- 📱 **响应式设计** - 完美支持移动端和桌面端，自适应各种屏幕尺寸
- 🎨 **现代化 UI** - 采用 Grid 和 Flexbox 布局，简洁美观的用户界面
- 💾 **多数据库支持** - 内置 PostgreSQL 支持，可扩展其他数据库
- 🔄 **版本控制** - 配置变更历史记录，支持版本回滚
- 🚦 **访问控制** - 支持 Basic 认证和自定义授权策略
- 💨 **智能缓存** - 内置内存缓存，可扩展分布式缓存
- 📊 **可视化界面** - 直观的配置管理 Dashboard，支持搜索、筛选、编辑

## 📦 安装

### 通过 NuGet 安装

```bash
dotnet add package SettingConfig
```

### 可选：Basic 认证支持

```bash
dotnet add package Azrng.SettingConfig.BasicAuthorization
```

## 🚀 快速开始

### 1️⃣ 配置服务

在 `Program.cs` 中添加配置服务：

```csharp
using Azrng.SettingConfig;

var builder = WebApplication.CreateBuilder(args);

// 获取数据库连接字符串
var conn = builder.Configuration.GetConnectionString("pgsql");

// 添加 SettingConfig 服务
builder.Services.AddSettingConfig(options =>
{
    // 数据库配置
    options.DbConnectionString = conn;
    options.DbSchema = "sample";  // 数据库 Schema

    // 路由配置
    options.RoutePrefix = "configDashboard";           // Dashboard 路由前缀
    options.ApiRoutePrefix = "/api/configDashboard";   // API 路由前缀

    // 可选：页面个性化
    options.PageTitle = "系统配置管理";
});

var app = builder.Build();

// 配置 Dashboard 中间件
app.UseSettingDashboard();

app.Run();
```

### 2️⃣ 访问管理界面

启动项目后，访问默认地址：

```
https://localhost:5001/configDashboard
```

### 3️⃣ 在代码中使用配置

```csharp
using Azrng.SettingConfig.Service;

public class MyService
{
    private readonly IConfigExternalProvideService _configService;

    public MyService(IConfigExternalProvideService configService)
    {
        _configService = configService;
    }

    public async Task ExampleAsync()
    {
        // 获取配置内容（字符串）
        var content = await _configService.GetConfigContentAsync("my-config-key");

        // 获取配置内容（反序列化对象）
        var settings = await _configService.GetConfigAsync<MySettings>("my-config-key");
    }
}
```

## 🔐 访问控制

### 默认策略

默认情况下，Dashboard 仅允许本地访问（127.0.0.1）。

### Basic 认证

安装 `Azrng.SettingConfig.BasicAuthorization` 包并配置：

```csharp
using Azrng.SettingConfig.BasicAuthorization;

builder.Services.AddSettingConfig(options =>
{
    options.DbConnectionString = conn;
    options.DbSchema = "sample";
    options.RoutePrefix = "configDashboard";
    options.ApiRoutePrefix = "/api/configDashboard";

    // 配置 Basic 认证
    options.Authorization = new[]
    {
        new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
        {
            RequireSsl = false,           // 是否要求 HTTPS
            SslRedirect = false,          // 是否自动重定向到 HTTPS
            LoginCaseSensitive = true,    // 用户名是否区分大小写
            Users = new[]
            {
                new BasicAuthAuthorizationUser
                {
                    Login = "admin",
                    PasswordClear = "your-password"  // 生产环境请使用加密密码
                }
            }
        })
    };
});
```

### 自定义授权

实现 `IDashboardAuthorizationFilter` 接口创建自定义授权策略：

```csharp
public class CustomAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardFilterContext context)
    {
        // 自定义授权逻辑
        var httpContext = context.HttpContext;
        // 实现你的授权判断
        return true;
    }
}
```

## 🎨 个性化配置

### 页面配置

```csharp
options.PageTitle = "我的配置中心";        // 页面标题
options.RoutePrefix = "admin/config";     // 路由前缀
options.ApiRoutePrefix = "/api/config";   // API 路由前缀
```

### 数据库配置

```csharp
options.DbConnectionString = connectionString;  // 连接字符串
options.DbSchema = "public";                     // Schema 名称
```

## 🔧 扩展开发

### 自定义数据源

实现 `IDataSourceProvider` 接口支持其他数据库：

```csharp
public class CustomDataSourceProvider : IDataSourceProvider
{
    public async Task InitializeAsync()
    {
        // 初始化数据库连接
    }

    public async Task<IEnumerable<ConfigItem>> GetConfigsAsync()
    {
        // 获取配置列表
    }

    // 实现其他接口方法...
}
```

## 版本更新记录

* **1.4.0** (最新) - 🎉 重大优化版本
  - 🚀 **前端优化**: 移除所有外部依赖 (jQuery, Bootstrap, Bootstrap Table, Layer.js)
  - ✅ **后端优化**: 简化代码，提升可维护性
  - 🔒 **安全增强**: 修复所有 XSS 漏洞，添加完整的安全头
  - ⚡ **性能提升**: 资源大小减少 97%，加载时间减少 75%
  * 🚀 **重大改进**：完全重构前端，移除所有外部依赖 (jQuery, Bootstrap, Bootstrap Table, Layer.js)
  * ✅ **性能优化**：资源大小减少 97% (500KB → 15KB)，加载时间减少 75%
  * 🔒 **安全增强**：修复所有 XSS 漏洞，添加完整的安全头 (CSP, X-Frame-Options 等)
  * 🎨 **UI 重构**：采用现代化设计，响应式布局，更好的用户体验
  * 📱 **移动端优化**：完美支持各种屏幕尺寸
  * ✨ **功能增强**：改进搜索、分页、复制等功能
  * 🛠️ **技术升级**：使用原生 JavaScript，现代 CSS (Grid, Flexbox)

* 1.3.1
  * 🆕 新增：支持完全离线使用，所有前端资源本地化
  * ✅ 优化：下载并本地化 Bootstrap、jQuery、Bootstrap Table 等依赖资源
  * ✅ 优化：添加 Bootstrap Icons 字体文件支持
  * ✅ 优化：内网环境无需外部网络连接即可正常使用

* 1.3.0
  * 🆕 新增：支持 .NET 9.0
  * ✅ 优化：启用可空引用类型支持
  * ✅ 优化：改进包版本管理，使用浮动版本号
  * ✅ 优化：完善 `DashboardOptions` 的 XML 文档注释
  * ✅ 重构：移除注释代码，清理构造函数逻辑

* 1.2.0
  * 支持 .NET 10

* 1.1.0
    * 适配 Azrng.Core 1.2.1 的修改

* 1.0.1
    * 支持通过调用 AddIfNotExistsAsync 接口初始化数据

* 1.0.0
    * 增加了历史版本配置的复制
    * 增加 Basic 认证方案

* 0.0.1
    * 基本的配置更新