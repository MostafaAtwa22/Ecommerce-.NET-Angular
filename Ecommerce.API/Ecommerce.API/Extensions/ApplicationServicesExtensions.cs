using Ecommerce.API.Behaviours;
using Ecommerce.API.Filters;
using Ecommerce.API.Helpers.Resolver;
using Ecommerce.Infrastructure.Repositories;
using MediatR;

namespace Ecommerce.API.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Open Generics
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped(typeof(IRedisRepository<>), typeof(RedisRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Automated Registration with Scrutor
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(IUnitOfWork), typeof(UnitOfWork), typeof(OrderService))
                .AddClasses(classes => classes.Where(type => 
                    (type.Name.EndsWith("Service") || type.Name.EndsWith("Repository")) 
                    && !type.Name.Contains("Generic") && !type.Name.Contains("Redis")))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProviderFilter>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandlerFilter>();
            
            services.AddHangfireServer();
            services.AddScoped<OrderBackgroundService>();
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));

            // Image URL Resolvers
            services.AddImageUrlResolvers();

            services.AddSingleton<IResponseCacheService, ResponseCacheService>();
            
            // ✅ Add CORS policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:4200", "https://localhost:4200", "http://tasaqolliui.runasp.net", "https://tasaqolliui.runasp.net")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // reconfigure the ApiController to handle validations
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = actionContext.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToArray();

                    var errorResponse = new ApiValidationErrorResponse
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(errorResponse);
                };
            });
            return services;
        }

        private static void AddImageUrlResolvers(this IServiceCollection services)
        {
            services.AddImageUrlResolver<Product, ProductResponseDto>("PictureUrl");
            services.AddImageUrlResolver<ApplicationUser, UserCommonDto>("ProfilePictureUrl");
            services.AddImageUrlResolver<ApplicationUser, OnlineUserDto>("ProfilePictureUrl");
            services.AddImageUrlResolver<OrderItem, OrderItemResponseDto>("ProductItemOrdered.PictureUrl");
            services.AddImageUrlResolver<ProductReview, ProductReviewDto>("ApplicationUser.ProfilePictureUrl");
            services.AddImageUrlResolver<Order, AllOrdersDto>("ApplicationUser.ProfilePictureUrl");
        }

        private static void AddImageUrlResolver<TSource, TDestination>(this IServiceCollection services, string propertyName)
            where TSource : class
            where TDestination : class
        {
            services.AddSingleton(provider =>
                new ImageUrlResolver<TSource, TDestination>(
                    provider.GetRequiredService<IConfiguration>(),
                    propertyName));
        }
    }
}
