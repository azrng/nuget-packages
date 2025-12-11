# Azrng.Job.QuartzJob
基于QuartzJob的扩展包

## 快速上手

引用Nuget包，然后注入服务

```
builder.Services.AddQuartzService();
```



#### 创建简单Job

```csharp
[JobConfig(nameof(HelloJob), "default", "0/5 * * * * ?")] // 指定名称 分组 以及调度周期cron
[DisallowConcurrentExecution]
public class HelloJob : IJob
{
    private readonly ILogger<HelloJob> _logger;

    public HelloJob(ILogger<HelloJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"job name：{context.JobDetail.Key.Name} 分组：{context.JobDetail.Key.Group} 执行 {DateTime.Now}");
        return Task.CompletedTask;
    }
}
```

### 创建不默认执行的Job

```csharp
public class CustomerJob1 : IJob
{
    private readonly ILogger<CustomerJob1> _logger;

    public CustomerJob1(ILogger<CustomerJob1> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"自定义job：{context.JobDetail.Key.Name} 执行 {DateTime.Now}");
        return Task.CompletedTask;
    }
}
```

可以通过注入接口IJobService去进行调度

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class JobController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpGet]
    public async Task<IResultModel<string>> StartOneJob()
    {
        await _jobService.StartJobAsync<CustomerJob1>("自定义job", DateTime.Now.AddSeconds(5));
        return ResultModel<string>.Success("ok");
    }

    [HttpGet]
    public async Task<IResultModel<string>> PauseOneJob()
    {
        await _jobService.PauseJobAsync(nameof(HelloJob));
        return ResultModel<string>.Success("ok");
    }

    [HttpGet]
    public async Task<IResultModel<string>> ResumeOneJob()
    {
        await _jobService.ResumeJobAsync(nameof(HelloJob));
        return ResultModel<string>.Success("ok");
    }
}
```

## 版本记录

* 1.0.0-beta1
  * 基础操作