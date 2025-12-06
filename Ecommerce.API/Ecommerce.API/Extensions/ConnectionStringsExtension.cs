using Ecommerce.Core.Entities.Identity;
using Ecommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Ecommerce.API.Extensions
{
    public static class ConnectionStringsExtension
    {
        public static WebApplicationBuilder GetConnectionString(this WebApplicationBuilder builder)
        {
            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("There is no Connection String");

            builder.Services.AddDbContext<ApplicationDbContext>(opt =>
            {
                opt.UseSqlServer(connectionString, sql =>
                {
                    sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                });
            });

            builder.Services.AddSingleton<IConnectionMultiplexer>(c =>
            {
                var redis = builder.Configuration.GetConnectionString("Redis")
                    ?? throw new InvalidOperationException("There is no Connection String");
                var configuration = ConfigurationOptions.Parse(redis, true);
                
                return ConnectionMultiplexer.Connect(configuration);
            });
            return builder;
        }
    }
}