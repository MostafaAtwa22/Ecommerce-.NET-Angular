using Ecommerce.API.Extensions;
using Ecommerce.API.Hubs;
using Ecommerce.API.Middlewares;
using Ecommerce.API.Options;
using Ecommerce.Infrastructure.Settings;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Ecommerce.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.GetConnectionString();
            builder.AddHealthCheckServices();

            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });
            builder.Services.AddApplicationServices();

            builder.Services.Configure<RequestTimingOptions>(
                builder.Configuration.GetSection("RequestTiming"));

            builder.Services.Configure<MailSettings>(
                builder.Configuration.GetSection("MailSettings"));
            
            builder.Services.Configure<SecurityStampValidatorOptions>(opt =>
            {
                opt.ValidationInterval = TimeSpan.Zero;
            });
            builder.Services.AddHttpClient();
            builder.Services.AddIdentityServices(builder.Configuration);
            
            builder.Services.AddSignalR();
            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerervices();
            builder.Services.AddCustomRateLimiting(builder.Configuration);
            builder.Services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
            builder.Host.UseSerilog((context, config) =>
            {
                config
                    .ReadFrom.Configuration(context.Configuration)
                    .MinimumLevel.Information();
            });
            var app = builder.Build();

            await app.AutoUpdateDataBaseAsync();

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseRequestTimingMiddleware();

            // Swagger in development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Handle 404s
            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            // allow to log http requests
            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseCors("AllowAngularApp");

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "http://localhost:4200");
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
                }
            });

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseAuthentication();
            app.UseRateLimiter();
            app.UseAuthorization();

            app.MapControllers();
            
            app.MapHub<ChatHub>("hubs/chat");
            
            app.UseHangfireJobs();

            app.Run();
        }
    }
}
