## Azrng.Core.Json

### 使用方法

注入配置

```c#
services.ConfigureDefaultJson();

// 还支持自定义配置
services.ConfigureDefaultJson(options=>{ });
```

使用的时候注入接口`IJsonSerializer`

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
  * 修复ToJson报错问题
* 1.2.2
  * 更新IJsonSerializer中ToJson泛型约束
* 1.2.1
  * 修复null值情况下反序列化报错的问题
* 1.2.0
    * ConfigureDefaultJson支持设置自定义的序列化设置
    * 支持反序列不规格json，比如包含注释、尾随逗号
* 1.1.0
    * 更新引用程序包
    * 更新命名空间
* 1.0.1-beta2
    * 扩展注入的时候支持无参方式
    * 增加对象深拷贝方法

* 1.0.1-beta1
    * 增加序列化转换器LongToStringConverter
    * 优化写法，调整目录
* 1.0.0
    * 基础的序列化包