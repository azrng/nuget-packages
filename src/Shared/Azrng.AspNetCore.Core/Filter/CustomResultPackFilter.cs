using Azrng.Core.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Azrng.AspNetCore.Core.Filter
{
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
            // 如果路由以这个开头就返回
            if (_ignoreRoutePrefix.Any(x => context.HttpContext.Request.Path.StartsWithSegments(x)))
            {
                return;
            }

            if (context.ActionDescriptor.FilterDescriptors.Any(x => x.Filter.GetType() == typeof(NoWrapperAttribute)))
            {
                return;
            }

            if (context.Result is EmptyResult)
            {
                context.Result = new OkObjectResult(new ResultModel(true, "成功"));
                return;
            }

            if (context.Result is FileContentResult)
            {
                return;
            }

            var resultModel = ((ObjectResult)context.Result).Value;

            if (resultModel is ResultModel)
            {
                return;
            }

            if (resultModel is ProblemDetails)
            {
                return;
            }

            context.Result = new OkObjectResult(new ResultModel<object?>(((ObjectResult)context.Result).Value, true, "success", "200"));
        }
    }

    /// <summary>
    /// 不使用全局的 ResultWrapperFilter 包装器
    /// </summary>
    public class NoWrapperAttribute : ActionFilterAttribute { }
}