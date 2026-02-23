using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ecommerce.API.Extensions
{
    public static class HealthChecksExtensions
    {
        public static WebApplicationBuilder AddHealthCheckServices(this WebApplicationBuilder builder)
        {
            var sqlServerConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("There is no Connection String");

            var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("There is no Connection String");

            builder.Services.AddHealthChecks()
                .AddSqlServer(
                    redisConnectionString,
                    name: "SqlServer health check",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new [] {"sql", "sqlServer", "healthCheck"})
                .AddRedis(
                    sqlServerConnectionString,
                    name: "Redis health check",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new [] {"key-value databases", "redis", "healthCheck"})
                .AddHangfire(_ => { },
                tags: new [] {"3rd part", "hangefire", "healthCheck"});

            return builder;
        }
    }
}