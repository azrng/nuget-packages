## Azrng.Core.NewtonsoftJson

### 使用方法

注入配置

```c#
services.ConfigureNewtonsoftJson();

// 支持自定义配置
services.ConfigureNewtonsoftJson((options) =>
{
});
```

使用的时候注入接口`IJsonSerializer`

### 依赖包

Azrng.Core

### 版本更新记录

* 1.2.6
  * 发布正式版
* 1.2.6-beta2
  * 引用.Net10正式包
* 1.2.6-beta1
    * 适配.Net10
* 1.2.5
  * 更新扩展方法Clone为JsonHelper的静态方法
* 1.2.4
  * 修复包引用问题
* 1.2.3
  * 更新ObjectExtensions到ObjectCopyExtensions
* 1.2.2
  * 修复包引用问题
* 1.2.1
  * 修复ToJson报错问题
* 1.2.0
  * 更新IJsonSerializer中ToJson泛型约束
* 1.1.1
  * 修复null值情况下反序列化报错的问题
* 1.1.0
  * 更新引用包
* 1.0.0
    * 基础的序列化包