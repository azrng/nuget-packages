using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CommonCollect.Validator.Aop
{
    public class ValidatorFilter : IAsyncActionFilter, IFilterMetadata
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments != null && !context.ActionArguments.FirstOrDefault().Value.IsValid(out var msg))
            {
                throw new ArgumentException(msg);
            }
            await next();
        }
    }
}
