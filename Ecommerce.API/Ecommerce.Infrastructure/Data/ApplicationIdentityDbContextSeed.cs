using System.Text.Json.Serialization;
using Ecommerce.Infrastructure.Extensions;

namespace Ecommerce.Infrastructure.Data
{
    public class ApplicationIdentityDbContextSeed
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = {
                new JsonStringEnumConverter()
            }
        };

        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ApplicationIdentityDbContextSeed>();

            await SeedRolesAsync(roleManager, logger);
            await SeedSuperAdminUserAsync(roleManager, logger);
            await SeedAdminRoleAsync(roleManager, logger);
            await SeedCustomerRoleAsync(roleManager, logger);
            await SeedUsersAsync(userManager, roleManager, logger);
        }

        private static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager, 
            ILogger logger)
        {
            try
            {
                var roles = new List<string>
                {
                    Role.SuperAdmin.ToString(),
                    Role.Admin.ToString(),
                    Role.Customer.ToString()
                };

                foreach (var roleName in roles)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var result = await roleManager.CreateAsync(new IdentityRole
                        {
                            Name = roleName,
                            NormalizedName = roleName.ToUpper()
                        });

                        if (result.Succeeded)
                            logger.LogInformation($"✅ Role '{roleName}' created.");
                        else
                            logger.LogWarning($"⚠️ Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error occurred while seeding roles.");
            }
        }

        private static async Task SeedUsersAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger logger)
        {
            try
            {
                if (userManager.Users.Any())
                    return;

                var usersData = await File.ReadAllTextAsync("../Ecommerce.Infrastructure/Seed/users.json");
                using var doc = JsonDocument.Parse(usersData);
                var usersArray = doc.RootElement.EnumerateArray();

                foreach (var element in usersArray)
                {
                    var userJson = element.GetRawText();
                    var user = JsonSerializer.Deserialize<ApplicationUser>(userJson, _jsonOptions);
                    if (user == null) continue;

                    user.EmailConfirmed = true;
                    user.NormalizedUserName = user.UserName!.ToUpper();
                    user.NormalizedEmail = user.Email!.ToUpper();
                    user.SecurityStamp = Guid.NewGuid().ToString();

                    var result = await userManager.CreateAsync(user, "P@ssw0rd123!");
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                            logger.LogError($"❌ Error seeding user {user.UserName}: {error.Description}");
                        continue;
                    }

                    if (element.TryGetProperty("Role", out var roleProp))
                    {
                        var roleName = roleProp.GetString();
                        if (!string.IsNullOrWhiteSpace(roleName))
                        {
                            if (!await roleManager.RoleExistsAsync(roleName))
                            {
                                await roleManager.CreateAsync(new IdentityRole
                                {
                                    Name = roleName,
                                    NormalizedName = roleName.ToUpper()
                                });
                            }

                            await userManager.AddToRoleAsync(user, roleName);
                        }
                    }

                    logger.LogInformation($"✅ User '{user.UserName}' created successfully.");
                }

                logger.LogInformation("🎉 User seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ An error occurred while seeding users.");
            }
        }

        private static async Task SeedSuperAdminUserAsync(
            RoleManager<IdentityRole> roleManager,
            ILogger logger)
        {
            try
            {
                logger.LogInformation("Starting Super Admin role seeding...");

                if (roleManager == null)
                {
                    logger.LogWarning("RoleManager is null. Skipping Super Admin seeding.");
                    return;
                }

                var superAdminRole = await roleManager.FindByNameAsync(Role.SuperAdmin.ToString());
                if (superAdminRole == null)
                {
                    logger.LogWarning("SuperAdmin role not found. Skipping seeding.");
                    return;
                }

                var claims = await roleManager.GetClaimsAsync(superAdminRole);

                if (!claims.Any())
                {
                    await roleManager.SeedClaimsForSuperAdmin();
                    logger.LogInformation("Super Admin role seeding completed successfully.");
                }
                else
                {
                    logger.LogInformation("Super Admin role already has claims. Skipping seeding.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding Super admin permissions.");
            }
        }

        private static async Task SeedAdminRoleAsync(
            RoleManager<IdentityRole> roleManager,
            ILogger logger)
        {
            try
            {
                var adminRole = await roleManager.FindByNameAsync(Role.Admin.ToString());
                if (adminRole == null)
                {
                    logger.LogWarning("Admin role not found.");
                    return;
                }

                var claims = await roleManager.GetClaimsAsync(adminRole);

                if (!claims.Any())
                {
                    await roleManager.SeedClaimsForAdmin();
                    logger.LogInformation("✅ Admin permissions seeded successfully.");
                }
                else
                {
                    logger.LogInformation("ℹ️ Admin role already has claims.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error while seeding Admin permissions.");
            }
        }

        private static async Task SeedCustomerRoleAsync(
            RoleManager<IdentityRole> roleManager,
            ILogger logger)
        {
            try
            {
                var customerRole = await roleManager.FindByNameAsync(Role.Customer.ToString());
                if (customerRole == null)
                {
                    logger.LogWarning("Customer role not found.");
                    return;
                }

                var claims = await roleManager.GetClaimsAsync(customerRole);

                if (!claims.Any())
                {
                    await roleManager.SeedClaimsForCustomer();
                    logger.LogInformation("✅ Customer permissions seeded successfully.");
                }
                else
                {
                    logger.LogInformation("ℹ️ Customer role already has claims.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error while seeding Customer permissions.");
            }
        }
    }
}
