# Azrng.AspNetCore.Job.Quartz 架构与原理说明

## 目录

- [项目概述](#项目概述)
- [架构设计](#架构设计)
- [核心组件](#核心组件)
- [工作原理](#工作原理)
- [依赖注入流程](#依赖注入流程)
- [作业生命周期](#作业生命周期)
- [扩展点](#扩展点)
- [时序图](#时序图)

---

## 项目概述

`Azrng.AspNetCore.Job.Quartz` 是基于 Quartz.NET 的 ASP.NET Core 任务调度扩展包，旨在提供简单、易用且功能完整的定时任务管理解决方案。

### 设计目标

1. **零配置启动** - 一行代码即可集成 Quartz 调度功能
2. **原生 DI 集成** - 完美集成 ASP.NET Core 依赖注入容器
3. **双重调度模式** - 支持特性自动配置和动态 API 调度两种方式
4. **可观测性** - 内置作业监听、执行历史和状态查询
5. **生命周期管理** - 完整的作业创建、暂停、恢复、删除功能

---

## 架构设计

### 分层架构

```
┌─────────────────────────────────────────────────────────────┐
│                        应用层 (API)                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  IJobService │  │ IJobStatus   │  │ IJobHistory  │      │
│  │              │  │   Service    │  │   Service    │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                       调度层 (Core)                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ JobHosted    │  │   Quartz     │  │   Dependency │      │
│  │   Service    │  │  Scheduler   │  │InjectionJob  │      │
│  │              │  │              │  │   Factory    │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
│  ┌──────────────────────────────────────────────────┐      │
│  │         QuartzJobListener (监听器)                 │      │
│  └──────────────────────────────────────────────────┘      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      用户代码层 (Jobs)                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   HelloJob   │  │  CustomJob   │  │    ...       │      │
│  │  (IJob)      │  │   (IJob)     │  │              │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
```

### 目录结构

```
Azrng.AspNetCore.Job.Quartz/
├── Options/                          # 配置选项
│   └── QuartzOptions.cs              # Quartz 配置选项类
├── Schedules/                        # 调度核心
│   ├── JobHostedService.cs           # 托管服务实现
│   └── DependencyInjectionJobFactory.cs  # DI 作业工厂
├── Listeners/                        # 监听器
│   └── QuartzJobListener.cs          # 作业执行监听器
├── Services/                         # 应用服务
│   ├── JobService.cs                 # 作业管理服务
│   ├── JobStatusService.cs           # 状态查询服务
│   └── InMemoryJobExecutionHistoryService.cs  # 历史记录服务
├── Model/                            # 数据模型
│   ├── JobExecutionHistory.cs        # 执行历史模型
│   └── ScheduleViewModel.cs          # 调度视图模型
├── IJobService.cs                    # 作业服务接口
├── ITriggerService.cs                # 触发器服务接口
├── ISchedulerService.cs              # 调度器服务接口
├── JobConfigAttribute.cs             # 作业配置特性
└── ServiceCollectionExtensions.cs    # DI 注册扩展
```

---

## 核心组件

### 1. ServiceCollectionExtensions (服务注册入口)

**职责**: 服务注册和 DI 容器配置

**核心代码**: [ServiceCollectionExtensions.cs:28](ServiceCollectionExtensions.cs#L28-L59)

```csharp
public static IServiceCollection AddQuartzService(
    this IServiceCollection services,
    Action<QuartzOptions>? configure = null,
    params Assembly[] assemblies)
```

**注册内容**:
- `ISchedulerFactory` - Quartz 调度器工厂
- `IJobFactory` -> `DependencyInjectionJobFactory` - 自定义 DI 工厂
- `JobHostedService` - 后台托管服务
- `IJobExecutionHistoryService` -> `InMemoryJobExecutionHistoryService` - 历史服务
- `IJobService` / `ITriggerService` / `ISchedulerService` / `IJobStatusService` - 应用服务
- 所有 `IJob` 实现类（Scoped 生命周期）

### 2. JobHostedService (后台托管服务)

**职责**: Quartz 调度器生命周期管理和自动作业注册

**核心代码**: [JobHostedService.cs:34](Schedules/JobHostedService.cs#L34-L119)

**启动流程**:
1. 获取并配置 Quartz 调度器
2. 设置自定义 JobFactory 支持 DI
3. 启动调度器
4. 扫描程序集查找 `IJob` 实现
5. 自动注册带有 `[JobConfig]` 特性的作业

**程序集扫描策略**:
- 优先级1: 配置的 `AssemblyNamesToScan`
- 优先级2: `ScanAllLoadedAssemblies` = true 时扫描所有已加载程序集
- 优先级3: 默认扫描入口程序集和调用程序集
- 应用 `ExcludedAssemblyPatterns` 排除规则

### 3. DependencyInjectionJobFactory (DI 作业工厂)

**职责**: 为每个作业创建独立的 DI Scope

**核心代码**: [DependencyInjectionJobFactory.cs:21](Schedules/DependencyInjectionJobFactory.cs#L21-L36)

```csharp
public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
{
    var scope = _serviceProvider.CreateScope();
    var jobKey = bundle.JobDetail.Key.ToString();
    _scopes.TryAdd(jobKey, scope);
    return scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
}
```

**设计要点**:
- 每个作业执行创建独立的 `IServiceScope`
- 支持 Scoped 服务注入到作业中
- 跟踪所有 Scope 以便清理

### 4. QuartzJobListener (作业监听器)

**职责**: 监听作业执行生命周期，记录日志和历史

**核心代码**: [QuartzJobListener.cs:31](Listeners/QuartzJobListener.cs#L31-L122)

**监听事件**:

| 事件 | 说明 |
|------|------|
| `JobToBeExecuted` | 作业即将执行 - 记录开始日志 |
| `JobExecutionVetoed` | 作业执行被拒绝 - 记录警告 |
| `JobWasExecuted` | 作业已执行 - 记录结果、保存历史 |

### 5. JobService (作业管理服务)

**职责**: 提供作业 CRUD 操作 API

**核心代码**: [IJobService.cs:19](IJobService.cs#L19-L74)

**方法**:
- `StartJobAsync<T>` - 创建单次任务
- `StartCronJobAsync<T>` - 创建定时任务（Cron 表达式）
- `PauseJobAsync` - 暂停任务
- `ResumeJobAsync` - 恢复任务
- `InterruptJobAsync` - 中断正在运行的任务
- `ExecuteJobAsync` - 立即触发执行
- `DeleteJobAsync` - 删除任务

### 6. JobStatusService (状态查询服务)

**职责**: 查询作业运行状态和详细信息

**核心代码**: [JobStatusService.cs:23](Services/JobStatusService.cs#L23-L48)

**方法**:
- `GetRunningJobsAsync` - 获取正在运行的作业
- `GetScheduledJobsAsync` - 获取所有已调度作业
- `IsJobRunningAsync` - 检查作业是否正在运行
- `GetJobDetailAsync` - 获取作业详情
- `GetNextFireTimeAsync` - 获取下次执行时间

### 7. JobConfigAttribute (作业配置特性)

**职责**: 声明式配置作业自动注册

**核心代码**: [JobConfigAttribute.cs:24](JobConfigAttribute.cs#L24-L42)

```csharp
[JobConfig(nameof(HelloJob), "default", "0/5 * * * * ?")]
public class HelloJob : IJob { }
```

**参数**:
- `name` - 作业名称
- `group` - 作业分组
- `cronExpression` - Cron 表达式

---

## 工作原理

### 启动流程

```
┌─────────────────┐
│  应用启动        │
│  Program.cs     │
└────────┬────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│  AddQuartzService()                      │
│  - 注册所有服务到 DI 容器                 │
│  - 注册 IJob 实现类                      │
│  - 注册 JobHostedService                 │
└────────┬────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│  JobHostedService.StartAsync()           │
│  - 获取 Scheduler                        │
│  - 设置 JobFactory                       │
│  - 启动 Scheduler                        │
└────────┬────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│  扫描程序集                              │
│  - 查找 IJob 实现类                      │
│  - 应用排除规则                          │
└────────┬────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│  自动注册 [JobConfig] 作业               │
│  - 创建 JobDetail                        │
│  - 创建 Trigger                          │
│  - 调度到 Scheduler                      │
└────────┬────────────────────────────────┘
         │
         ▼
┌─────────────────┐
│  调度器运行中    │
└─────────────────┘
```

### 作业执行流程

``┌──────────────────┐
│  Trigger 触发      │
└────────┬─────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│  DependencyInjectionJobFactory.NewJob()  │
│  - 创建 IServiceScope                    │
│  - 从 Scope 解析 Job 实例                │
└────────┬────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│  QuartzJobListener.JobToBeExecuted()     │
│  - 记录开始日志                          │
└────────┬────────────────────────────────┘
         │
         ▼
┌──────────────────┐
│  IJob.Execute()  │
│  - 执行用户代码   │
└────────┬─────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│  QuartzJobListener.JobWasExecuted()      │
│  - 记录执行日志                          │
│  - 保存历史记录                          │
└────────┬────────────────────────────────┘
         │
         ▼
┌──────────────────┐
│  Dispose Scope   │
└──────────────────┘
```

### 动态调度流程

```
用户代码
   │
   ▼ 调用 IJobService.StartCronJobAsync<T>()
   │
   ▼ JobService
   │  - 创建 JobBuilder
   │  - 创建 TriggerBuilder (WithCronSchedule)
   │  - 调用 scheduler.ScheduleJob()
   │
   ▼ Quartz Scheduler
   │  - 存储 JobDetail
   │  - 存储 Trigger
   │  - 计算下次触发时间
   │
   ▼ 等待触发...
```

---

## 依赖注入流程

### 服务注册层次

```csharp
// Singleton 层
services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
services.AddSingleton<IJobFactory, DependencyInjectionJobFactory>();
services.AddSingleton<IJobExecutionHistoryService, InMemoryJobExecutionHistoryService>();

// Hosted Service
services.AddHostedService<JobHostedService>();

// Scoped 层
services.AddScoped<IJobService, JobService>();
services.AddScoped<ITriggerService, TriggerService>();
services.AddScoped<ISchedulerService, SchedulerService>();
services.AddScoped<IJobStatusService, JobStatusService>();

// 所有 IJob 实现类 (Scoped)
services.AddScoped(typeof(HelloJob));
services.AddScoped(typeof(CustomJob));
// ...
```

### Scope 创建与销毁

```
Root Container (Singleton)
    │
    ├── SchedulerFactory (Singleton)
    ├── JobFactory (Singleton)
    │
    └── 当 Job 被触发时:
        │
        ▼ 创建 Scope 1
        ├── IJobService (Scoped)
        ├── HelloJob (Scoped)
        └── [其他注入的服务]
        │
        ▼ 执行完成后
        Dispose Scope 1
```

---

## 作业生命周期

### 状态转换

```
┌───────┐     ScheduleJob()     ┌──────────┐
│ None  │ ──────────────────▶  │ Scheduled │
└───────┘                       └──────────┘
                                    │
                    ┌───────────────┼───────────────┐
                    │               │               │
                    ▼               ▼               ▼
              PauseJob()    Trigger Fired    DeleteJob()
                    │               │               │
                    ▼               ▼               ▼
              ┌──────────┐    ┌─────────┐    ┌───────┐
              │  Paused  │    │Running  │    │ None  │
              └──────────┘    └─────────┘    └───────┘
                    │               │
                    │               ▼
                    │         ┌──────────┐
                    │         │Complete  │
                    │         └──────────┘
                    │               │
                    └───────────────┤
                                    │
                                    ▼
                              ┌──────────┐
                              │ Scheduled│
                              └──────────┘
```

### 作业组概念

- **JobGroup**: 作业的逻辑分组（如 "default", "reports", "maintenance"）
- **JobKey**: 唯一标识一个作业（Name + Group）
- **TriggerKey**: 唯一标识一个触发器（Name + Group）

---

## 扩展点

### 1. 自定义历史存储

默认使用内存存储，可实现 `IJobExecutionHistoryService` 接口持久化到数据库：

```csharp
public class DatabaseJobHistoryService : IJobExecutionHistoryService
{
    public async Task AddHistoryAsync(JobExecutionHistory history, CancellationToken cancellationToken = default)
    {
        // 保存到数据库
        await _dbContext.JobHistories.AddAsync(history, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // ... 其他方法实现
}

// 注册
services.AddSingleton<IJobExecutionHistoryService, DatabaseJobHistoryService>();
```

### 2. 自定义监听器

实现 Quartz 的 `IJobListener` 接口：

```csharp
public class CustomJobListener : IJobListener
{
    public string Name => "CustomJobListener";

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        // 自定义逻辑：发送通知、记录指标等
        return Task.CompletedTask;
    }

    // ... 其他方法
}

// 在 JobHostedService 启动后添加监听器
await _scheduler.ListenerManager.AddJobListener(new CustomJobListener());
```

### 3. 作业并发控制

使用 Quartz 内置特性：

```csharp
[DisallowConcurrentExecution]  // 禁止并发执行
[PersistJobDataAfterExecution] // 执行后持久化 JobDataMap
public class MyJob : IJob { }
```

### 4. 作业中断支持

实现 `IInterruptableJob` 接口：

```csharp
public class InterruptableJob : IJob, IInterruptableJob
{
    private volatile bool _interrupted = false;

    public void Interrupt()
    {
        _interrupted = true;
    }

    public Task Execute(IJobExecutionContext context)
    {
        while (!_interrupted && /* 有工作要做 */)
        {
            // 处理逻辑
        }

        if (_interrupted)
        {
            // 清理资源
        }

        return Task.CompletedTask;
    }
}
```

---

## 时序图

### 应用启动时序

``┌─────────┐    ┌─────────────────┐    ┌──────────────────┐    ┌─────────────┐
│ ASP.NET  │    │ ServiceCollection│    │ JobHostedService │    │   Quartz    │
│  Core    │    │   Extensions    │    │                  │    │  Scheduler  │
└────┬────┘    └────────┬─────────┘    └────────┬─────────┘    └──────┬──────┘
     │                  │                        │                     │
     │ AddQuartzService │                        │                     │
     │─────────────────>│                        │                     │
     │                  │                        │                     │
     │                  │ Register services      │                     │
     │                  │───────────────────────>│                     │
     │                  │                        │                     │
     │                  │                        │   StartAsync()      │
     │                  │                        │────────────────────>│
     │                  │                        │                     │
     │                  │                        │   GetScheduler()    │
     │                  │                        │<────────────────────│
     │                  │                        │                     │
     │                  │                        │   Set JobFactory    │
     │                  │                        │────────────────────>│
     │                  │                        │                     │
     │                  │                        │   Start()           │
     │                  │                        │────────────────────>│
     │                  │                        │                     │
     │                  │                        │   Scan assemblies   │
     │                  │                        │<────────────────────│
     │                  │                        │                     │
     │                  │                        │   ScheduleJob()     │
     │                  │                        │────────────────────>│
     │                  │                        │                     │
     ▼                  ▼                        ▼                     ▼
```

### 作业执行时序

``┌─────────────┐    ┌──────────────────┐    ┌─────────────┐    ┌─────────────┐
│   Quartz    │    │    JobFactory    │    │   Listener   │    │     Job     │
│  Scheduler  │    │                  │    │              │    │  Instance   │
└──────┬──────┘    └────────┬─────────┘    └──────┬──────┘    └──────┬──────┘
       │                     │                     │                    │
       │  Trigger Fired      │                     │                    │
       │────────────────────────────────────────────────────────────────>│
       │                     │                     │                    │
       │  NewJob()           │                     │                    │
       │────────────────────>│                     │                    │
       │                     │                     │                    │
       │                     │ Create Scope        │                    │
       │                     │─────────────────────────────────────────>│
       │                     │                     │                    │
       │                     │ Resolve Job         │                    │
       │                     │<─────────────────────────────────────────│
       │                     │                     │                    │
       │  Return Job         │                     │                    │
       │<────────────────────│                     │                    │
       │                     │                     │                    │
       │                     │  JobToBeExecuted    │                    │
       │                     │────────────────────>│                    │
       │                     │                     │                    │
       │  Execute()          │                     │                    │
       │────────────────────────────────────────────────────────────────>│
       │                     │                     │                    │
       │                     │                     │                    │ [Execute Logic]
       │                     │                     │                    │
       │  Return             │                     │                    │
       │<───────────────────────────────────────────────────────────────│
       │                     │                     │                    │
       │                     │  JobWasExecuted     │                    │
       │                     │────────────────────>│                    │
       │                     │                     │                    │
       │                     │ Dispose Scope       │                    │
       │                     │─────────────────────────────────────────>│
       ▼                     ▼                     ▼                    ▼
```

---

## 关键设计决策

### 为什么使用 Scoped 生命周期注册 Job？

- **支持 Scoped 服务注入**: 允许作业注入 DbContext、Scoped 仓储等
- **资源隔离**: 每次执行创建新实例，避免状态污染
- **自动清理**: 执行完成后自动释放 Scope 及其内部服务

### 为什么需要自定义 JobFactory？

Quartz 默认的 `SimpleJobFactory` 使用 `Activator.CreateInstance`，无法：
- 支持 DI 容器注入
- 管理 Scoped 生命周期
- 自动释放资源

`DependencyInjectionJobFactory` 解决了这些问题。

### 为什么使用内存存储历史记录？

- **简单快速**: 无需额外数据库依赖
- **适合轻量场景**: 对于大多数应用，内存存储足够
- **可扩展**: 提供接口，可轻松替换为数据库实现

---

## 性能考虑

### 并发执行

- 默认允许同一作业的多个实例并发执行
- 使用 `[DisallowConcurrentExecution]` 禁用并发

### 内存管理

- 历史记录无限增长，建议定期清理：
  ```csharp
  await _historyService.CleanupExpiredHistoryAsync(retentionDays: 30);
  ```

### 程序集扫描

- 避免扫描所有已加载程序集（`ScanAllLoadedAssemblies = false`）
- 使用 `AssemblyNamesToScan` 指定程序集
- 使用 `ExcludedAssemblyPatterns` 排除系统程序集
