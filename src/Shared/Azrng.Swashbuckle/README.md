# Azrng.Swashbuckle

这是一个基于 Swashbuckle.AspNetCore 的扩展库，提供了更加便捷的方式来配置和使用 Swagger 文档功能。

## 功能特性

- 简化 Swagger 配置流程
- 自动加载 XML 注释文档
- 内置 JWT 认证支持
- 支持多框架：.NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0
- 默认优化的 UI 设置
- 支持开发环境控制

## 安装

通过 NuGet 安装:

```
Install-Package Azrng.Swashbuckle
```

或通过 .NET CLI:

```
dotnet add package Azrng.Swashbuckle
```

## 使用方法

### 基本配置

在 Program.cs 中配置服务：

```csharp
// 在 ConfigureServices 或者直接在 builder.Services 中添加
services.AddDefaultSwaggerGen();

// 或者带自定义配置
services.AddDefaultSwaggerGen(options =>
{
    // 自定义 SwaggerGen 选项
    options.SwaggerDoc("v1", new OpenApiInfo 
    {
        Title = "My API",
        Version = "v1",
        Description = "API 文档描述"
    });
}, title: "My API", showJwtToken: true);
```

在管道配置中启用：

```csharp
// 在 Configure 或者直接在 app 中添加
app.UseDefaultSwagger();

// 或者带自定义配置
app.UseDefaultSwagger(
    onlyDevelopmentEnabled: true, // 仅在开发环境中启用
    setupAction: swaggerOptions => 
    {
        // 自定义 Swagger 选项
    },
    action: uiOptions => 
    {
        // 自定义 UI 选项
        uiOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
```

### 高级配置

#### 启用 JWT 认证

```csharp
services.AddDefaultSwaggerGen(showJwtToken: true);
```

#### 自定义 API 信息

```csharp
services.AddDefaultSwaggerGen(apiInfo: new OpenApiInfo
{
    Title = "我的 API",
    Version = "v1",
    Description = "这是我的 API 描述",
    Contact = new OpenApiContact
    {
        Name = "联系人姓名",
        Email = "contact@example.com"
    }
});
```

#### XML 注释支持

该库会自动加载应用程序目录下的所有 XML 文档文件，确保在项目属性中启用了 XML 文档生成：

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

### 参数说明

#### AddDefaultSwaggerGen 方法参数

- `action`: 自定义 SwaggerGenOptions 配置委托
- `title`: API 标题（默认为 "SwaggerAPI"）
- `showJwtToken`: 是否显示 JWT 认证支持（默认为 false）

#### UseDefaultSwagger 方法参数

- `onlyDevelopmentEnabled`: 是否仅在开发环境中启用（默认为 false）
- `setupAction`: 自定义 SwaggerOptions 配置委托
- `action`: 自定义 SwaggerUIOptions 配置委托

## 版本更新记录

* 0.4.0
  * 更新Jwt授权操作
* 0.3.0
  * 增加对.Net10的支持
* 0.2.1
  * 移除WebApplication的扩展方法UseDefaultSwagger，改为IApplicationBuilder的扩展方法
* 0.2.0
    * 增加对.Net9的支持
* 0.1.0
    * 可扩展性增强
* 0.0.1
    * 基本操作

