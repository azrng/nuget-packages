# Azrng.Core.NewtonsoftJson 架构与原理说明

## 一、项目概述

`Azrng.Core.NewtonsoftJson` 是一个基于 Newtonsoft.Json 封装的 JSON 序列化库，提供了统一的 JSON 序列化接口实现和依赖注入支持，旨在简化 .NET 应用中的 JSON 序列化操作。

### 1.1 核心目标

- 提供 `IJsonSerializer` 接口的 Newtonsoft.Json 实现
- 支持依赖注入，便于集成到 ASP.NET Core 应用
- 提供默认配置和自定义配置能力
- 解决 JavaScript 长整型精度丢失问题
- 提供静态工具类支持快速开发

### 1.2 支持的框架

- .NET 6.0
- .NET 7.0
- .NET 8.0
- .NET 9.0
- .NET 10.0

## 二、架构设计

### 2.1 整体架构

```
┌─────────────────────────────────────────────────────────────┐
│                      应用层 (Application)                     │
│                  services.ConfigureNewtonsoftJson()          │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                    服务注册层 (Registration)                  │
│              JsonSerializerExtension.ConfigureNewtonsoftJson  │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                    接口抽象层 (Abstraction)                   │
│                   IJsonSerializer (Azrng.Core)               │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                    实现层 (Implementation)                    │
│                   NewtonsoftJsonSerializer                   │
└────────────────────────┬────────────────────────────────────┘
                         │
         ┌───────────────┼───────────────┐
         ▼               ▼               ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│   Options    │ │  Converters  │ │  Resolvers   │
│ Configuration│ │   Layer      │ │    Layer     │
└──────────────┘ └──────────────┘ └──────────────┘
         │               │               │
         ▼               ▼               ▼
┌─────────────────────────────────────────────────────────────┐
│                   Newtonsoft.Json 底层库                     │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 核心组件

#### 2.2.1 接口层

**[IJsonSerializer](src/Shared/Azrng.Core/IJsonSerializer.cs)** (来自 Azrng.Core 包)

```csharp
public interface IJsonSerializer
{
    string ToJson<T>(T obj) where T : class;
    T ToObject<T>(string json);
    T Clone<T>(T obj) where T : class;
    List<T> ToList<T>(string json);
}
```

**职责**：定义 JSON 序列化的标准契约，实现与底层序列化库的解耦。

#### 2.2.2 实现层

**[NewtonsoftJsonSerializer](src/Shared/Azrng.Core.NewtonsoftJson/NewtonsoftJsonSerializer.cs)**

核心序列化器实现，通过依赖注入获取配置选项：

```csharp
public class NewtonsoftJsonSerializer : IJsonSerializer
{
    private readonly JsonNetSerializerOptions _serializerOptions;

    public NewtonsoftJsonSerializer(IOptions<JsonNetSerializerOptions> serializerOptions)
    {
        _serializerOptions = serializerOptions.Value;
    }

    public string ToJson<T>(T obj) where T : class
    {
        return JsonConvert.SerializeObject(obj, _serializerOptions.JsonSerializeOptions);
    }

    public T? ToObject<T>(string json)
    {
        return json.IsNullOrWhiteSpace()
            ? default
            : JsonConvert.DeserializeObject<T>(json, _serializerOptions.JsonDeserializeOptions);
    }

    public T? Clone<T>(T obj) where T : class
    {
        return ToObject<T>(ToJson(obj));
    }

    public List<T>? ToList<T>(string json)
    {
        return json.IsNullOrWhiteSpace()
            ? null
            : JsonConvert.DeserializeObject<List<T>>(json, _serializerOptions.JsonDeserializeOptions);
    }
}
```

**设计原理**：
- 采用 Options 模式，支持灵活的配置
- 序列化和反序列化使用独立的配置选项
- 实现了空值安全处理，避免空引用异常
- 深拷贝通过"序列化-反序列化"实现，简单且可靠

#### 2.2.3 配置层

**[JsonNetSerializerOptions](src/Shared/Azrng.Core.NewtonsoftJson/JsonNetSerializerOptions.cs)**

配置选项类，定义序列化行为：

```csharp
public class JsonNetSerializerOptions
{
    private static readonly IContractResolver _camelCaseResolver =
        new CamelCasePropertyNamesContractResolver();

    public JsonSerializerSettings JsonSerializeOptions { get; set; } = CreateDefaultJsonOptions();
    public JsonSerializerSettings JsonDeserializeOptions { get; set; } = CreateDefaultJsonOptions();

    private static JsonSerializerSettings CreateDefaultJsonOptions()
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = _camelCaseResolver,
        };
        settings.Converters.Add(new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
        return settings;
    }
}
```

**默认配置**：
- **命名策略**：CamelCase（驼峰命名），符合 JavaScript 惯例
- **时间格式**：`"yyyy-MM-dd HH:mm:ss"`，中文友好格式
- **分离配置**：序列化和反序列化可独立配置

#### 2.2.4 扩展注册层

**[JsonSerializerExtension](src/Shared/Azrng.Core.NewtonsoftJson/JsonSerializerExtension.cs)**

依赖注入扩展方法：

```csharp
public static void ConfigureNewtonsoftJson(
    this IServiceCollection services,
    Action<JsonNetSerializerOptions>? configureOptions = null)
{
    services.AddScoped<IJsonSerializer, NewtonsoftJsonSerializer>();
    if (configureOptions is not null)
        services.AddOptions<JsonNetSerializerOptions>().Configure(configureOptions);
    else
        services.AddOptions<JsonNetSerializerOptions>();
}
```

**设计特点**：
- 使用 Scoped 生命周期，符合 HTTP 请求的最佳实践
- 支持可选的配置回调
- 自动注册 Options 模式

#### 2.2.5 转换器层

##### **[LongToStringConverter](src/Shared/Azrng.Core.NewtonsoftJson/JsonConverters/LongToStringConverter.cs)**

长整型转字符串转换器：

```csharp
public class LongToStringConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(long) == objectType;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value?.ToString());
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jt = JToken.ReadFrom(reader);
        return jt.Value<long>();
    }
}
```

**应用场景**：解决 JavaScript 中长整型（如雪花算法 ID）精度丢失问题。

##### **[JsonConverterLong](src/Shared/Azrng.Core.NewtonsoftJson/ContractResolvers/JsonConverterLong.cs)**

另一种长整型转换器实现：

```csharp
public class JsonConverterLong : JsonConverter
{
    public override bool CanConvert(Type objectType) => true;

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if ((reader.ValueType == null || reader.ValueType == typeof(long?)) && reader.Value == null)
            return null;
        long.TryParse(reader.Value != null ? reader.Value.ToString() : "", out var result);
        return result;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            writer.WriteValue(value);
        else
            writer.WriteValue(value?.ToString() ?? "");
    }
}
```

**与 LongToStringConverter 的区别**：
- `CanConvert` 总是返回 `true`，需要配合 ContractResolver 使用
- 更完善的空值处理逻辑
- 通过 CustomContractResolver 自动应用

##### **[CustomContractResolver](src/Shared/Azrng.Core.NewtonsoftJson/ContractResolvers/CustomContractResolver.cs)**

自定义契约解析器：

```csharp
public class CustomContractResolver : CamelCasePropertyNamesContractResolver
{
    protected override JsonConverter? ResolveContractConverter(Type objectType)
    {
        return objectType == typeof(long) ? new JsonConverterLong() : base.ResolveContractConverter(objectType);
    }
}
```

**作用**：自动为所有 `long` 类型应用 `JsonConverterLong` 转换器，无需手动标记属性。

#### 2.2.6 工具层

**[JsonHelper](src/Shared/Azrng.Core.NewtonsoftJson/Utils/JsonHelper.cs)**

静态工具类，提供快速序列化方法：

```csharp
public static class JsonHelper
{
    [Obsolete]
    public static string ToJson(object obj) { ... }

    [Obsolete]
    public static T? ToObject<T>(string? json) { ... }

    public static string Serialize(object obj, JsonSerializerSettings? serializerSettings = null) { ... }

    public static T? Deserialize<T>(string? json, JsonSerializerSettings? serializerSettings = null) { ... }

    public static List<T>? ToList<T>(string? json) { ... }

    public static bool IsJArrayString(string? jsonStr) { ... }

    public static bool IsJsonString(string jsonStr) { ... }

    public static T? Clone<T>(T? obj) where T : class { ... }
}
```

**设计意图**：
- 提供不依赖 DI 的快速调用方式
- 旧版本方法标记为 `Obsolete`，引导使用接口方式
- 提供验证和判断工具方法

## 三、核心原理

### 3.1 Options 模式

采用 Microsoft.Extensions.Options 的 Options 模式：

```
┌─────────────────────────────────────────────────────────────┐
│                    IServiceCollection                        │
│         services.ConfigureNewtonsoftJson(options => {        │
│             options.JsonSerializeOptions.Formatting = ...;   │
│         })                                                   │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│              IOptions<JsonNetSerializerOptions>              │
│                      (自动注入)                               │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                  NewtonsoftJsonSerializer                    │
│            构造函数接收 IOptions<T> 获取配置                  │
└─────────────────────────────────────────────────────────────┘
```

**优势**：
- 配置集中管理
- 支持配置热更新（IOptionsSnapshot）
- 配置与代码分离
- 易于测试

### 3.2 序列化流程

#### 3.2.1 对象序列化为 JSON

```
输入对象
    │
    ▼
ToJson<T>(T obj)
    │
    ▼
JsonConvert.SerializeObject(obj, JsonSerializeOptions)
    │
    ▼
应用 CamelCasePropertyNamesContractResolver (重命名属性)
    │
    ▼
应用 IsoDateTimeConverter (格式化时间)
    │
    ▼
应用自定义 Converters (如 LongToStringConverter)
    │
    ▼
输出 JSON 字符串
```

#### 3.2.2 JSON 反序列化为对象

```
JSON 字符串
    │
    ▼
ToObject<T>(string json)
    │
    ├─→ 空值检查 → 返回 default
    │
    ▼
JsonConvert.DeserializeObject<T>(json, JsonDeserializeOptions)
    │
    ▼
应用 CamelCasePropertyNamesContractResolver (属性名匹配)
    │
    ▼
应用自定义 Converters (如 LongToStringConverter)
    │
    ▼
输出强类型对象
```

### 3.3 深拷贝原理

通过序列化-反序列化实现深拷贝：

```csharp
public T? Clone<T>(T obj) where T : class
{
    // 1. 序列化为 JSON 字符串
    string json = ToJson(obj);

    // 2. 反序列化为新对象
    return ToObject<T>(json);
}
```

**特点**：
- 真正的深拷贝，所有引用类型都被复制
- 循环引用会导致序列化失败（需要配置 ReferenceLoopHandling）
- 性能不如反射-based 深拷贝，但实现简单

### 3.4 长整型精度问题解决

#### 问题背景

JavaScript 的 Number 类型最大安全整数为 `2^53 - 1`（9007199254740991），超过此值会丢失精度。雪花算法生成的 ID 通常超过这个限制。

#### 解决方案

将 `long` 序列化为字符串：

```csharp
// 序列化：long → string
writer.WriteValue(value?.ToString() ?? "");

// 反序列化：string → long
long.TryParse(reader.Value?.ToString() ?? "", out var result);
return result;
```

**使用方式**：

方式一：手动添加转换器
```csharp
services.ConfigureNewtonsoftJson(options =>
{
    options.JsonSerializeOptions.Converters.Add(new LongToStringConverter());
    options.JsonDeserializeOptions.Converters.Add(new LongToStringConverter());
});
```

方式二：使用 CustomContractResolver（自动应用）
```csharp
var settings = new JsonSerializerSettings
{
    ContractResolver = new CustomContractResolver()
};
```

## 四、依赖关系

```
Azrng.Core.NewtonsoftJson
    │
    ├─→ Azrng.Core (核心接口和扩展方法)
    │       └─→ IJsonSerializer
    │       └─→ StringExtensions (IsNullOrWhiteSpace)
    │
    ├─→ Newtonsoft.Json (13.0.3)
    │       └─→ JsonConvert
    │       └─→ JsonSerializerSettings
    │       └─→ JsonConverter
    │       └─→ ContractResolver
    │
    └─→ Microsoft.Extensions.* (依赖注入和 Options)
        ├─→ Microsoft.Extensions.DependencyInjection
        └─→ Microsoft.Extensions.Options
```

## 五、使用场景

### 5.1 ASP.NET Core Web API

```csharp
// Program.cs
services.ConfigureNewtonsoftJson();

// Controller
public class UserController : ControllerBase
{
    private readonly IJsonSerializer _jsonSerializer;

    public UserController(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    [HttpGet]
    public IActionResult GetUser()
    {
        var user = new User { Id = 1234567890123456789, Name = "Test" };
        string json = _jsonSerializer.ToJson(user);
        return Ok(json);
    }
}
```

### 5.2 消息队列处理

```csharp
public class MessageHandler
{
    private readonly IJsonSerializer _jsonSerializer;

    public void Handle(string message)
    {
        var event = _jsonSerializer.ToObject<MyEvent>(message);
        // 处理事件...
    }
}
```

### 5.3 缓存序列化

```csharp
public class CacheService
{
    private readonly IJsonSerializer _jsonSerializer;

    public void Set<T>(string key, T value) where T : class
    {
        string json = _jsonSerializer.ToJson(value);
        _cache.Set(key, json);
    }

    public T? Get<T>(string key) where T : class
    {
        string json = _cache.Get(key);
        return _jsonSerializer.ToObject<T>(json);
    }
}
```

## 六、扩展性设计

### 6.1 自定义转换器

继承 `JsonConverter` 实现自定义转换逻辑：

```csharp
public class CustomConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(MyCustomType);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        // 自定义序列化逻辑
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        // 自定义反序列化逻辑
    }
}
```

### 6.2 自定义命名策略

```csharp
public class SnakeCaseContractResolver : DefaultContractResolver
{
    protected override string ResolvePropertyName(string propertyName)
    {
        return propertyName.ToSnakeCase(); // 自定义实现
    }
}

services.ConfigureNewtonsoftJson(options =>
{
    options.JsonSerializeOptions.ContractResolver = new SnakeCaseContractResolver();
});
```

### 6.3 多配置支持

```csharp
services.ConfigureNewtonsoftJson("Api", options =>
{
    options.JsonSerializeOptions.Formatting = Formatting.Indented;
});

services.ConfigureNewtonsoftJson("Database", options =>
{
    options.JsonSerializeOptions.Formatting = Formatting.None;
});
```

## 七、性能考虑

### 7.1 序列化性能

- **对象分配**：每次序列化都会分配字符串
- **反射开销**：Newtonsoft.Json 使用反射，有一定性能开销
- **优化建议**：对于高频场景，考虑使用源生成或预编译

### 7.2 生命周期

使用 `Scoped` 生命周期：
- 每个 HTTP 请求创建一个实例
- 避免单例导致的配置问题
- 平衡性能和资源使用

### 7.3 缓存策略

Newtonsoft.Json 内部缓存了反射元数据，首次序列化较慢，后续序列化性能显著提升。

## 八、测试支持

### 8.1 单元测试

```csharp
[Test]
public void ToJson_ShouldReturnCamelCase()
{
    // Arrange
    var options = new JsonNetSerializerOptions();
    var serializer = new NewtonsoftJsonSerializer(Options.Create(options));
    var obj = new TestClass { UserName = "Test" };

    // Act
    string json = serializer.ToJson(obj);

    // Assert
    Assert.That(json, Does.Contain("\"userName\""));
}
```

### 8.2 Mock 接口

```csharp
var mockSerializer = new Mock<IJsonSerializer>();
mockSerializer.Setup(s => s.ToJson(It.IsAny<object>()))
              .Returns("{\"test\":true}");
```

## 九、安全考虑

### 9.1 反序列化风险

- 避免反序列化不可信的 JSON
- 考虑设置 `TypeNameHandling` 防止类型注入攻击
- 验证输入数据

### 9.2 配置建议

```csharp
services.ConfigureNewtonsoftJson(options =>
{
    // 禁用类型名称处理，防止反序列化攻击
    options.JsonDeserializeOptions.TypeNameHandling = TypeNameHandling.None;
});
```

## 十、版本兼容性

### 10.1 多目标框架

项目支持多目标框架（Multi-targeting），确保在不同 .NET 版本中可用：

```xml
<TargetFrameworks>net6.0;net7.0;net8.0;net9.0;net10.0</TargetFrameworks>
```

### 10.2 包版本管理

针对不同的 .NET 版本，使用对应的 Microsoft.Extensions.* 版本：

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net6.0'">
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="6.0.0"/>
</ItemGroup>

<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0"/>
</ItemGroup>
```

## 十一、总结

### 11.1 优势

1. **统一接口**：通过 IJsonSerializer 接口实现与底层库解耦
2. **依赖注入**：原生支持 DI，易于集成
3. **灵活配置**：Options 模式支持灵活配置
4. **问题解决**：内置解决 JavaScript 长整型精度问题
5. **多框架支持**：支持 .NET 6-10

### 11.2 适用场景

- ASP.NET Core Web API
- 微服务间通信
- 缓存序列化
- 消息队列处理
- 配置文件读写

### 11.3 最佳实践

1. 优先使用依赖注入的 `IJsonSerializer` 接口
2. 在应用启动时统一配置序列化选项
3. 对于有精度问题的长整型，使用 `LongToStringConverter`
4. 自定义转换器处理特殊类型
5. 注意反序列化安全性，避免反序列化不可信数据
