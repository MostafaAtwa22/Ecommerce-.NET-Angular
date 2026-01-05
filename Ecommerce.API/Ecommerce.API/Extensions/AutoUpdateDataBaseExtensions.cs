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
                // 1. Apply database migrations 
                var context = services.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();

                // 2. Seed Identity (Roles + Users + permissions)
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                await ApplicationIdentityDbContextSeed.SeedAsync(userManager, roleManager, loggerFactory);

                // 3. Seed main entities (Products, Categories, etc.)
                await ApplicationDbContextSeed.SeedAsync(context, loggerFactory);

                // 4. Resolve ProductService — REQUIRED for review seeding
                var productService = services.GetRequiredService<IProductService>();

                // 5. Seed product reviews (requires users + products + productService)
                await ApplicationDbContextSeed.SeedProductReviewsAsync(
                    context,
                    userManager,
                    productService,
                    loggerFactory
                );
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger(typeof(AutoUpdateDataBaseExtensions));
                logger.LogError(ex, "❌ An error occurred during database update or seeding.");
            }

            return app;
        }
    }
}
