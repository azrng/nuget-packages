using Azrng.AspNetCore.Job.Quartz;
using Quartz;

namespace QuartzApi.JobSample;

[JobConfig(nameof(HelloJob), "default", "0/5 * * * * ?")]
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