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
            UserManager<ApplicationUser> userManager,
            IProductService productService,
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
                if (!context.ProductReviews.Any())
                {
                    // Get all customers
                    var customers = await userManager.GetUsersInRoleAsync(Role.Customer.ToString());
                    if (!customers.Any())
                    {
                        return;
                    }

                    var products = await context.Products.ToListAsync();
                    if (!products.Any())
                    {
                        return;
                    }

                    var random = new Random();

                    var reviews = new List<ProductReview>();

                    foreach (var customer in customers)
                    {
                        foreach (var product in products)
                        {
                            reviews.Add(new ProductReview
                            {
                                ProductId = product.Id,
                                ApplicationUserId = customer.Id,
                                Rating = random.Next(1, 6),
                                Comment = $"Review for {product.Name} by {customer.UserName}",
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }

                    await context.ProductReviews.AddRangeAsync(reviews);
                    await context.SaveChangesAsync();

                    foreach (var product in products)
                    {
                        await productService.UpdateProductRatingAsync(product.Id);
                    }

                }
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<ApplicationDbContext>();
                logger.LogError(ex, "An error happend will seeding");
            }
        }
    }
}