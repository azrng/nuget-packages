using Azrng.AspNetCore.Job.Quartz;
using Common.Core.Results;
using Microsoft.AspNetCore.Mvc;
using QuartzApi.JobSample;

namespace QuartzApi.Controllers;

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