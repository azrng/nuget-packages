## Azrng.AspNetCore.Inject

这是一个基于 ASP.NET Core 的自动依赖注入库，提供了模块化的依赖注入机制，可以简化服务注册过程。

### 功能特性

- 模块化服务注册
- 自动依赖注入
- 支持多种注入模式
- 循环依赖检测
- 模块依赖管理
- 支持生命周期配置

### 安装

通过 NuGet 安装:

```
Install-Package Azrng.AspNetCore.Inject
```

或通过 .NET CLI:

```
dotnet add package Azrng.AspNetCore.Inject
```

### 使用方法

#### 1. 创建模块

首先创建一个模块类，实现 [IModule](IModule.cs) 接口：

```csharp
public class MyModule : IModule
{
    public void ConfigureServices(ServiceContext services)
    {
        // 在这里注册服务
        services.Services.AddScoped<IMyService, MyService>();
    }
}
```

#### 2. 定义需要注入的服务

使用 [[InjectOn]](InjectOnAttribute.cs) 特性标记需要自动注入的类：

```csharp
[InjectOn(Lifetime = ServiceLifetime.Scoped, Scheme = InjectScheme.Any)]
public class UserService : IUserService
{
    // 实现
}
```

#### 3. 定义模块依赖关系

使用 [[InjectModule]]() 特性声明模块间的依赖关系：

```csharp
[InjectModule<AnotherModule>]
[InjectModule(typeof(ThirdModule))]
public class MyModule : IModule
{
    public void ConfigureServices(ServiceContext services)
    {
        // 配置服务
    }
}
```

#### 4. 在 Program.cs 中注册模块

```csharp
var builder = WebApplication.CreateBuilder(args);

// 注册模块
builder.Services.AddModule<MyModule>();

var app = builder.Build();
```

### 注入特性详解

#### InjectOnAttribute

[[InjectOn]]() 特性用于标记需要自动注入的类，具有以下属性：

- `Lifetime`: 服务生命周期（Transient、Scoped、Singleton）
- `Scheme`: 注入模式
  - `Any`: 注入所有接口和基类
  - `Some`: 手动指定要注入的服务
  - `OnlyBaseClass`: 只注入基类
  - `OnlyInterfaces`: 只注入实现的接口
  - `None`: 不注入
- `Own`: 是否注入自身类型
- `ServicesType`: 当 Scheme 为 Some 时，手动指定要注入的服务类型

示例：

```csharp
// 注入所有接口，生命周期为 Scoped
[InjectOn(Lifetime = ServiceLifetime.Scoped, Scheme = InjectScheme.OnlyInterfaces)]
public class UserService : IUserService, IAnotherService
{
}

// 注入特定服务
[InjectOn(Lifetime = ServiceLifetime.Singleton, Scheme = InjectScheme.Some,
          ServicesType = new[] { typeof(IMyService), typeof(IAnotherService) })]
public class MyService : IMyService, IAnotherService, IDisposable
{
}
```

#### InjectModuleAttribute

[[InjectModule]]() 特性用于声明模块依赖关系：

```csharp
// 泛型方式
[InjectModule<DatabaseModule>]
// 或者类型方式
[InjectModule(typeof(LoggingModule))]
public class BusinessModule : IModule
{
    // 实现
}
```

### 模块生命周期

模块按照依赖关系进行初始化，系统会构建模块依赖树并按后序遍历的方式初始化模块，确保依赖模块先于被依赖模块初始化。

### 循环依赖检测

系统会自动检测模块间的循环依赖关系，如果发现循环依赖会抛出异常：

```
System.OverflowException: 检测到循环依赖引用或重复引用！
```

### 最佳实践

1. **合理划分模块**：按照业务功能划分模块，避免模块过大或过小

2. **明确依赖关系**：使用 [InjectModule] 明确声明模块依赖

3. **选择合适的注入模式**：
   - 对于只实现单一接口的服务，使用 `OnlyInterfaces`
   - 对于需要注入具体类型的场景，设置 `Own = true`
   - 对于复杂的注入需求，使用 `Some` 模式手动指定

4. **生命周期管理**：
   - 瞬时对象使用 `ServiceLifetime.Transient`
   - Web 请求相关的服务使用 `ServiceLifetime.Scoped`
   - 全局单例使用 `ServiceLifetime.Singleton`

### 依赖包

- Microsoft.Extensions.Configuration.Abstractions
- Microsoft.Extensions.DependencyInjection

### 注意事项

1. 确保模块类实现了 [IModule](IModule.cs) 接口
2. 使用自动注入的类必须是非抽象、非静态的公共类
3. 模块依赖关系不能形成循环
4. 系统会自动处理重复注册的情况