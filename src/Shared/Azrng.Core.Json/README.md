## Azrng.Core.Json

这是一个基于 System.Text.Json 封装的 JSON 序列化库，提供了常用的 JSON 序列化和反序列化功能，并内置了一些常用的转换器。

### 功能特性

- 支持多框架：.NET 6.0 / 7.0 / 8.0 / 9.0 / 10.0
- 内置常用 JSON 转换器（如 LongToStringConverter、DateTime 转换器等）
- 支持自定义序列化配置
- 支持处理不规范的 JSON 格式（如包含注释、尾随逗号等）
- 提供对象深拷贝功能
- 支持 AOT 和修剪兼容性

### 安装

通过 NuGet 安装:

```
Install-Package Azrng.Core.Json
```

或通过 .NET CLI:

```
dotnet add package Azrng.Core.Json
```

### 使用方法

#### 基本配置

在 `Program.cs` 或 `Startup.cs` 中注册服务：

```c#
// 基本配置
services.ConfigureDefaultJson();

// 自定义配置
services.ConfigureDefaultJson(options =>
{
    // 自定义序列化选项
    options.JsonSerializeOptions.WriteIndented = true;
    options.JsonDeserializeOptions.PropertyNameCaseInsensitive = false;
});
```

#### 在服务中使用

注入`IJsonSerializer`接口并在代码中使用：

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

除了通过依赖注入使用外，还提供了一个静态帮助类用于对象深拷贝：

```c#
using Azrng.Core.Json.Utils;

var obj = new MyObject { Name = "test" };
var clonedObj = JsonHelper.Clone(obj);
```

#### 内置转换器

该库包含以下内置转换器：

- [LongToStringConverter]() - 长整型与字符串互转
- [DateTimeConverter]() - DateTime 类型处理
- [DateTimeOffsetConverter]() - DateTimeOffset 类型处理
- [EnumStringConverter]() - 枚举与字符串互转
- [NullableStringConverter]() - 可空字符串处理
- [NullableStructConverterFactory]() - 可空结构体处理

可以通过自定义配置添加这些转换器：

```c#
services.ConfigureDefaultJson(options =>
{
    options.JsonSerializeOptions.Converters.Add(new LongToStringConverter());
    options.JsonDeserializeOptions.Converters.Add(new LongToStringConverter());
});
```

### 版本更新记录

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