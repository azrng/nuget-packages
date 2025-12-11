# Azrng.AspNetCore.Authentication.Basic

Basic认证包，该包依赖Azrng.Core.Json或者Azrng.Core.NewtonsoftJson来实现序列化操作

## 操作

注册

```
services.AddAuthentication(BasicAuthentication.AuthenticationSchema)
    .AddBasicAuthentication(options =>
    {
        options.UserName = "admin";
        options.Password = "123456";
    });
```

## 版本更新记录

* 1.0.0
  * 适配.Net10
* 1.0.0-beat1
    * 更新依赖包
* 0.1.0
    * 适配Common.Core1.2.1的修改
    * 适配.Net9
* 0.0.2
    * 增加认证失败响应内容处理
    * 支持.Net6、.Net7、.Net8
* 0.0.1-beta2
    * 增加认证失败响应内容处理
    * 支持.Net6、.Net7、.Net8
* 0.0.1
    * 基础的Basic认证包