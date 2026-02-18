using Ecommerce.Infrastructure.Data.Interceptions;
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
                opt.UseSqlServer(connectionString)
                    .AddInterceptors(new SoftDeleteInterceptor());;
            });

            builder.Services.AddSingleton<IConnectionMultiplexer>(c =>
            {
                var redis = builder.Configuration.GetConnectionString("Redis")
                    ?? throw new InvalidOperationException("There is no Connection String");
                var configuration = ConfigurationOptions.Parse(redis, true);
                
                return ConnectionMultiplexer.Connect(configuration);
            });

            builder.Services.AddHangfire(config =>
            {
                config
                    .UseSqlServerStorage(connectionString)
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings();
            });
            return builder;
        }
    }
}
