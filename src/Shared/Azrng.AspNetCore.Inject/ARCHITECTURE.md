# Azrng.AspNetCore.Inject 架构与原理说明

## 1. 项目概述

`Azrng.AspNetCore.Inject` 是一个基于 ASP.NET Core 的模块化自动依赖注入库，通过模块化的方式管理服务的注册和依赖关系，简化依赖注入的配置过程。

### 1.1 核心目标

- **模块化设计**：将相关服务组织到模块中，便于管理和维护
- **自动依赖注入**：通过特性标记自动扫描和注册服务
- **依赖管理**：支持模块间的依赖关系声明和自动解析
- **循环依赖检测**：自动检测模块间的循环依赖关系

## 2. 核心架构

### 2.1 整体架构图

```
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│                  (用户代码 - Program.cs)                     │
└────────────────────────┬────────────────────────────────────┘
                         │
                         │ AddModule<TModule>()
                         ▼
┌─────────────────────────────────────────────────────────────┐
│              ServiceCollectionExtensions                     │
│  ┌─────────────────────────────────────────────────────┐   │
│  │            BuildModule() 构建模块树                  │   │
│  │  ┌───────────────────────────────────────────────┐  │   │
│  │  │   BuildTree() 递归构建依赖树                  │  │   │
│  │  │   + 循环依赖检测                              │  │   │
│  │  └───────────────────────────────────────────────┘  │   │
│  │  ┌───────────────────────────────────────────────┐  │   │
│  │  │  InitModuleTree() 后序遍历初始化模块           │  │   │
│  │  └───────────────────────────────────────────────┘  │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                         │
        ┌────────────────┼────────────────┐
        ▼                ▼                ▼
┌───────────────┐ ┌──────────────┐ ┌────────────────┐
│   ModuleNode  │ │  IModule     │ │ ServiceContext │
│  (模块依赖树) │ │  (模块接口)  │ │  (服务上下文)  │
└───────────────┘ └──────────────┘ └────────────────┘
        │
        ▼
┌─────────────────────────────────────────────────────────────┐
│              InitInjectService() 自动注入                   │
│  ┌─────────────────────────────────────────────────────┐   │
│  │          扫描程序集中的 [InjectOn] 特性             │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  InjectScheme.OnlyInterfaces  → 接口注入     │   │   │
│  │  │  InjectScheme.OnlyBaseClass   → 基类注入     │   │   │
│  │  │  InjectScheme.Any             → 全部注入     │   │   │
│  │  │  InjectScheme.Some            → 指定注入     │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                         │
                         ▼
                ┌────────────────┐
                │ IServiceCollection│
                │   (DI 容器)     │
                └────────────────┘
```

### 2.2 目录结构

```
Azrng.AspNetCore.Inject/
├── Attributes/
│   ├── InjectModuleAttribute.cs      # 模块依赖声明特性
│   └── InjectOnAttribute.cs          # 服务注入标记特性
├── IModule.cs                         # 模块接口定义
├── ServiceContext.cs                  # 服务上下文
├── ServiceCollectionExtensions.cs    # 核心扩展方法
├── ModuleNode.cs                      # 模块依赖树节点
└── InjectScheme.cs                    # 注入模式枚举
```

## 3. 核心组件详解

### 3.1 IModule 接口

所有模块必须实现的接口，定义了模块的契约。

```csharp
public interface IModule
{
    void ConfigureServices(ServiceContext services);
}
```

**职责**：
- 定义模块必须实现服务配置方法
- 接收 `ServiceContext` 参数，可访问 `IServiceCollection` 和 `IConfiguration`

### 3.2 ServiceContext 类

服务上下文，封装了依赖注入容器和配置信息。

```csharp
public class ServiceContext
{
    public IServiceCollection Services { get; }      // DI 容器
    public IConfiguration Configuration { get; }     // 配置信息
}
```

**设计模式**：上下文对象模式
**职责**：
- 在模块初始化过程中传递必要的上下文信息
- 避免直接传递多个参数

### 3.3 ModuleNode 类

模块依赖树的节点，用于构建和检测模块依赖关系。

```csharp
internal class ModuleNode
{
    public Type ModuleType { get; set; }              // 当前模块类型
    public ModuleNode? ParentModule { get; set; }     // 父模块（链表指针）
    public HashSet<ModuleNode>? Childs { get; set; }  // 子模块集合
}
```

**设计模式**：
- **组合模式**：树形结构表示模块依赖关系
- **链表**：通过 `ParentModule` 指针实现循环依赖检测

**核心方法**：
```csharp
public bool ContainsTree(ModuleNode childModule)
{
    if (childModule.ModuleType == ModuleType) return true;
    if (ParentModule == null) return false;
    return ParentModule.ContainsTree(childModule);  // 向上回溯
}
```

### 3.4 InjectModuleAttribute 特性

声明模块间的依赖关系。

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class InjectModuleAttribute : Attribute
{
    public Type ModuleType { get; }
}

// 泛型版本
public sealed class InjectModuleAttribute<TModule> : InjectModuleAttribute
    where TModule : IModule
{ }
```

**使用示例**：
```csharp
[InjectModule<DatabaseModule>]
[InjectModule(typeof(LoggingModule))]
public class BusinessModule : IModule
{ }
```

### 3.5 InjectOnAttribute 特性

标记需要自动注入的服务类。

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class InjectOnAttribute : Attribute
{
    public Type[]? ServicesType { get; set; }       // 手动指定服务类型
    public ServiceLifetime Lifetime { get; set; }   // 生命周期
    public InjectScheme Scheme { get; set; }        // 注入模式
    public bool Own { get; set; }                   // 是否注入自身
}
```

### 3.6 InjectScheme 枚举

定义服务的注入模式。

| 模式 | 说明 | 使用场景 |
|------|------|----------|
| `OnlyInterfaces` | 只注入实现的接口 | 最常用，仅暴露接口 |
| `OnlyBaseClass` | 只注入基类 | 继承体系的服务 |
| `Any` | 注入接口和基类 | 需要同时通过多种类型获取 |
| `Some` | 手动指定注入类型 | 精确控制注册的服务类型 |
| `None` | 不自动注入 | 仅用于模块内手动注册 |

## 4. 核心工作原理

### 4.1 模块注册流程

```
用户调用 AddModule<TModule>()
        │
        ▼
┌─────────────────────────────────────────┐
│ 1. 验证模块是否实现 IModule 接口        │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│ 2. 调用 BuildModule() 构建模块树        │
└────────────────┬────────────────────────┘
                 │
    ┌────────────┴────────────┐
    ▼                         ▼
┌─────────────────┐   ┌──────────────────────┐
│ BuildTree()     │   │ 后序遍历初始化模块    │
│ 递归构建依赖树  │   │ InitModuleTree()     │
└─────────────────┘   └──────────────────────┘
```

### 4.2 依赖树构建算法

**位置**：[ServiceCollectionExtensions.cs:84-112](ServiceCollectionExtensions.cs#L84-L112)

```csharp
private static void BuildTree(IServiceCollection services,
                              ModuleNode currentNode,
                              IEnumerable<InjectModuleAttribute> injectModules)
{
    // 1. 将当前模块注册为 Transient 服务
    services.AddTransient(currentNode.ModuleType);

    // 2. 遍历依赖的模块
    foreach (var childModule in injectModules)
    {
        // 3. 创建子节点
        var childTree = new ModuleNode
        {
            ModuleType = childModule.ModuleType,
            ParentModule = currentNode
        };

        // 4. 循环依赖检测
        if (currentNode.ContainsTree(childTree))
        {
            throw new OverflowException("检测到循环依赖引用或重复引用！");
        }

        // 5. 添加到子节点集合
        currentNode.Childs.Add(childTree);

        // 6. 递归处理子模块的依赖
        var childDependencies = childModule.ModuleType.GetCustomAttributes()
            .Where(x => x.GetType().IsSubclassOf(typeof(InjectModuleAttribute)))
            .OfType<InjectModuleAttribute>();

        BuildTree(services, childTree, childDependencies);
    }
}
```

**算法特点**：
- **深度优先构建**：递归处理每个模块的依赖
- **链表回溯检测**：通过 `ParentModule` 指针向上查找循环依赖
- **时间复杂度**：O(n²)，其中 n 是模块数量

**循环依赖检测示例**：

```
情况1：循环依赖
ModuleA → ModuleB → ModuleC → ModuleA ❌

检测过程：
1. ModuleC 添加 ModuleA 子节点时
2. ContainsTree() 从 ModuleC 向上回溯
3. ModuleC.Parent = ModuleB → 不匹配
4. ModuleB.Parent = ModuleA → 匹配！
5. 抛出异常

情况2：正常依赖
ModuleA → ModuleB → ModuleC ✓

检测过程：
1. ModuleC 添加子节点 ModuleD
2. ContainsTree() 向上回溯：C → B → A
3. 都不匹配 D，检测通过
```

### 4.3 模块初始化顺序

**位置**：[ServiceCollectionExtensions.cs:123-150](ServiceCollectionExtensions.cs#L123-L150)

使用**后序遍历**（Post-order Traversal）初始化模块，确保依赖模块先于被依赖模块初始化。

```
示例依赖树：
        StartupModule
             │
      ┌──────┴──────┐
      ▼             ▼
  ModuleA       ModuleB
      │             │
      ▼             ▼
  ModuleC       ModuleD

初始化顺序（后序遍历）：
1. ModuleC
2. ModuleA
3. ModuleD
4. ModuleB
5. StartupModule
```

**后序遍历代码**：
```csharp
private static void InitModuleTree(...)
{
    // 先处理所有子节点
    if (moduleNode.Childs != null)
    {
        foreach (var item in moduleNode.Childs)
        {
            InitModuleTree(..., item);  // 递归
        }
    }

    // 再处理当前节点
    if (!moduleTypes.Add(moduleNode.ModuleType))
        return;  // 已处理过则跳过

    var module = (IModule)serviceProvider.GetRequiredService(moduleNode.ModuleType);
    module.ConfigureServices(context);
    InitInjectService(context.Services, moduleNode.ModuleType.Assembly, injectTypes);
}
```

### 4.4 自动依赖注入机制

**位置**：[ServiceCollectionExtensions.cs:158-247](ServiceCollectionExtensions.cs#L158-L247)

#### 4.4.1 扫描过滤条件

```csharp
assembly.GetTypes().Where(x =>
    x.IsClass &&           // 必须是类
    !x.IsAbstract &&       // 非抽象类
    !x.IsNestedPublic)     // 非嵌套公共类
```

#### 4.4.2 注入模式处理

| 模式 | 实现逻辑 |
|------|----------|
| `OnlyInterfaces` | `item.GetInterfaces()` → 遍历接口注册 |
| `OnlyBaseClass` | `item.BaseType` → 注册基类关系 |
| `Any` | 同时执行接口和基类注入 |
| `Some` | 使用 `inject.ServicesType` 手动指定的类型 |
| `None` | 跳过注入 |

**代码片段**：
```csharp
// 自身注入
if (inject.Own)
{
    switch (inject.Lifetime)
    {
        case ServiceLifetime.Scoped:
            services.AddScoped(item);
            break;
        // ... 其他生命周期
    }
}

// 接口注入
if (inject.Scheme == InjectScheme.OnlyInterfaces ||
    inject.Scheme == InjectScheme.Any)
{
    var interfaces = item.GetInterfaces();
    foreach (var @interface in interfaces)
    {
        services.AddScoped(@interface, item);
    }
}

// 基类注入
if (inject.Scheme == InjectScheme.OnlyBaseClass ||
    inject.Scheme == InjectScheme.Any)
{
    services.AddScoped(item.BaseType, item);
}

// 手动指定注入
if (inject.Scheme == InjectScheme.Some)
{
    foreach (var type in inject.ServicesType)
    {
        services.AddScoped(type, item);
    }
}
```

## 5. 设计模式应用

| 设计模式 | 应用位置 | 说明 |
|----------|----------|------|
| **模块化模式** | IModule + ServiceCollectionExtensions | 将应用分解为独立模块 |
| **组合模式** | ModuleNode 树形结构 | 统一处理单个模块和模块组合 |
| **特性驱动编程** | InjectModule/InjectOn | 声明式编程风格 |
| **模板方法模式** | ServiceCollectionExtensions | 定义模块注册算法骨架 |
| **策略模式** | InjectScheme 枚举 | 不同的注入策略 |

## 6. 时序图

### 6.1 完整的模块注册和初始化流程

```
用户代码      ServiceCollectionExtensions    ModuleNode      ServiceContext      DI容器
   │                    │                       │                │                 │
   │ AddModule<T>()     │                       │                │                 │
   ├───────────────────>│                       │                │                 │
   │                    │ BuildModule()         │                │                 │
   │                    ├──────────────────────>│                │                 │
   │                    │ BuildTree()           │                │                 │
   │                    │ [递归构建依赖树]       │                │                 │
   │                    │                       │                │                 │
   │                    │ BuildServiceProvider() │                │                 │
   │                    ├───────────────────────────────────────>│                 │
   │                    │                       │                │                 │
   │                    │ InitModuleTree()      │                │                 │
   │                    │ [后序遍历]             │                │                 │
   │                    │                       │                │                 │
   │                    │ GetService(Module)    │                │                 │
   │                    ├────────────────────────────────────────>│                 │
   │                    │                       │                │                 │
   │                    │ module.ConfigureServices()              │                 │
   │                    ├──────────────────────>│                │                 │
   │                    │                       │                │                 │
   │                    │ InitInjectService()   │                │                 │
   │                    │ [扫描程序集并注册]     │                │                 │
   │                    │                       │                │                 │
   │                    │ services.AddScoped()   │                │                 │
   │                    ├────────────────────────────────────────>│                 │
   │                    │                       │                │                 │
   │<───────────────────│                       │                │                 │
```

### 6.2 循环依赖检测流程

```
BuildTree()    ModuleNode A    ModuleNode B    ModuleNode C
    │                │                │                │
    │ 创建A节点      │                │                │
    ├───────────────>│                │                │
    │                │                │                │
    │ 添加依赖 B     │                │                │
    ├───────────────────────────────>│                │
    │                │                │                │
    │ 添加依赖 C     │                │                │
    ├───────────────────────────────────────────────>│
    │                │                │                │
    │ 尝试添加 A     │                │                │
    ├───────────────────────────────────────────────>│
    │                │                │                │
    │ ContainsTree() │                │                │
    ├───────────────────────────────────────────────>│
    │                │                │                │
    │ 回溯到 B       │                │                │
    │<───────────────────────────────│                │
    │                │                │                │
    │ 回溯到 A       │                │                │
    │<───────────────│                │                │
    │                │                │                │
    │ 发现匹配！抛出异常              │                │
    │<────────────────────────────────────────────────│
```

## 7. 性能特点

### 7.1 时间复杂度

| 操作 | 复杂度 | 说明 |
|------|--------|------|
| 依赖树构建 | O(n²) | 每个模块的 ContainsTree() 可能需要遍历整条链 |
| 模块初始化 | O(n) | 后序遍历，每个节点访问一次 |
| 服务扫描 | O(m) | m 为程序集中的类型数量 |

### 7.2 空间复杂度

- **模块树**：O(n)，n 为模块数量
- **记录集合**：O(n + m)，模块类型 + 注入服务类型

### 7.3 优化建议

1. **减少模块数量**：避免创建过多细粒度模块
2. **程序集隔离**：不同模块放在不同程序集，避免重复扫描
3. **缓存考虑**：框架层面已使用 `HashSet<Type>` 避免重复注册

## 8. 使用场景

### 8.1 适用场景

- ✅ 中大型应用的模块化架构
- ✅ 需要清晰模块依赖关系的项目
- ✅ 微服务或模块化单体应用
- ✅ 插件化架构

### 8.2 不适用场景

- ❌ 小型简单应用（过度设计）
- ❌ 不需要模块化的场景
- ❌ 对启动时间极度敏感的应用

## 9. 扩展点

### 9.1 自定义注入模式

可以扩展 `InjectScheme` 枚举并在 `InitInjectService()` 中添加处理逻辑。

### 9.2 模块生命周期钩子

可以在 `IModule` 接口中添加更多生命周期方法：
- `ConfigureServices()` - 服务配置（现有）
- `OnApplicationStartup()` - 应用启动时
- `OnApplicationShutdown()` - 应用关闭时

## 10. 参考资料

- 框架设计文档：https://maomi.whuanle.cn/2.design_module.html
- ASP.NET Core 依赖注入官方文档
- 模块化设计模式

---

**文档版本**：1.0
**最后更新**：2026-02-17
