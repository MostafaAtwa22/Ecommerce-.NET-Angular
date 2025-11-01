using Ecommerce.API.Extensions;
using Ecommerce.API.Helpers;
using Ecommerce.API.Middlewares;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.GetConnectionString();

            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddControllers();
            builder.Services.AddApplicationServices();

            // ✅ Add CORS policy
            builder.Services.AddCors(options =>
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


            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
