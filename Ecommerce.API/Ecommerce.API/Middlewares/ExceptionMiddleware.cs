using System.Text.Json;

namespace Ecommerce.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";

                var statusCode = ex switch
                {
                    NotFoundException => HttpStatusCode.NotFound,
                    BadRequestException => HttpStatusCode.BadRequest,
                    _ => HttpStatusCode.InternalServerError
                };

                context.Response.StatusCode = (int)statusCode;

                var response = _env.IsDevelopment()
                    ? new ApiException((int)statusCode, ex.Message, ex.StackTrace?.ToString() ?? string.Empty)
                    : new ApiException((int)statusCode, ex.Message);

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}
