using AutoMapper;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.Core.Entities.Identity;

namespace Ecommerce.API.Helpers.Resolver
{
    public class UserUrlResolver : IValueResolver<ApplicationUser, UserCommonDto, string>
    {
        private readonly IConfiguration _config;

        public UserUrlResolver(IConfiguration config)
        {
            _config = config;
        }
        
        public string Resolve(ApplicationUser source, UserCommonDto destination,
            string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.ProfilePictureUrl))
                return $"{_config["ApiUrl"]}/{source.ProfilePictureUrl}";
            return null!;
        }
    }
}