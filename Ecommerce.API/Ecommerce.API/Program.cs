using Ecommerce.API.Extensions;
using Ecommerce.API.Helpers;
using Ecommerce.API.Middlewares;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;

namespace Ecommerce.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.GetConnectionString();

            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });            
            builder.Services.AddApplicationServices();

            builder.Services.AddIdentityServices(builder.Configuration);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerervices();

            var app = builder.Build();

            await app.AutoUpdateDataBaseAsync();

            app.UseMiddleware<ExceptionMiddleware>();

            // Swagger in development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Handle 404s
            app.UseStatusCodePagesWithReExecute("/errors/{0}");

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

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
