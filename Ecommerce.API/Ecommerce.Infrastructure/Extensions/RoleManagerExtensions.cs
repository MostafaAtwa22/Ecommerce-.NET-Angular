using Ecommerce.Core.Constants;
using Ecommerce.Infrastructure.Constants;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Ecommerce.Infrastructure.Extensions
{
    public static class RoleManagerExtensions
    {
        public static async Task SeedClaimsForSuperAdmin(
            this RoleManager<IdentityRole> roleManager)
        {
            var superAdminRole = await roleManager.FindByNameAsync(Role.SuperAdmin.ToString());
            if (superAdminRole is null) return;

            var modules = Permissions.GetAllModules();
            foreach (var module in modules)
            {
                await roleManager.AddPermissionsClaims(superAdminRole, module);
            }
        }

        public static async Task AddPermissionsClaims(
            this RoleManager<IdentityRole> roleManager,
            IdentityRole role,
            string module)
        {
            var allClaims = await roleManager.GetClaimsAsync(role);
            var allPermissions = Permissions.GeneratePermissionsList(module);

            foreach (var permission in allPermissions)
            {
                if (!allClaims.Any(c => c.Type == Permissions.ClaimType && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission));
                }
            }
        }

        public static async Task SeedClaimsForAdmin(
            this RoleManager<IdentityRole> roleManager)
        {
            var adminRole = await roleManager.FindByNameAsync(Role.Admin.ToString());
            if (adminRole is null) return;

            var modules = Permissions.GetAllModules();

            var excludedModules = new List<string>
            {
                Modules.Roles
            };

            foreach (var module in modules)
            {
                if (excludedModules.Contains(module))
                    continue;

                var permissions = Permissions.GeneratePermissionsList(module);

                foreach (var permission in permissions)
                {
                    if (module == Modules.Account &&
                    (permission.EndsWith(CRUD.Create) || permission.EndsWith(CRUD.Update)))
                        continue;

                    var claims = await roleManager.GetClaimsAsync(adminRole);

                    if (!claims.Any(c =>
                        c.Type == Permissions.ClaimType &&
                        c.Value == permission))
                    {
                        await roleManager.AddClaimAsync(
                            adminRole,
                            new Claim(Permissions.ClaimType, permission));
                    }
                }
            }
        }

        public static async Task SeedClaimsForCustomer(
            this RoleManager<IdentityRole> roleManager)
        {
            var customerRole = await roleManager.FindByNameAsync(Role.Customer.ToString());
            if (customerRole is null) return;

            var claims = await roleManager.GetClaimsAsync(customerRole);

            void AddIfNotExists(string permission)
            {
                if (!claims.Any(c =>
                    c.Type == Permissions.ClaimType &&
                    c.Value == permission))
                {
                    roleManager.AddClaimAsync(
                        customerRole,
                        new Claim(Permissions.ClaimType, permission)
                    ).Wait();
                }
            }

            var readOnlyModules = new List<string>
            {
                Modules.Products,
                Modules.ProductBrands,
                Modules.ProductTypes,
                Modules.ProductReviews,
                Modules.DeliveryMethods
            };

            foreach (var module in readOnlyModules)
            {
                AddIfNotExists($"{Permissions.ClaimValue}.{module}.{CRUD.Read}");
            }

            foreach (var perm in Permissions.GeneratePermissionsList(Modules.Baskets))
                AddIfNotExists(perm);

            foreach (var perm in Permissions.GeneratePermissionsList(Modules.WishLists))
                AddIfNotExists(perm);

            AddIfNotExists($"{Permissions.ClaimValue}.{Modules.Orders}.{CRUD.Create}");
            AddIfNotExists($"{Permissions.ClaimValue}.{Modules.Orders}.{CRUD.Read}");

            AddIfNotExists($"{Permissions.ClaimValue}.{Modules.Profile}.{CRUD.Read}");
            AddIfNotExists($"{Permissions.ClaimValue}.{Modules.Profile}.{CRUD.Update}");
        }
    }
}