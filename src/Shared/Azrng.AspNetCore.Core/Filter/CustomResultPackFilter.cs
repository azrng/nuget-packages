using Azrng.Core.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Azrng.AspNetCore.Core.Filter;

/// <summary>
/// 自定义返回类过滤器
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class CustomResultPackFilter : Attribute, IResultFilter
{
    /// <summary>
    /// 要忽略的路由前缀
    /// </summary>
    private readonly string[] _ignoreRoutePrefix;

    public CustomResultPackFilter(string[]? ignoreRoutePrefix = null)
    {
        _ignoreRoutePrefix = ignoreRoutePrefix ?? Array.Empty<string>();
    }

    public void OnResultExecuted(ResultExecutedContext context) { }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        // 如果路由以忽略前缀开头就返回
        if (_ignoreRoutePrefix.Any(x => context.HttpContext.Request.Path.StartsWithSegments(x)))
        {
            return;
        }

        // 如果有 NoWrapperAttribute 特性就不包装
        if (context.ActionDescriptor.FilterDescriptors.Any(x => x.Filter is NoWrapperAttribute))
        {
            return;
        }

        // 处理 EmptyResult
        if (context.Result is EmptyResult)
        {
            context.Result = new OkObjectResult(new ResultModel(true, "成功"));
            return;
        }

        // 不处理文件结果
        if (context.Result is FileResult)
        {
            return;
        }

        // 不处理已包装的 ResultModel
        if (context.Result is ObjectResult objectResult)
        {
            // 如果值已经是 ResultModel 类型，不重复包装
            if (objectResult.Value is ResultModel)
            {
                return;
            }

            // 不包装 ProblemDetails
            if (objectResult.Value is ProblemDetails)
            {
                return;
            }

            // 包装其他对象结果
            context.Result = new OkObjectResult(new ResultModel<object?>(objectResult.Value, true, "success", "200"));
            return;
        }

        // 处理其他结果类型 (ContentResult, StatusCodeResult 等)
        // 对于这些类型，我们选择不包装，保持原有行为
        if (context.Result is ContentResult or StatusCodeResult or RedirectToActionResult or LocalRedirectResult)
        {
            return;
        }
    }
}

/// <summary>
/// 不使用全局的 ResultWrapperFilter 包装器
/// </summary>
public class NoWrapperAttribute : ActionFilterAttribute { }
