using Azrng.AspNetCore.Core.Filter;
using Azrng.Core.Results;
using Azrng.SettingConfig.Dto;
using Azrng.SettingConfig.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiSettingConfig.Controllers;

[Route("api/[Controller]/[action]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;
    private readonly IConfigExternalProvideService _configExternalProvideService;

    public TestController(ILogger<TestController> logger, IConfigExternalProvideService configExternalProvideService)
    {
        _logger = logger;
        _configExternalProvideService = configExternalProvideService;
    }

    [HttpPost]
    public string GetInfo(Userinfo userinfo)
    {
        _logger.LogInformation("aaa");
        //var name = AppSettings.GetValue("Service:Name");
        return "succ";
    }

    [HttpGet]
    [NoWrapper]
    [Authorize]
    public IResultModel<string> GetResult()
    {
        var bb = ResultModel.Success();
        return ResultModel<string>.Success("bb");
    }

    [HttpGet]
    public async Task<string> Init()
    {
        await _configExternalProvideService.AddIfNotExistsAsync(new List<AddSettingInfoDto>()
        {
            new AddSettingInfoDto()
            {
                Key = "test",
                Name = "test",
                Value = "test",
                Description = "test",
                Version = "1.0.0"
            },
            new AddSettingInfoDto()
            {
                Key = "test2",
                Name = "test2",
                Value = "test2",
                Description = "test2",
                Version = "1.0.0"
            },
            new AddSettingInfoDto()
            {
                Key = "test3",
                Name = "test3",
                Value = "test3",
                Description = "test3",
                Version = "1.0.0"
            },
            new AddSettingInfoDto()
            {
                Key = "test4",
                Name = "test4",
                Value = "test4",
                Description = "test4",
                Version = "1.0.0"
            },
            new AddSettingInfoDto()
            {
                Key = "test5",
                Name = "test5",
                Value = "test5",
                Description = "test5",
                Version = "1.0.0"
            },
            new AddSettingInfoDto()
            {
                Key = "test6",
                Name = "test6",
                Value = "test6",
                Description = "test6",
                Version = "1.0.0"
            },
            new AddSettingInfoDto()
            {
                Key = "test7",
                Name = "test7",
                Value = "test7",
                Description = "test7",
                Version = "1.0.0"
            },
            new AddSettingInfoDto()
            {
                Key = "test8",
                Name = "test8",
                Value = "test8",
                Description = "test8",
                Version = "1.0.0"
            },
        });
        return "success";
    }
}

public class Userinfo
{
    //[Required]
    //[MinLength(5)]
    public string Id { get; set; }

    //[MinLength(6)]
    public string Name { get; set; }
}

public class UserDto
{
    public string UserId { get; set; }

    public string Name { get; set; }

    public string Sex { get; set; }
}