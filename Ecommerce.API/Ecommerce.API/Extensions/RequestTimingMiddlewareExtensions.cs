using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.API.Middlewares;

namespace Ecommerce.API.Extensions
{
    public static class RequestTimingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestTimingMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<RequestTimingMiddleware>();
    }
}