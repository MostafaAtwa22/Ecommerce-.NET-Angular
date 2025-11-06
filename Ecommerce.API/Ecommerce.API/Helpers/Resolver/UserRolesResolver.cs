using AutoMapper;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.API.Helpers.Resolver
{
    public class UserRolesResolver : IValueResolver<ApplicationUser, UserDto, ICollection<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRolesResolver(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public ICollection<string> Resolve(ApplicationUser source, UserDto destination, ICollection<string> destMember, ResolutionContext context)
        {
            return _userManager.GetRolesAsync(source).Result;
        }
    }
}