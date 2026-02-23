using System.Threading.RateLimiting;
using Ecommerce.API.Options;

namespace Ecommerce.API.Extensions
{
    public static class RateLimiterServiceExtensions
    {
        public static IServiceCollection AddCustomRateLimiting(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<RateLimitOptions>(configuration.GetSection(RateLimitOptions.SectionName));

            services.AddRateLimiter(options =>
            {
                var rateLimitConfig = configuration
                    .GetSection(RateLimitOptions.SectionName)
                    .Get<RateLimitOptions>() ?? new RateLimitOptions();

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)retryAfter.TotalSeconds).ToString();
                    }

                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        statusCode = 429,
                        message = "Too many requests. Please try again later.",
                        retryAfter = retryAfter.TotalSeconds
                    }, cancellationToken);
                };

                options.AddFixedWindowLimiter("customer-login", config =>
                {
                    config.PermitLimit = rateLimitConfig.Authentication.PermitLimit;
                    config.Window = TimeSpan.FromMinutes(rateLimitConfig.Authentication.WindowMinutes);
                    config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    config.QueueLimit = rateLimitConfig.Authentication.QueueLimit;
                });

                options.AddFixedWindowLimiter("customer-register", config =>
                {
                    config.PermitLimit = rateLimitConfig.CustomerRegister.PermitLimit;
                    config.Window = TimeSpan.FromMinutes(rateLimitConfig.CustomerRegister.WindowMinutes);
                    config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    config.QueueLimit = rateLimitConfig.CustomerRegister.QueueLimit;
                });

                options.AddPolicy("customer-browsing", httpContext =>
                {
                    var customerId = httpContext.User.Identity?.Name ??
                                httpContext.Connection.RemoteIpAddress?.ToString() ??
                                "anonymous";

                    return RateLimitPartition.GetSlidingWindowLimiter(customerId, key =>
                        new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitConfig.Products.PermitLimit,
                            Window = TimeSpan.FromMinutes(rateLimitConfig.Products.WindowMinutes),
                            SegmentsPerWindow = rateLimitConfig.Products.SegmentsPerWindow,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = rateLimitConfig.Products.QueueLimit
                        });
                });

                options.AddPolicy("customer-cart", httpContext =>
                {
                    var customerId = httpContext.User.Identity?.Name ??
                                httpContext.Connection.RemoteIpAddress?.ToString() ??
                                "anonymous";

                    return RateLimitPartition.GetTokenBucketLimiter(customerId, key =>
                        new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = rateLimitConfig.CustomerCart.TokenLimit,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(rateLimitConfig.CustomerCart.ReplenishmentPeriodMinutes),
                            TokensPerPeriod = rateLimitConfig.CustomerCart.TokensPerPeriod,
                            AutoReplenishment = true,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = rateLimitConfig.CustomerCart.QueueLimit
                        });
                });

                options.AddPolicy("customer-orders", httpContext =>
                {
                    var customerId = httpContext.User.Identity?.Name ??
                                    httpContext.Connection.RemoteIpAddress?.ToString() ??
                                    "anonymous";

                    return RateLimitPartition.GetTokenBucketLimiter(customerId, key =>
                        new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = rateLimitConfig.CustomerOrders.TokenLimit,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(rateLimitConfig.CustomerOrders.ReplenishmentPeriodMinutes),
                            TokensPerPeriod = rateLimitConfig.CustomerOrders.TokensPerPeriod,
                            AutoReplenishment = true,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = rateLimitConfig.CustomerOrders.QueueLimit
                        });
                });

                options.AddPolicy("customer-payment", httpContext =>
                {
                    var customerId = httpContext.User.Identity?.Name ??
                                    httpContext.Connection.RemoteIpAddress?.ToString() ??
                                    "anonymous";

                    return RateLimitPartition.GetConcurrencyLimiter(customerId, key =>
                        new ConcurrencyLimiterOptions
                        {
                            PermitLimit = rateLimitConfig.Payment.ConcurrentLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = rateLimitConfig.Payment.QueueLimit
                        });
                });

                options.AddPolicy("customer-profile", httpContext =>
                {
                    var customerId = httpContext.User.Identity?.Name ??
                                    httpContext.Connection.RemoteIpAddress?.ToString() ??
                                    "anonymous";

                    return RateLimitPartition.GetSlidingWindowLimiter(customerId, key =>
                        new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitConfig.CustomerProfile.PermitLimit,
                            Window = TimeSpan.FromMinutes(rateLimitConfig.CustomerProfile.WindowMinutes),
                            SegmentsPerWindow = rateLimitConfig.CustomerProfile.SegmentsPerWindow,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = rateLimitConfig.CustomerProfile.QueueLimit
                        });
                });

                options.AddPolicy("customer-wishlist", httpContext =>
                {
                    var customerId = httpContext.User.Identity?.Name ??
                                    httpContext.Connection.RemoteIpAddress?.ToString() ??
                                    "anonymous";

                    return RateLimitPartition.GetTokenBucketLimiter(customerId, key =>
                        new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = rateLimitConfig.CustomerWishlist.TokenLimit,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(rateLimitConfig.CustomerWishlist.ReplenishmentPeriodMinutes),
                            TokensPerPeriod = rateLimitConfig.CustomerWishlist.TokensPerPeriod,
                            AutoReplenishment = true,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = rateLimitConfig.CustomerWishlist.QueueLimit
                        });
                });

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    if (ipAddress == "::1" || ipAddress == "127.0.0.1" || ipAddress.StartsWith("192.168."))
                    {
                        return RateLimitPartition.GetNoLimiter("localhost");
                    }

                    return RateLimitPartition.GetTokenBucketLimiter(ipAddress, key =>
                        new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = rateLimitConfig.Global.TokenLimit,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(rateLimitConfig.Global.ReplenishmentPeriodMinutes),
                            TokensPerPeriod = rateLimitConfig.Global.TokensPerPeriod,
                            AutoReplenishment = true,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = rateLimitConfig.Global.QueueLimit
                        });
                });
            });

            return services;
        }
    }
}
