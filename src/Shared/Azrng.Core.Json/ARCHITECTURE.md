# Azrng.Core.Json 项目架构说明

## 1. 项目概述

`Azrng.Core.Json` 是一个基于 .NET `System.Text.Json` 封装的 JSON 序列化库，提供了统一的 JSON 序列化接口和丰富的自定义转换器。项目设计遵循依赖注入原则，支持 AOT 编译和代码修剪，确保在现代 .NET 应用中具有最佳性能。

## 2. 整体架构

```
┌─────────────────────────────────────────────────────────────────┐
│                        应用层 (Application)                      │
│  services.ConfigureDefaultJson()                                 │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                      接口抽象层 (Azrng.Core)                     │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  IJsonSerializer                                         │    │
│  │    - ToJson<T>()                                         │    │
│  │    - ToObject<T>()                                       │    │
│  │    - Clone<T>()                                          │    │
│  │    - ToList<T>()                                         │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                      实现层 (Implementation)                     │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  SysTextJsonSerializer : IJsonSerializer                 │    │
│  │  ┌─────────────────────────────────────────────────┐    │    │
│  │  │  DefaultJsonSerializerOptions                    │    │    │
│  │  │    - JsonSerializeOptions                         │    │    │
│  │  │    - JsonDeserializeOptions                       │    │    │
│  │  └─────────────────────────────────────────────────┘    │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    转换器层 (Converters)                         │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐     │
│  │LongToString    │  │DateTimeFormat │  │EnumString      │     │
│  │Converter       │  │Converters     │  │Converter       │     │
│  └────────────────┘  └────────────────┘  └────────────────┘     │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐     │
│  │NullableStruct  │  │JsonCompatible  │  │NullableString  │     │
│  │ConverterFactory│  │Converter       │  │Converter       │     │
│  └────────────────┘  └────────────────┘  └────────────────┘     │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    工具层 (Utilities)                           │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  JsonHelper (Static Utility Class)                      │    │
│  │    - Serialize() / Deserialize()                         │    │
│  │    - Clone()                                             │    │
│  │    - IsJsonString()                                      │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
```

## 3. 核心组件

### 3.1 IJsonSerializer 接口

**位置**: `Azrng.Core/IJsonSerializer.cs`

定义了 JSON 序列化的标准接口：

```csharp
public interface IJsonSerializer
{
    string ToJson<T>(T obj) where T : class;
    T ToObject<T>(string json);
    T Clone<T>(T obj) where T : class;
    List<T> ToList<T>(string json);
}
```

**设计理念**:
- 接口定义在 `Azrng.Core` 基础包中，便于其他序列化实现（如 Newtonsoft.Json）也实现此接口
- 泛型约束确保类型安全
- 方法简洁，覆盖常用场景

### 3.2 SysTextJsonSerializer 实现类

**位置**: `Azrng.Core.Json/SysTextJsonSerializer.cs`

核心实现类，封装了 `System.Text.Json.JsonSerializer`。

**关键特性**:
1. **AOT 友好**: 使用 `UnconditionalSuppressMessage` 抑制修剪警告
2. **依赖注入**: 通过 `IOptions<DefaultJsonSerializerOptions>` 注入配置
3. **静态辅助方法**: 提供 `GetValue` 和 `JsonToArrayList` 等静态方法

**工作原理**:
```
序列化流程:
对象 → JsonSerializer.Serialize(JsonSerializeOptions) → JSON字符串

反序列化流程:
JSON字符串 → JsonSerializer.Deserialize(JsonDeserializeOptions) → 对象

深拷贝流程:
对象 → ToJson() → JSON字符串 → ToObject() → 新对象
```

### 3.3 DefaultJsonSerializerOptions 配置类

**位置**: `Azrng.Core.Json/DefaultJsonSerializerOptions.cs`

管理序列化和反序列化的默认配置。

**默认配置**:
| 配置项 | 值 | 说明 |
|--------|-----|------|
| PropertyNameCaseInsensitive | true | 属性名不区分大小写 |
| PropertyNamingPolicy | CamelCase | 驼峰命名策略 |
| DictionaryKeyPolicy | CamelCase | 字典键使用驼峰 |
| Encoder | UnsafeRelaxedJsonEscaping | 宽松转义，减少转义字符 |
| ReferenceHandler | IgnoreCycles | 忽略循环引用 |
| ReadCommentHandling | Skip | 跳过 JSON 注释 |
| AllowTrailingCommas | true | 允许尾随逗号 |

**双选项设计**:
- `JsonSerializeOptions`: 用于序列化
- `JsonDeserializeOptions`: 用于反序列化，额外包含兼容性转换器

### 3.4 JsonSerializerExtension 扩展类

**位置**: `Azrng.Core.Json/JsonSerializerExtension.cs`

提供依赖注入配置扩展方法。

```csharp
public static void ConfigureDefaultJson(
    this IServiceCollection services,
    Action<DefaultJsonSerializerOptions>? configureOptions = null)
```

**使用模式**:
```csharp
// 默认配置
services.ConfigureDefaultJson();

// 自定义配置
services.ConfigureDefaultJson(options =>
{
    options.JsonSerializeOptions.WriteIndented = true;
});
```

## 4. 自定义转换器架构

### 4.1 转换器类型与用途

| 转换器 | 用途 | 应用场景 |
|--------|------|----------|
| LongToStringConverter | long ↔ string | JavaScript 精度问题处理 |
| DateTimeToStringConverter | DateTime 格式化 | 统一时间格式 |
| DateTimeOffsetConverter | DateTimeOffset 处理 | 时区感知场景 |
| EnumStringConverter | 枚举 ↔ 字符串 | 友好的枚举展示 |
| NullableStructConverterFactory | 可空结构体处理 | 空字符串转 null |
| JsonCompatibleConverter | 兼容性转换器 | 反序列化兼容 |

### 4.2 LongToStringConverter 原理

**问题背景**: JavaScript 的 `Number` 类型精度为 53 位，无法安全表示 64 位长整型，导致精度丢失。

**解决方案**:
```
序列化: long(1234567890123456789) → "1234567890123456789"
反序列化: "1234567890123456789" → long(1234567890123456789)
```

### 4.3 EnumStringConverter 架构

**设计模式**: 工厂模式 + 泛型

```
EnumStringConverterFactory (检测枚举类型)
         │
         ▼
    CreateConverter()
         │
         ▼
EnumStringConverter<TEnum> (具体转换器)
         │
    ┌────┴────┐
    ▼         ▼
   Read()    Write()
  (JSON→值)  (值→JSON)
```

**类型处理**:
- 支持枚举类型
- 支持 `Nullable<TEnum>` 类型
- 自动检测底层类型

### 4.4 NullableStructConverterFactory 原理

**处理场景**: 将空字符串/空白字符串转换为 `null`

```csharp
// 输入: { "value": "" }
// 输出: obj.Value == null

// 输入: { "value": "   " }
// 输出: obj.Value == null
```

**实现机制**:
1. 检测类型是否有 `HasValue` 属性（可空结构体特征）
2. 读取时判断 `JsonTokenType.String` 且内容为空
3. 动态创建 `NullableConverter<T>` 实例

### 4.5 JsonCompatibleConverter 兼容处理

**作用**: 提供反序列化时的向后兼容性

| 转换器 | 说明 |
|--------|------|
| EnumReader | 使用 `JsonStringEnumConverter` 兼容多种枚举格式 |
| DateTimeReader | 使用 ISO 8601 格式 ("O") 兼容多种时间格式 |

**条件启用**:
```csharp
if (RuntimeFeature.IsDynamicCodeSupported)
{
    options.Converters.Add(JsonCompatibleConverter.EnumReader);
}
```

## 5. 工具类设计

### 5.1 JsonHelper 静态类

**位置**: `Azrng.Core.Json/Utils/JsonHelper.cs`

提供无需依赖注入的静态方法。

**方法对比**:

| 功能 | IJsonSerializer | JsonHelper |
|------|-----------------|------------|
| 序列化 | ToJson() | Serialize() |
| 反序列化 | ToObject() | Deserialize() |
| 深拷贝 | Clone() | Clone() |
| 列表反序列化 | ToList() | - |
| JSON 校验 | - | IsJsonString() |

**深拷贝实现差异**:
```csharp
// IJsonSerializer: 使用默认配置
public T Clone<T>(T obj) => ToObject<T>(ToJson(obj));

// JsonHelper: 使用 UTF-8 字节数组（更高效）
public static T? Clone<T>(T? obj)
{
    var jsonString = JsonSerializer.SerializeToUtf8Bytes(obj);
    return JsonSerializer.Deserialize<T>(jsonString);
}
```

## 6. AOT 和修剪兼容性

### 6.1 问题背景

- **修剪**: 发布时删除未使用的代码以减小程序集大小
- **AOT**: 预先编译，提高启动速度
- **反射**: 传统的 JSON 序列化依赖反射，与 AOT/修剪不兼容

### 6.2 解决方案

1. **抑制警告**: 使用 `UnconditionalSuppressMessage` 特性
```csharp
[UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
    Justification = "JsonSerializer.IsReflectionEnabledByDefault...")]
```

2. **条件编译**: 根据 `RuntimeFeature.IsDynamicCodeSupported` 决定是否使用动态转换器

3. **TypeInfoResolverChain** (.NET 8+):
```csharp
public void PrependJsonSerializerContext(JsonSerializerContext context)
{
    this.JsonSerializeOptions.TypeInfoResolverChain.Insert(0, context);
    this.JsonDeserializeOptions.TypeInfoResolverChain.Insert(0, context);
}
```

## 7. 数据流图

### 7.1 序列化流程

```
┌─────────────┐
│   对象 T     │
└──────┬──────┘
       │
       ▼
┌─────────────────────────────┐
│ SysTextJsonSerializer.ToJson│
└──────┬──────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│ JsonSerializer.Serialize(obj, Options)  │
└──────┬──────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│    应用自定义转换器 (如需要)              │
│  - LongToStringConverter                 │
│  - DateTimeToStringConverter             │
│  - EnumStringConverter                   │
└──────┬──────────────────────────────────┘
       │
       ▼
┌─────────────┐
│ JSON 字符串  │
└─────────────┘
```

### 7.2 反序列化流程

```
┌─────────────┐
│ JSON 字符串  │
└──────┬──────┘
       │
       ▼
┌─────────────────────────────────┐
│SysTextJsonSerializer.ToObject<T>│
└──────┬──────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────┐
│JsonSerializer.Deserialize<T>(json, Options) │
└──────┬──────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│   应用兼容性转换器                        │
│  - JsonStringEnumConverter               │
│  - JsonDateTimeConverter                 │
│  - NullableStructConverterFactory        │
└──────┬──────────────────────────────────┘
       │
       ▼
┌─────────────┐
│   对象 T     │
└─────────────┘
```

## 8. 设计模式

### 8.1 策略模式
- `IJsonSerializer` 接口定义策略
- `SysTextJsonSerializer` 为具体策略实现

### 8.2 工厂模式
- `JsonConverterFactory` 基类
- `EnumStringConverterFactory`、`NullableStructConverterFactory` 实现动态转换器创建

### 8.3 选项模式
- `DefaultJsonSerializerOptions` 封装配置
- 通过 `IOptions<T>` 注入

### 8.4 扩展方法模式
- `JsonSerializerExtension` 提供 `IServiceCollection` 扩展
- 简化服务注册代码

## 9. 性能考虑

1. **UTF-8 字节数组**: `JsonHelper.Clone` 使用 `SerializeToUtf8Bytes` 避免中间字符串分配
2. **宽松转义**: `UnsafeRelaxedJsonEscaping` 减少不必要的转义字符
3. **转换器缓存**: 工厂类内部缓存转换器实例（如 `JsonCompatibleConverter._stringEnumConverter`）
4. **避免反射**: AOT 支持通过 `TypeInfoResolver` 提供元数据

## 10. 扩展性

### 10.1 添加自定义转换器

```csharp
services.ConfigureDefaultJson(options =>
{
    options.JsonSerializeOptions.Converters.Add(new MyCustomConverter());
    options.JsonDeserializeOptions.Converters.Add(new MyCustomConverter());
});
```

### 10.2 实现自定义序列化器

```csharp
public class MyJsonSerializer : IJsonSerializer
{
    public string ToJson<T>(T obj) where T : class
    {
        // 自定义实现
    }
    // ...
}

// 注册
services.ConfigureDefaultJson();
services.Replace(ServiceDescriptor.Singleton<IJsonSerializer, MyJsonSerializer>());
```

## 11. 最佳实践

1. **依赖注入**: 优先使用 `IJsonSerializer` 接口
2. **静态方法**: 非依赖注入场景使用 `JsonHelper`
3. **配置管理**: 通过 `ConfigureDefaultJson` 统一配置
4. **类型安全**: 注意 JavaScript 数值精度问题，使用 `LongToStringConverter`
5. **AOT 准备**: .NET 8+ 项目考虑使用 `PrependJsonSerializerContext`

## 12. 依赖关系

```
Azrng.Core.Json
    │
    ├── Azrng.Core (IJsonSerializer 接口)
    │   └── Azrng.Core.Extension (IsNullOrWhiteSpace 扩展)
    │
    ├── Microsoft.Extensions.DependencyInjection.Abstractions (DI)
    ├── Microsoft.Extensions.Options.Abstractions (选项模式)
    │
    └── System.Text.Json (核心序列化库)
```
