# Azrng.Notification.QYWeiXinRobot

> 企业微信机器人消息推送 SDK，支持多种消息类型和依赖注入

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-1.0.0-green.svg)](https://www.nuget.org/packages/Azrng.Notification.QYWeiXinRobot)

## 📖 项目简介

`Azrng.Notification.QYWeiXinRobot` 是一个功能完善的企业微信机器人消息推送 SDK，为 .NET 应用程序提供便捷的企业微信群机器人集成能力。

### 核心功能

- ✅ **多种消息类型** - 支持文本、图片、文件、Markdown、图文、模板卡片等
- ✅ **依赖注入** - 原生支持 Microsoft.Extensions.DependencyInjection
- ✅ **简单易用** - 提供直观的 API 接口
- ✅ **多框架支持** - 支持 .NET Standard 2.1 和 .NET 6.0+
- ✅ **媒体上传** - 支持图片、文件等媒体资源上传

### 解决的问题

- 简化企业微信机器人集成复杂度
- 提供统一的消息发送接口
- 支持丰富的消息类型
- 封装 HTTP 请求和认证逻辑

## 🛠️ 技术栈

| 组件 | 版本 | 说明 |
|------|------|------|
| Microsoft.Extensions.Http | 3.1.32 / 6.0.0 | HTTP 客户端工厂 |
| Microsoft.Extensions.DependencyInjection.Abstractions | 3.1.32 / 6.0.0 | 依赖注入抽象 |
| Newtonsoft.Json | 13.0.1 / 13.0.3 | JSON 序列化 |
| .NET | Standard 2.1 / 6.0+ | 目标框架 |

## 🚀 快速开始

### 环境要求

- .NET Standard 2.1 或 .NET 6.0+
- 企业微信群机器人 Webhook URL

### 安装

```bash
dotnet add package Azrng.Notification.QYWeiXinRobot
```

### 配置与使用

#### 1. 配置 appsettings.json

```json
{
  "QYWeiXinRobot": {
    "WebhookUrl": "https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=your_key"
  }
}
```

#### 2. 注册服务

```csharp
// Program.cs 或 Startup.cs
services.AddQYWeiXinRobot(Configuration.GetSection("QYWeiXinRobot"));
```

#### 3. 注入并发送消息

```csharp
public class NotificationService
{
    private readonly IQYWeiXinRobotClient _robotClient;

    public NotificationService(IQYWeiXinRobotClient robotClient)
    {
        _robotClient = robotClient;
    }

    public async Task SendTextAsync(string content)
    {
        var message = new TextMessageDto
        {
            Text = new TextContent
            {
                Content = content
            }
        };

        var result = await _robotClient.SendMessageAsync(message);
        if (result.ErrCode != 0)
        {
            // 处理错误
            Console.WriteLine($"发送失败: {result.ErrMsg}");
        }
    }
}
```

## 📚 API 文档

### 支持的消息类型

#### 1. 文本消息

```csharp
var message = new TextMessageDto
{
    Text = new TextContent
    {
        Content = "你的快递已到，请携带工牌前往门卫室领取。"
    }
};
await _robotClient.SendMessageAsync(message);
```

#### 2. Markdown 消息

```csharp
var message = new MarkdownMessageDto
{
    Markdown = new MarkdownContent
    {
        Content = @"实时新增用户反馈<font color=\"warning\">132例</font>，请相关同事注意。
>类型:<font color=\"comment\">用户反馈</font>
>普通用户反馈:<font color=\"comment\">117例</font>
>VIP用户反馈:<font color=\"comment\">15例</font>"
    }
};
await _robotClient.SendMessageAsync(message);
```

#### 3. 图片消息

```csharp
// 先上传图片
var uploadResult = await _robotClient.UploadMediaAsync("image", imageBytes);

// 发送图片消息
var message = new ImageMessageDto
{
    Image = new ImageContent
    {
        MediaId = uploadResult.MediaId
    }
};
await _robotClient.SendMessageAsync(message);
```

#### 4. 文件消息

```csharp
// 先上传文件
var uploadResult = await _robotClient.UploadMediaAsync("file", fileBytes);

// 发送文件消息
var message = new FileMessageDto
{
    File = new FileContent
    {
        MediaId = uploadResult.MediaId
    }
};
await _robotClient.SendMessageAsync(message);
```

#### 5. 图文消息

```csharp
var message = new NewsMessageDto
{
    News = new NewsContent
    {
        Articles = new List<NewsArticle>
        {
            new NewsArticle
            {
                Title = "中秋节礼品领取",
                Description = "今年中秋节公司准备了豪礼，快来领取吧",
                Url = "www.qq.com",
                PicUrl = "http://res.mail.qq.com/node/ww/wwopenmng/images/independents/1580056494232/6e2816c18a9e49e29b31d14db3d36074.jpg"
            }
        }
    }
};
await _robotClient.SendMessageAsync(message);
```

#### 6. 模板卡片消息

```csharp
var message = new TemplateCardMessageDto
{
    TemplateCard = new TemplateCardContent
    {
        CardType = "text_notice",
        Source = new SourceInfo
        {
            IconUrl = "https://wework.qpic.cn/wwpic/xxx",
            Desc = "企业微信",
            DescColor = 0
        },
        MainTitle = new MainTitle
        {
            Title = "欢迎使用企业微信",
            Desc = "您的好友正在邀请您加入企业微信"
        },
        EmphasisContent = new EmphasisContent
        {
            Title = "100",
            Desc = "数据含义"
        },
        SubTitleText = "下载企业微信还能抢红包！",
        HorizontalContentList = new List<HorizontalContent>
        {
            new HorizontalContent
            {
                Keyname = "邀请人",
                Value = "张三"
            }
        },
        JumpList = new List<Jump>
        {
            new Jump
            {
                Type = 1,
                Url = "https://work.weixin.qq.com",
                Title = "企业微信官网"
            }
        },
        CardAction = new CardAction
        {
            Type = 1,
            Url = "https://work.weixin.qq.com"
        }
    }
};
await _robotClient.SendMessageAsync(message);
```

### 核心接口

```csharp
public interface IQYWeiXinRobotClient
{
    // 发送消息
    Task<ApiResult> SendMessageAsync(BaseSendMessageDto message);

    // 上传媒体文件
    Task<FileUploadResult> UploadMediaAsync(string type, byte[] media, string filename = null);
}
```

### 配置模型

```csharp
public class QYWeiXinRobotConfig
{
    public string WebhookUrl { get; set; }
}
```

### API 响应模型

```csharp
public class ApiResult
{
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }
}
```

## 💡 典型应用场景

### 1. 系统告警通知

```csharp
public async Task SendAlertAsync(string alertMessage)
{
    var message = new TextMessageDto
    {
        Text = new TextContent
        {
            Content = $"【系统告警】\n{alertMessage}\n\n时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
        }
    };
    await _robotClient.SendMessageAsync(message);
}
```

### 2. 业务数据推送

```csharp
public async Task SendDailyReportAsync(int userCount, int orderCount)
{
    var content = $@"## 今日数据汇总
> 用户增长: <font color=""info"">{userCount}</font>
> 订单数量: <font color=""warning"">{orderCount}</font>

数据统计时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

    var message = new MarkdownMessageDto
    {
        Markdown = new MarkdownContent { Content = content }
    };
    await _robotClient.SendMessageAsync(message);
}
```

### 3. CI/CD 通知

```csharp
public async Task SendBuildNotificationAsync(string projectName, bool success)
{
    var color = success ? "info" : "warning";
    var status = success ? "成功" : "失败";

    var message = new MarkdownMessageDto
    {
        Markdown = new MarkdownContent
        {
            Content = $@"## 构建通知
项目: {projectName}
状态: <font color=""{color}"">{status}</font>
时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
        }
    };
    await _robotClient.SendMessageAsync(message);
}
```

## 🔧 高级配置

### 自定义 HTTP 客户端

```csharp
services.AddQYWeiXinRobot(new QYWeiXinRobotConfig
{
    WebhookUrl = "your_webhook_url"
});
```

### 多机器人配置

```csharp
// 注册多个机器人
services.AddQYWeiXinRobot("RobotA", configA);
services.AddQYWeiXinRobot("RobotB", configB);

// 使用时注入指定机器人
public class Service
{
    private readonly IQYWeiXinRobotClient _robotA;

    public Service(IQYWeiXinRobotClient robotA)
    {
        _robotA = robotA;
    }
}
```

## ⚠️ 注意事项

1. **频率限制** - 企业微信机器人每分钟最多发送 20 条消息
2. **消息大小** - 文本消息内容不超过 4096 字节
3. **媒体文件** - 图片文件大小不超过 2MB，支持 JPG、PNG 格式
4. **Webhook 安全** - 请妥善保管 Webhook URL，不要泄露到公网仓库

## 📖 消息类型限制

| 消息类型 | 大小限制 | 格式要求 |
|---------|---------|---------|
| 文本 | ≤4096 字节 | UTF-8 编码 |
| Markdown | ≤4096 字节 | 支持 Markdown 格式 |
| 图片 | ≤2MB | JPG、PNG |
| 文件 | ≤20MB | - |
| 图文 | 不限 | 支持图文链接 |
| 模板卡片 | 不限 | JSON 格式 |

## 🤝 贡献指南

欢迎提交 Issue 和 Pull Request！

## 📄 许可证

[MIT License](LICENSE)

## 🔗 相关链接

- [企业微信机器人 API 文档](https://developer.work.weixin.qq.com/document/path/91770)
- [项目文档](https://azrng.github.io/nuget-docs)
- [GitHub 仓库](https://github.com/azrng/nuget-packages)
