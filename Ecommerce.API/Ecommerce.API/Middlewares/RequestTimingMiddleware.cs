using System.Diagnostics;
using Microsoft.Extensions.Options;
using Ecommerce.API.Options;

namespace Ecommerce.API.Middlewares
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTimingMiddleware> _logger;
        private readonly RequestTimingOptions _options;

        public RequestTimingMiddleware(
            RequestDelegate next,
            ILogger<RequestTimingMiddleware> logger,
            IOptions<RequestTimingOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            string category = elapsedMs switch
            {
                var ms when ms < _options.FastThreshold => "Fast",
                var ms when ms < _options.SlowThreshold => "Moderate",
                _ => "Slow"
            };

            _ = category switch
            {
                "Fast"     => Log("FAST", context, elapsedMs),
                "Moderate" => Log("MODERATE", context, elapsedMs),
                "Slow"     => LogWarning("SLOW", context, elapsedMs),
                _          => Log("UNKNOWN", context, elapsedMs)
            };

            if (!context.Response.HasStarted)
            {
                context.Response.Headers["X-Request-Duration-ms"] = elapsedMs.ToString();
                context.Response.Headers["X-Request-Speed"] = category;
            }
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
