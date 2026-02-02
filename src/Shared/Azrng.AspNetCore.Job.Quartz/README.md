# Azrng.AspNetCore.Job.Quartz

基于 Quartz.NET 的 ASP.NET Core 任务调度扩展包，提供简单易用的定时任务管理功能。

## 特性

- ✅ **简单易用**：一行代码即可集成 Quartz 调度功能
- ✅ **依赖注入支持**：完美集成 ASP.NET Core 依赖注入
- ✅ **灵活配置**：支持通过特性配置和动态调度两种方式
- ✅ **作业监听**：内置作业执行监听器，自动记录执行日志
- ✅ **历史记录**：内置作业执行历史记录功能
- ✅ **状态查询**：提供作业状态查询和统计功能
- ✅ **生命周期管理**：完整的作业创建、暂停、恢复、删除功能
- ✅ **类型安全**：使用泛型约束确保类型安全
- ✅ **多框架支持**：支持 .NET 8.0、9.0、10.0

## 快速开始

### 1. 安装 NuGet 包

```bash
dotnet add package Azrng.AspNetCore.Job.Quartz
```

### 2. 添加服务注册

在 `Program.cs` 中添加 Quartz 服务：

```csharp
// 使用默认配置
builder.Services.AddQuartzService();

// 或使用自定义配置
builder.Services.AddQuartzService(options =>
{
    options.SchedulerName = "MyScheduler";
    options.EnableJobHistory = true;
    options.JobHistoryRetentionDays = 30;
    options.EnableDetailedLogging = true;
});
```

### 3. 创建作业任务

#### 方式一：使用特性自动配置（固定任务）

```csharp
using Azrng.AspNetCore.Job.Quartz;
using Quartz;
using Microsoft.Extensions.Logging;

[JobConfig(nameof(HelloJob), "default", "0/5 * * * * ?")] // 每5秒执行一次
[DisallowConcurrentExecution] // 禁止并发执行
public class HelloJob : IJob
{
    private readonly ILogger<HelloJob> _logger;

    public HelloJob(ILogger<HelloJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"任务 [{context.JobDetail.Key.Name}] 执行于 {DateTime.Now}");
        return Task.CompletedTask;
    }
}
```

#### 方式二：通过服务动态调度（按需任务）

```csharp
// 定义作业类
public class CustomJob : IJob
{
    private readonly ILogger<CustomJob> _logger;

    public CustomJob(ILogger<CustomJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"自定义任务执行于 {DateTime.Now}");
        return Task.CompletedTask;
    }
}

// 通过 API 控制调度
[ApiController]
[Route("[controller]/[action]")]
public class JobController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobController(IJobService jobService)
    {
        _jobService = jobService;
    }

    /// <summary>
    /// 启动单次任务
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> StartOneJob()
    {
        await _jobService.StartJobAsync<CustomJob>(
            "自定义任务",
            DateTime.Now.AddSeconds(5));

        return Ok("任务已创建");
    }

    /// <summary>
    /// 启动定时任务（Cron表达式）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> StartCronJob()
    {
        await _jobService.StartCronJobAsync<CustomJob>(
            "定时任务",
            "0/10 * * * * ?", // 每10秒执行一次
            "default",
            new Dictionary<string, object>
            {
                { "CustomData", "测试数据" }
            });

        return Ok("定时任务已创建");
    }

    /// <summary>
    /// 暂停任务
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> PauseJob()
    {
        await _jobService.PauseJobAsync(nameof(HelloJob));
        return Ok("任务已暂停");
    }

    /// <summary>
    /// 恢复任务
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ResumeJob()
    {
        await _jobService.ResumeJobAsync(nameof(HelloJob));
        return Ok("任务已恢复");
    }

    /// <summary>
    /// 立即执行任务
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExecuteJob()
    {
        await _jobService.ExecuteJobAsync(nameof(HelloJob));
        return Ok("任务已触发执行");
    }

    /// <summary>
    /// 删除任务
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DeleteJob()
    {
        await _jobService.DeleteJobAsync(nameof(HelloJob));
        return Ok("任务已删除");
    }
}
```

## 配置选项

```csharp
builder.Services.AddQuartzService(options =>
{
    // 基础配置
    options.SchedulerName = "MyScheduler";
    options.SchedulerId = "AUTO";
    options.StartOnApplicationStart = true;
    options.WaitForJobsToCompleteOnShutdown = true;

    // 并发配置
    options.MaxBatchSize = 1;
    options.BatchTriggerAcquisitionFireAheadTimeWindow = 0;
    options.JobScanInterval = 1000;

    // 持久化配置（可选）
    options.EnableJobPersistence = false;
    options.Persistence = new PersistenceOptions
    {
        ConnectionString = "your-connection-string",
        DatabaseProviderType = DatabaseProviderType.SqlServer,
        TablePrefix = "QRTZ_",
        CreateTablesOnStartup = false
    };

    // 历史记录配置
    options.EnableJobHistory = true;
    options.JobHistoryRetentionDays = 30;

    // 监听器配置
    options.EnableJobListener = true;
    options.EnableTriggerListener = true;
    options.EnableSchedulerListener = true;

    // 其他配置
    options.MisfirePolicy = MisfirePolicy.ExecuteOnce;
    options.EnableDetailedLogging = true;
    options.JobTimeoutSeconds = 0; // 0表示不限制
});
```

## 高级功能

### 作业状态查询

```csharp
public class JobStatusController : ControllerBase
{
    private readonly IJobStatusService _statusService;

    public JobStatusController(IJobStatusService statusService)
    {
        _statusService = statusService;
    }

    /// <summary>
    /// 获取正在运行的作业
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRunningJobs()
    {
        var runningJobs = await _statusService.GetRunningJobsAsync();
        return Ok(runningJobs);
    }

    /// <summary>
    /// 获取所有已调度的作业
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetScheduledJobs()
    {
        var scheduledJobs = await _statusService.GetScheduledJobsAsync();
        return Ok(scheduledJobs);
    }

    /// <summary>
    /// 检查作业是否正在运行
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> IsJobRunning(string jobName, string jobGroup = "default")
    {
        var isRunning = await _statusService.IsJobRunningAsync(jobName, jobGroup);
        return Ok(new { IsRunning = isRunning });
    }

    /// <summary>
    /// 获取作业详情
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetJobDetail(string jobName, string jobGroup = "default")
    {
        var detail = await _statusService.GetJobDetailAsync(jobName, jobGroup);
        return Ok(detail);
    }
}
```

### 作业执行历史

```csharp
public class JobHistoryController : ControllerBase
{
    private readonly IJobExecutionHistoryService _historyService;

    public JobHistoryController(IJobExecutionHistoryService historyService)
    {
        _historyService = historyService;
    }

    /// <summary>
    /// 获取作业执行历史
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHistory(string jobName, string jobGroup = "default", int pageIndex = 1, int pageSize = 20)
    {
        var (histories, totalCount) = await _historyService.GetHistoryAsync(jobName, jobGroup, pageIndex, pageSize);
        return Ok(new { Histories = histories, TotalCount = totalCount });
    }

    /// <summary>
    /// 获取最近的执行历史
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRecentHistory(int count = 50)
    {
        var recentHistory = await _historyService.GetRecentHistoryAsync(count);
        return Ok(recentHistory);
    }

    /// <summary>
    /// 获取作业执行统计
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStatistics(string jobName, string jobGroup = "default")
    {
        var statistics = await _historyService.GetStatisticsAsync(jobName, jobGroup);
        return Ok(statistics);
    }
}
```

## Cron 表达式示例

```
0 0 2 * * ?          # 每天凌晨2点执行
0/5 * * * * ?        # 每5秒执行一次
0 0/5 * * * ?        # 每5分钟执行一次
0 0 12 * * ?         # 每天中午12点执行
0 15 10 * * ?        # 每天上午10:15执行
0 15 10 * * ? 2025    # 2025年的每天上午10:15执行
0 0-5 14 * * ?       # 每天下午2点到2:05每分钟执行一次
0 10,44 14 ? 3 WED   # 每年3月的星期三下午2:10和2:44执行
```

## API 文档

### IJobService - 作业管理服务

| 方法 | 说明 |
|------|------|
| `StartJobAsync<T>()` | 启动单次任务 |
| `StartCronJobAsync<T>()` | 启动定时任务（Cron表达式） |
| `PauseJobAsync()` | 暂停任务 |
| `ResumeJobAsync()` | 恢复任务 |
| `InterruptJobAsync()` | 中断正在运行的任务 |
| `ExecuteJobAsync()` | 立即执行任务 |
| `DeleteJobAsync()` | 删除任务 |

### IJobStatusService - 作业状态服务

| 方法 | 说明 |
|------|------|
| `GetRunningJobsAsync()` | 获取正在运行的作业 |
| `GetScheduledJobsAsync()` | 获取所有已调度的作业 |
| `IsJobRunningAsync()` | 检查作业是否正在运行 |
| `GetNextFireTimeAsync()` | 获取作业下次执行时间 |
| `GetJobDetailAsync()` | 获取作业详情 |

### IJobExecutionHistoryService - 作业历史服务

| 方法 | 说明 |
|------|------|
| `GetHistoryAsync()` | 获取作业执行历史 |
| `GetRecentHistoryAsync()` | 获取最近的执行历史 |
| `GetStatisticsAsync()` | 获取作业执行统计 |
| `CleanupExpiredHistoryAsync()` | 清理过期历史记录 |

## 注意事项

1. **作业类必须是公共类**：只有 public 类才能被扫描和注册
2. **支持依赖注入**：作业类可以注入其他服务（需要注册为 Scoped 或 Singleton）
3. **Cron表达式验证**：确保 Cron 表达式格式正确
4. **异常处理**：建议在作业的 Execute 方法中添加异常处理逻辑
5. **并发控制**：使用 `[DisallowConcurrentExecution]` 特性防止同一作业并发执行

## 版本历史

- **1.0.0-beta1** - 基础功能发布
- **1.1.0-beta1** - 添加配置选项、作业监听器、历史记录、状态查询功能

## 许可证

版权归 Azrng 所有

## 贡献

欢迎提交 Issue 和 Pull Request！
