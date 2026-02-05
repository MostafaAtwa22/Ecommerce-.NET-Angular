using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ecommerce.API.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InvalidateCacheAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _pattern;

        public InvalidateCacheAttribute(string pattern)
        {
            _pattern = pattern;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next();

            var isSuccess = executedContext.Result switch
            {
                ObjectResult { StatusCode: >= 200 and < 300 } => true,
                ObjectResult { StatusCode: null } => true, 
                StatusCodeResult { StatusCode: >= 200 and < 300 } => true,
                _ => false
            };

            if (isSuccess)
            {
                var cacheService = context.HttpContext.RequestServices
                    .GetRequiredService<IResponseCacheService>();
                await cacheService.RemoveCacheByPatternAsync(_pattern);
            }
        }
    }
}
