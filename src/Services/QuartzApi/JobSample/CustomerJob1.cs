using Quartz;

namespace QuartzApi.JobSample;

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