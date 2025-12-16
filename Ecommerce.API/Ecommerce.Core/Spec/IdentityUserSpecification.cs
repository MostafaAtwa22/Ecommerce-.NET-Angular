using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Params;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Core.Spec
{
    public class IdentityUserSpecification
    {
        public static async Task<IQueryable<ApplicationUser>> ApplyAsync(
            IQueryable<ApplicationUser> query,
            UserSpecParams specParams,
            UserManager<ApplicationUser> userManager,
            bool forCount = false)
        {
            if (!string.IsNullOrEmpty(specParams.Search))
            {
                query = query.Where(u =>
                    u.UserName!.ToLower().Contains(specParams.Search.ToLower()) ||
                    u.Email!.ToLower().Contains(specParams.Search.ToLower()));
            }

            if (!string.IsNullOrEmpty(specParams.Role))
            {
                var usersInRole = await userManager.GetUsersInRoleAsync(specParams.Role);
                var userIds = usersInRole.Select(u => u.Id).ToHashSet();

                query = query.Where(u => userIds.Contains(u.Id));
            }

            if (!forCount)
            {
                query = specParams.Sort switch
                {
                    "NameAsc"   => query.OrderBy(u => u.UserName),
                    "NameDesc"  => query.OrderByDescending(u => u.UserName),
                    "EmailAsc"  => query.OrderBy(u => u.Email),
                    "EmailDesc" => query.OrderByDescending(u => u.Email),
                    _           => query.OrderBy(u => u.UserName)
                };

                query = query
                    .Skip((specParams.PageIndex - 1) * specParams.PageSize)
                    .Take(specParams.PageSize);
            }

            return query;
        }
    }
}
