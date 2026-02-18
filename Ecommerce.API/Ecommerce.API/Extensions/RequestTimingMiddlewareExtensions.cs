using Ecommerce.API.Middlewares;

namespace Ecommerce.API.Extensions
{
    public static class RequestTimingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestTimingMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<RequestTimingMiddleware>();
    }
}
