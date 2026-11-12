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
            // 如果值已经实现统一返回接口，不重复包装
            if (objectResult.Value is IResultModel)
            {
                return;
            }

            // 不包装 ProblemDetails
            if (objectResult.Value is ProblemDetails)
            {
                return;
            }

            // 保留 action 声明的状态码（201 CreatedAtAction / 202 Accepted / StatusCode(...) 等），
            // 仅替换响应体为统一外壳；原实现强制改 200 覆盖了这些语义
            context.Result = new ObjectResult(new ResultModel<object?>(objectResult.Value, true, "success", "200"))
            {
                StatusCode = objectResult.StatusCode
            };
            return;
        }

        // ContentResult / StatusCodeResult / 重定向等结果类型不包装，保持原有行为
    }
}

/// <summary>
/// 不使用全局的 ResultWrapperFilter 包装器
/// </summary>
public class NoWrapperAttribute : ActionFilterAttribute { }
