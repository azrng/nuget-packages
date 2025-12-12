using Azrng.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace APIStudy.Controllers;

public class TestController : BaseController
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public string GetInfo(Userinfo userinfo)
    {
        _logger.LogInformation("aaa");
        //var name = AppSettings.GetValue("Service:Name");
        return "success"+ userinfo.Name;
    }

    [HttpGet]
    public IResultModel<string> GetResult()
    {
        var bb = ResultModel.Success();
        return ResultModel<string>.Success("bb");
    }

    [HttpGet]
    public IResultModel<string> Failure()
    {
        return ResultModel<string>.Failure("失败");
    }

    [HttpGet]
    public IResultModel<string> ThrowError()
    {
        throw new ParameterException("操作异常");
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