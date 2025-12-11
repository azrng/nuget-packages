# Azrng.Swashbuckle

## 使用方法

在ConfigureServices中注入

```csharp
services.AddDefaultSwaggerGen()
```

Configure中使用服务

```csharp
app.UseDefaultSwagger();
```

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

