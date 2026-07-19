using Azrng.Core.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Azrng.AspNetCore.Core.Filter;

/// <summary>
/// 模型验证Action过滤器
/// </summary>
public class ModelVerifyFilter : ActionFilterAttribute
{
    private readonly CommonMvcConfig _config;

    /// <summary>
    /// 模型校验过滤器
    /// </summary>
    public ModelVerifyFilter(IOptions<CommonMvcConfig>? config = null)
    {
        // 通过 IOptions 注入，使 Configure<CommonMvcConfig> 设置的选项真正生效
        _config = config?.Value ?? new CommonMvcConfig();
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
            return;

        var errorResults = new List<ErrorInfo>();
        foreach (var (key, value) in context.ModelState)
        {
            var result = new ErrorInfo { Field = key, };
            foreach (var error in value.Errors)
            {
                if (!string.IsNullOrEmpty(result.Message))
                {
                    result.Message += '|';
                }

                result.Message += error.ErrorMessage;
            }

            errorResults.Add(result);
        }

        if (_config.UseHttpStateCode)
        {
            context.HttpContext.Response.StatusCode = 400;
            context.Result =
                new BadRequestObjectResult(new ResultModel(false, "参数格式不正确", StatusCodes.Status400BadRequest.ToString(), errorResults));
            return;
        }

        context.Result = new ObjectResult(new ResultModel(false, "参数格式不正确", StatusCodes.Status400BadRequest.ToString(), errorResults));
    }

    public override void OnActionExecuted(ActionExecutedContext context) { }
}