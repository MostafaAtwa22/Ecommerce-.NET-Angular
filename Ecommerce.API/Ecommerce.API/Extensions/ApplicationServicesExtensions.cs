using Ecommerce.API.BackgroundJobs;
using Ecommerce.API.Behaviours;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.API.Filters;
using Ecommerce.API.Helpers.Resolver;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Entities.orderAggregate;
using Ecommerce.Core.Interfaces;
using Ecommerce.Infrastructure.Repositories;
using Ecommerce.Infrastructure.Services;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped(typeof(IRedisRepository<>), typeof(RedisRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IGoogleService, GoogleService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProviderFilter>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandlerFilter>();
            services.AddHangfireServer();
            services.AddScoped<OrderBackgroundService>();
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
            services.AddScoped<IChatbotService, GroqChatbotService>();
            
            services.AddSingleton(provider =>
                new ImageUrlResolver<Product, ProductResponseDto>(
                    provider.GetRequiredService<IConfiguration>(),
                    "PictureUrl"));
            services.AddSingleton(provider =>
                new ImageUrlResolver<ApplicationUser, UserCommonDto>(
                    provider.GetRequiredService<IConfiguration>(),
                    "ProfilePictureUrl"));
            services.AddSingleton(provider =>
                new ImageUrlResolver<ApplicationUser, OnlineUserDto>(
                    provider.GetRequiredService<IConfiguration>(),
                    "ProfilePictureUrl"));
            services.AddSingleton(provider =>
                new ImageUrlResolver<OrderItem, OrderItemResponseDto>(
                    provider.GetRequiredService<IConfiguration>(),
                    "ProductItemOrdered.PictureUrl"));
            services.AddSingleton(provider =>
                new ImageUrlResolver<ProductReview, ProductReviewDto>(
                    provider.GetRequiredService<IConfiguration>(),
                    "ApplicationUser.ProfilePictureUrl"));
            services.AddSingleton(provider =>
                new ImageUrlResolver<Order, AllOrdersDto>(
                    provider.GetRequiredService<IConfiguration>(),
                    "ApplicationUser.ProfilePictureUrl"));

            services.AddSingleton<IResponseCacheService, ResponseCacheService>();
            
            // ✅ Add CORS policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:4200", "https://localhost:4200")
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
    }
}