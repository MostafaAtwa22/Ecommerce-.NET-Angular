
namespace Ecommerce.API.Helpers.Resolver
{
    public class OnlineUserRolesResolver : IValueResolver<ApplicationUser, OnlineUserDto, IList<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public OnlineUserRolesResolver(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IList<string> Resolve(ApplicationUser source, OnlineUserDto destination, IList<string> destMember, ResolutionContext context)
        {
            return _userManager.GetRolesAsync(source).Result;
        }
    }
}
