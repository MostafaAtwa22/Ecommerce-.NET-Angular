using System.Text.Json;
using Ecommerce.Core.Constants;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Entities.orderAggregate;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Data
{
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedAsync(ApplicationDbContext context,
            ILoggerFactory loggerFactory)
        {
            try
            {
                if (!context.ProductBrands.Any())
                    await SeedProductBrandsAsync(context);
                if (!context.ProductTypes.Any())
                    await SeedProductTypesAsync(context);
                if (!context.Products.Any())
                    await SeedProductsAsync(context);
                if (!context.DeliveryMethods.Any())
                    await SeedDeliveryMethodsAsync(context);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<ApplicationDbContext>();
                logger.LogError(ex, "An error happend will seeding");
            }
        }
        private static async Task SeedProductBrandsAsync(ApplicationDbContext context)
        {
            var brandsData =
                await File.ReadAllTextAsync("../Ecommerce.Infrastructure/Seed/brands.json");
            var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandsData);

            foreach (var brand in brands!)
                await context.ProductBrands.AddAsync(brand);
            await context.SaveChangesAsync();
        }
        private static async Task SeedProductTypesAsync(ApplicationDbContext context)
        {
            var typesData =
                await File.ReadAllTextAsync("../Ecommerce.Infrastructure/Seed/types.json");
            var types = JsonSerializer.Deserialize<List<ProductType>>(typesData);

            foreach (var type in types!)
                await context.ProductTypes.AddAsync(type);
            await context.SaveChangesAsync();
        }
        private static async Task SeedProductsAsync(ApplicationDbContext context)
        {
            var productsData =
                await File.ReadAllTextAsync("../Ecommerce.Infrastructure/Seed/products.json");
            var products = JsonSerializer.Deserialize<List<Product>>(productsData);

            foreach (var product in products!)
                await context.Products.AddAsync(product);
            await context.SaveChangesAsync(); ;
        }
        private static async Task SeedDeliveryMethodsAsync(ApplicationDbContext context)
        {
            var DeliveryMethodsData =
                await File.ReadAllTextAsync("../Ecommerce.Infrastructure/Seed/delivery.json");
            var DeliveryMethods = JsonSerializer.Deserialize<List<DeliveryMethod>>(DeliveryMethodsData);

            foreach (var DeliveryMethod in DeliveryMethods!)
                await context.DeliveryMethods.AddAsync(DeliveryMethod);
            await context.SaveChangesAsync();
        }

        public static async Task SeedProductReviewsAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IProductService productService,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ApplicationDbContext>();
            try
            {
                logger.LogInformation("🔄 Checking for existing product reviews...");

                if (!await context.ProductReviews.AnyAsync())
                {
                    logger.LogInformation("🔄 Seeding product reviews...");

                    var customers = await userManager.GetUsersInRoleAsync(Role.Customer.ToString());
                    if (!customers.Any())
                    {
                        logger.LogWarning("⚠️ No customers found for seeding reviews.");
                        return;
                    }

                    var products = await context.Products.ToListAsync();
                    if (!products.Any())
                    {
                        logger.LogWarning("⚠️ No products found for seeding reviews.");
                        return;
                    }

                    var random = new Random();
                    var reviews = new List<ProductReview>();
                    var customerList = customers.ToList();
                    var productList = products.ToList();

                    // Create 3-5 reviews per product
                    foreach (var product in productList)
                    {
                        // Random number of reviews per product (3-5)
                        int numberOfReviews = random.Next(3, 6);

                        // Shuffle customers and take random ones for this product
                        var shuffledCustomers = customerList.OrderBy(c => random.Next()).Take(numberOfReviews).ToList();

                        foreach (var customer in shuffledCustomers)
                        {
                            reviews.Add(new ProductReview
                            {
                                ProductId = product.Id,
                                ApplicationUserId = customer.Id,
                                Rating = random.Next(1, 6), // Rating from 1-5
                                Comment = GetRandomReviewComment(product.Name, customer.UserName!),
                                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30)) // Random date within last 30 days
                            });
                        }
                    }

                    if (reviews.Any())
                    {
                        await context.ProductReviews.AddRangeAsync(reviews);
                        await context.SaveChangesAsync();
                        logger.LogInformation($"✅ Seeded {reviews.Count} product reviews.");

                        // Update product ratings
                        logger.LogInformation("🔄 Updating product ratings...");
                        foreach (var product in productList)
                        {
                            try
                            {
                                await productService.UpdateProductRatingAsync(product.Id);
                            }
                            catch (Exception ex)
                            {
                                logger.LogWarning($"⚠️ Failed to update rating for product {product.Id}: {ex.Message}");
                            }
                        }
                        logger.LogInformation("✅ Product ratings updated.");
                    }
                }
                else
                {
                    logger.LogInformation("✅ Product reviews already exist.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error occurred while seeding product reviews.");
                throw;
            }
        }

        private static string GetRandomReviewComment(string productName, string userName)
        {
            var comments = new List<string>
            {
                $"Great product! {productName} exceeded my expectations.",
                $"Good quality for the price. I'm satisfied with my purchase of {productName}.",
                $"Could be better. The {productName} has some room for improvement.",
                $"Excellent product! Highly recommend {productName} to everyone.",
                $"Decent product, but shipping took longer than expected.",
                $"Love it! The {productName} works perfectly for my needs.",
                $"Average product. Nothing special about the {productName}.",
                $"Fantastic purchase! The {productName} is worth every penny.",
                $"Not what I expected. The {productName} has some issues.",
                $"Perfect! Exactly what I needed. {productName} is awesome!"
            };

            var random = new Random();
            return comments[random.Next(comments.Count)];
        }

    }
}