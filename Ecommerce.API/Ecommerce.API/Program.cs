using Ecommerce.API.Errors;
using Ecommerce.API.Extensions;
using Ecommerce.API.Helpers;
using Ecommerce.API.Middlewares;
using Ecommerce.Core.Interfaces;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
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

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            await app.AutoUpdateDataBaseAsync();

            app.UseMiddleware<ExceptionMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Middleware to handle the excption of non-exists endpoint (404 Not found)
            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
