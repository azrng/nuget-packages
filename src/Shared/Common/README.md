# Common

> .NET 应用程序的通用工具类库，提供丰富的辅助方法和扩展功能

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-1.4.2-green.svg)](https://www.nuget.org/packages/AzrngCommon)

## 📖 项目简介

`Common` 是一个功能完善的 .NET 通用类库，汇集了日常开发中常用的工具类、扩展方法和辅助函数。本项目旨在提升开发效率，减少重复代码，提供统一、可靠的工具方法。

### 核心功能

- ✅ **HTTP 扩展** - HttpClient 扩展方法，简化 HTTP 请求
- ✅ **辅助工具** - 验证码、压缩、Cookie、拼音转换等常用工具
- ✅ **LayUI 支持** - 提供 LayUI 前端框架配套的 C# 数据模型
- ✅ **安全模块** - 默认用户模型等安全相关功能
- ✅ **多版本支持** - 支持 .NET 6.0/7.0/8.0/9.0

### 解决的问题

- 减少项目中的重复代码
- 提供标准化的工具方法
- 统一团队开发规范
- 加速业务开发进程

## 🛠️ 技术栈

| 组件 | 版本 | 说明 |
|------|------|------|
| PinYinConverterCore | 1.0.2 | 汉字转拼音工具 |
| SixLabors.ImageSharp | 2.1.3 | 图像处理库 |
| SixLabors.ImageSharp.Drawing | 1.0.0-beta15 | 图像绘图工具 |
| Microsoft.Extensions.* | 6.0.0+ | 微软扩展依赖注入等 |

## 🚀 快速开始

### 环境要求

- .NET 6.0 或更高版本
- 支持 .NET Standard 2.1 的运行时

### 安装

```bash
dotnet add package AzrngCommon
```

### 使用示例

#### 1. HTTP 扩展

```csharp
using Common.Extensions;

// 使用 HTTP 扩展方法
var response = await httpClient.GetAsync("/api/users");
```

#### 2. 验证码生成

```csharp
using Common.Helpers;

// 生成验证码
var captchaHelper = new CaptchaHelper();
var captchaText = captchaHelper.GenerateText(4);
var captchaImage = captchaHelper.GenerateImage(captchaText);
```

#### 3. 拼音转换

```csharp
using Common.Helpers;

// 汉字转拼音
var pinyin = PinYinHelper.GetPinyin("中国"); // 返回 "zhongguo"
```

#### 4. 压缩解压

```csharp
using Common.Helpers;

// 字符串压缩
var compressed = CompressHelper.Compress(originalString);
var decompressed = CompressHelper.Decompress(compressed);
```

## 📚 功能模块

### Extensions 扩展方法

- `HttpClientExtension` - HTTP 客户端扩展
- 简化 HTTP 请求处理
- 统一响应格式

### Helpers 辅助工具

- `CaptchaHelper` - 验证码生成
- `CompressHelper` - 字符串压缩/解压
- `CookieHelper` - Cookie 操作
- `PinYinHelper` - 汉字转拼音

### Results 结果模型

针对 LayUI 前端框架优化的结果类：

- `LayAjaxResult` - AJAX 响应结果
- `LayNavbar` - 导航栏数据
- `LayPadding` - 内边距配置
- `LayTreeSelect` - 树形选择
- `LayZTreeNode` - 树节点数据

### Requests 请求模型

- `BasePageRequest` - 分页请求基类

### Security 安全模块

- `DefaultUser` - 默认用户模型

## 💡 典型应用场景

### 1. LayUI 前端对接

```csharp
[HttpGet("users")]
public LayAjaxResult GetUsers(BasePageRequest request)
{
    var users = _userService.GetUsers(request.Page, request.Limit);
    return LayAjaxResult.Success(users);
}
```

### 2. 验证码验证

```csharp
public IActionResult ValidateCaptcha(string input, string sessionCaptcha)
{
    var isValid = CaptchaHelper.Validate(input, sessionCaptcha);
    return Json(new { success = isValid });
}
```

### 3. 数据压缩传输

```csharp
public async Task SendDataAsync(string data)
{
    var compressed = CompressHelper.Compress(data);
    await _httpClient.PostAsync("/api/data", compressed);
}
```

## 🔧 配置选项

本项目无需特殊配置，安装后即可使用。

## 📦 依赖说明

本项目依赖以下 NuGet 包：

```
PinYinConverterCore (1.0.2)
SixLabors.ImageSharp (2.1.3)
SixLabors.ImageSharp.Drawing (1.0.0-beta15)
Microsoft.Extensions.DependencyInjection
Microsoft.Extensions.Configuration
Microsoft.AspNetCore.Http.Extensions
```

## 🤝 贡献指南

欢迎提交 Issue 和 Pull Request！

## 📄 许可证

[MIT License](LICENSE)

## 🔗 相关链接

- [项目文档](https://azrng.github.io/nuget-docs)
- [GitHub 仓库](https://github.com/azrng/nuget-packages)
