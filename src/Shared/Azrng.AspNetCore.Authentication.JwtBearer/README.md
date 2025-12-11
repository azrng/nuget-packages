# Azrng.AspNetCore.Authentication.JwtBearer

nuget：Azrng.AspNetCore.Authentication.JwtBearer

## 使用方法

注入服务

```csharp
services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearerAuthentication(options =>
            {
                options.JwtAudience = jwtKeyConfig.Audience;
                options.JwtIssuer = jwtKeyConfig.Issuer;
                options.JwtSecretKey = jwtKeyConfig.Key;
            });
```

使用依赖注入注入IBearerAuthService来创建Token等

## 版本更新记录

* 1.3.0
  * 支持.Net10
* 1.2.0
  * 移除依赖包
* 1.1.0
  * 适配Common.Core1.2.1的修改
* 1.0.0
  * 从包Common.JwtToken中迁移过来