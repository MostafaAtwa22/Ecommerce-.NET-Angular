using System.Diagnostics;

namespace Ecommerce.API.Middlewares
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTimingMiddleware> _logger;

        private const int FastThreshold = 300;    
        private const int SlowThreshold = 1000;  

        public RequestTimingMiddleware(
            RequestDelegate next,
            ILogger<RequestTimingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            string category = elapsedMs switch
            {
                < FastThreshold => "Fast",
                < SlowThreshold => "Moderate",
                _ => "Slow"
            };

            _ = category switch
            {
                "Fast" => Log("FAST", context, elapsedMs),
                "Moderate" => Log("MODERATE", context, elapsedMs),
                "Slow" => LogWarning("SLOW", context, elapsedMs),
                _ => Log("UNKNOWN", context, elapsedMs)
            };

            context.Response.Headers["X-Request-Duration-ms"] = elapsedMs.ToString();
            context.Response.Headers["X-Request-Speed"] = category;
        }

        private bool Log(string prefix, HttpContext context, long ms)
        {
            _logger.LogInformation($"{prefix} Request [{context.Request.Method}] {context.Request.Path} — {ms} ms");
            return true;
        }

        private bool LogWarning(string prefix, HttpContext context, long ms)
        {
            _logger.LogWarning($"{prefix} Request [{context.Request.Method}] {context.Request.Path} — {ms} ms");
            return true;
        }
    }
}
