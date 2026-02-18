
namespace Ecommerce.API.Extensions
{
    public static class UserManagerExtensions
    {
        public static async Task<ApplicationUser?> FindUserByClaimsPrinciplesWithAddressAsync(
            this UserManager<ApplicationUser> userManager,
            ClaimsPrincipal user)
        {
            var email = user.RetrieveEmailFromPrincipal();

            return await userManager.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public static async Task<ApplicationUser?> FindUserByClaimPrinciplesAsync(
            this UserManager<ApplicationUser> userManager,
            ClaimsPrincipal user)
        {
            var email = user.RetrieveEmailFromPrincipal();

            return await userManager.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
