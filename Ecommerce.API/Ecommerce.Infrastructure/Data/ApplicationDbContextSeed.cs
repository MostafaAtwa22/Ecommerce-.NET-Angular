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
                {
                    var brandsData =
                        await File.ReadAllTextAsync("../Ecommerce.Infrastructure/Seed/brands.json");
                    var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandsData);

                    foreach (var brand in brands!)
                        await context.ProductBrands.AddAsync(brand);
                    await context.SaveChangesAsync();
                }
                if (!context.ProductTypes.Any())
                {
                    var typesData =
                        await File.ReadAllTextAsync("../Ecommerce.Infrastructure/Seed/types.json");
                    var types = JsonSerializer.Deserialize<List<ProductType>>(typesData);

                    foreach (var type in types!)
                        await context.ProductTypes.AddAsync(type);
                    await context.SaveChangesAsync();
                }
                if (!context.Products.Any())
                {
                    var productsData =
                        await File.ReadAllTextAsync("../Ecommerce.Infrastructure/Seed/products.json");
                    var products = JsonSerializer.Deserialize<List<Product>>(productsData);

                    foreach (var product in products!)
                        await context.Products.AddAsync(product);
                    await context.SaveChangesAsync();
                }
                if (!context.DeliveryMethods.Any())
                {
                    var DeliveryMethodsData =
                        await File.ReadAllTextAsync("../Ecommerce.Infrastructure/Seed/delivery.json");
                    var DeliveryMethods = JsonSerializer.Deserialize<List<DeliveryMethod>>(DeliveryMethodsData);

                    foreach (var DeliveryMethod in DeliveryMethods!)
                        await context.DeliveryMethods.AddAsync(DeliveryMethod);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<ApplicationDbContext>();
                logger.LogError(ex, "An error happend will seeding");
            }
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

                var customers = await userManager.GetUsersInRoleAsync(Role.Customer.ToString());
                if (!customers.Any())
                {
                    logger.LogWarning("⚠️ No customers found for seeding reviews.");
                    return;
                }

                var products = await context.Products
                    .Include(p => p.ProductReviews)
                    .ToListAsync();

                if (!products.Any())
                {
                    logger.LogWarning("⚠️ No products found for seeding reviews.");
                    return;
                }

                var random = new Random();
                var customerList = customers.ToList();

                foreach (var product in products)
                {
                    var existingReviews = product.ProductReviews.ToList();

                    // Target 3-5 reviews per product
                    int targetReviews = random.Next(3, 6);
                    int reviewsToAdd = targetReviews - existingReviews.Count;

                    // Add new reviews for customers who haven't reviewed this product yet
                    var newReviewCustomers = customerList
                        .Where(c => !existingReviews.Any(r => r.ApplicationUserId == c.Id))
                        .OrderBy(c => random.Next())
                        .Take(reviewsToAdd)
                        .ToList();

                    foreach (var customer in newReviewCustomers)
                    {
                        var review = new ProductReview
                        {
                            ProductId = product.Id,
                            ApplicationUserId = customer.Id,
                            Rating = random.Next(1, 6),
                            Comment = GetRandomReviewComment(product.Name, customer.UserName!),
                            Headline = GetRandomHeadline(product.Name),
                            HelpfulCount = 0,
                            NotHelpfulCount = 0,
                            CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30))
                        };

                        await context.ProductReviews.AddAsync(review);
                    }

                    // Optionally update existing reviews to add new fields if missing
                    foreach (var existingReview in existingReviews)
                    {
                        if (string.IsNullOrEmpty(existingReview.Headline))
                        {
                            existingReview.Headline = GetRandomHeadline(product.Name);
                        }

                        // Ensure counts are set
                        existingReview.HelpfulCount = existingReview.HelpfulCount;
                        existingReview.NotHelpfulCount = existingReview.NotHelpfulCount;

                        // Optional: update comment if missing
                        if (string.IsNullOrEmpty(existingReview.Comment))
                        {
                            existingReview.Comment = GetRandomReviewComment(product.Name,
                                customerList[random.Next(customerList.Count)].UserName!);
                        }
                    }
                }

                await context.SaveChangesAsync();
                logger.LogInformation("✅ Product reviews seeded/updated.");

                // Update product ratings
                logger.LogInformation("🔄 Updating product ratings...");
                foreach (var product in products)
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

        private static string GetRandomHeadline(string productName)
        {
            var headlines = new List<string>
            {
                $"Loved the {productName}!",
                $"Disappointed with the {productName}",
                $"{productName} is just okay",
                $"Highly recommend {productName}",
                $"Not worth buying {productName}",
                $"Excellent {productName} experience",
                $"Average {productName}, could improve",
                $"Fantastic value for {productName}"
            };

            var random = new Random();
            return headlines[random.Next(headlines.Count)];
        }
    }
}