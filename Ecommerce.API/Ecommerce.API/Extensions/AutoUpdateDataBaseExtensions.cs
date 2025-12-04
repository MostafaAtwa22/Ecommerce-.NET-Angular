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
                // 1. Migrate the database first
                var context = services.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();

                // 2. Seed Identity (users & roles) - needs to happen before product reviews
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                await ApplicationIdentityDbContextSeed.SeedAsync(userManager, roleManager, loggerFactory);

                // 3. Seed main product data
                await ApplicationDbContextSeed.SeedAsync(context, loggerFactory);

                // 4. Seed product reviews (requires users and products to exist)
                var productService = services.GetRequiredService<IProductService>();
                await ApplicationDbContextSeed.SeedProductReviewsAsync(context, userManager, productService, loggerFactory);

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
