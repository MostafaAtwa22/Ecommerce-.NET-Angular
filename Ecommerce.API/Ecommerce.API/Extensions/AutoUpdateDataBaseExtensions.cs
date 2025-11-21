using Ecommerce.Core.Entities.Identity;
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
                // Seed the main app database
                var context = services.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();
                await ApplicationDbContextSeed.SeedAsync(context, loggerFactory);

                // Seed Identity database (users & roles)
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                await ApplicationIdentityDbContextSeed.SeedAsync(userManager, roleManager, loggerFactory);
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
