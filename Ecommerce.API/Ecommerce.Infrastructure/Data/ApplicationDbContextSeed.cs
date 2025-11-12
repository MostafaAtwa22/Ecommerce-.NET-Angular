using System.Text.Json;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Entities.orderAggregate;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Data
{
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedAsync (ApplicationDbContext context, ILoggerFactory loggerFactory) 
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
    }
}