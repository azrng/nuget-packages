using Azrng.AspNetCore.Job.Quartz;
using Common.Core.Results;
using Microsoft.AspNetCore.Mvc;
using QuartzApi.JobSample;

namespace QuartzApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SchedulerController : ControllerBase
{
    private readonly ISchedulerService _scheduler;

    public SchedulerController(ISchedulerService scheduler)
    {
        _scheduler = scheduler;
    }

    [HttpGet]
    public async Task<IResultModel<string>> Start()
    {
        await _scheduler.StartAsync();
        return ResultModel<string>.Success("ok");
    }

    [HttpGet]
    public async Task<IResultModel<string>> Stop()
    {
        await _scheduler.StopAsync();
        return ResultModel<string>.Success("ok");
    }
}