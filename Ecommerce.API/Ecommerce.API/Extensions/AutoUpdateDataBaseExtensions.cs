using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Interfaces;
using Ecommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Extensions
{
    public static class AutoUpdateDataBaseExtensions
    {
        public static async Task<IApplicationBuilder> AutoUpdateDataBaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

            try
            {
                // Seed Identity database (users & roles)
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                await ApplicationIdentityDbContextSeed.SeedAsync(userManager, roleManager, loggerFactory);

                // Seed the main app database
                var context = services.GetRequiredService<ApplicationDbContext>();
                var productService = services.GetRequiredService<IProductService>();
                await context.Database.MigrateAsync();
                await ApplicationDbContextSeed.SeedAsync(context, userManager, productService, loggerFactory);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger(typeof(AutoUpdateDataBaseExtensions));
                logger.LogError(ex, "❌ An error occurred during database migration or seeding.");
            }

            return app;
        }
    }
}
