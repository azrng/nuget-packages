## Azrng.Core.NewtonsoftJson

这是一个基于 Newtonsoft.Json 封装的 JSON 序列化库，提供了常用的 JSON 序列化和反序列化功能。

### 功能特性

- 基于成熟的 Newtonsoft.Json 库
- 支持多框架：.NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0
- 内置常用 JSON 转换器
- 支持自定义序列化配置
- 提供对象深拷贝功能
- 默认使用 CamelCase 命名策略
- 默认时间格式为 "yyyy-MM-dd HH:mm:ss"

### 安装

通过 NuGet 安装:

```
Install-Package Azrng.Core.NewtonsoftJson
```

或通过 .NET CLI:

```
dotnet add package Azrng.Core.NewtonsoftJson
```

### 使用方法

#### 基本配置

在 `Program.cs` 或 `Startup.cs` 中注册服务：

```c#
// 基本配置
services.ConfigureNewtonsoftJson();

// 自定义配置
services.ConfigureNewtonsoftJson(options =>
{
    // 自定义序列化选项
    options.JsonSerializeOptions.Formatting = Formatting.Indented;
    options.JsonDeserializeOptions.DateFormatString = "yyyy-MM-dd";
});
```

#### 在服务中使用

注入 [IJsonSerializer]() 接口并在代码中使用：

```c#
public class MyService
{
    private readonly IJsonSerializer _jsonSerializer;

    public MyService(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public void DoSomething()
    {
        var obj = new MyObject { Name = "test" };

        // 序列化对象
        string json = _jsonSerializer.ToJson(obj);

        // 反序列化对象
        var deserializedObj = _jsonSerializer.ToObject<MyObject>(json);

        // 对象深拷贝
        var clonedObj = _jsonSerializer.Clone(obj);

        // 反序列化为列表
        var list = _jsonSerializer.ToList<MyObject>("[{\"Name\":\"item1\"},{\"Name\":\"item2\"}]");
    }
}
```

#### 静态工具方法

除了通过依赖注入使用外，还提供了部分静态帮助类用于便利操作：

```c#
using Azrng.Core.NewtonsoftJson.Utils;

// JSON 转列表
var list = JsonHelper.ToList<MyObject>("[{\"Name\":\"item1\"},{\"Name\":\"item2\"}]");

// 验证 JSON 格式
bool isValid = JsonHelper.IsJsonString(json);
bool isArray = JsonHelper.IsJArrayString(json);
```

**注意**：其他序列化操作（如对象转 JSON、JSON 转对象、对象深拷贝）建议使用依赖注入的 `IJsonSerializer` 接口，以获得更好的可维护性和一致性。

该库包含以下内置转换器：

- [LongToStringConverter]() - 长整型与字符串互转

可以通过自定义配置添加这些转换器：

```c#
services.ConfigureNewtonsoftJson(options =>
{
    options.JsonSerializeOptions.Converters.Add(new LongToStringConverter());
    options.JsonDeserializeOptions.Converters.Add(new LongToStringConverter());
});
```

### 依赖包

- Azrng.Core
- Newtonsoft.Json

### 版本更新记录

* 1.3.1
  * 更新包引用
* 1.3.0
  * 更新JsonHelper方法
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