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

            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddControllers();
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

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
