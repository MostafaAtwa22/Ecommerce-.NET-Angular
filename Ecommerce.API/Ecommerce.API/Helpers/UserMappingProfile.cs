using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Helpers.Resolver;
using Ecommerce.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.API.Helpers
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<RegisterDto, ApplicationUser>();

            CreateMap<ApplicationUser, UserCommonDto>()
                .ForMember(dest => dest.Gender, o => o.MapFrom(src => src.Gender.ToString()))
                .ForMember(dest => dest.ProfilePicture,
                    o => o.MapFrom<ImageUrlResolver<ApplicationUser, UserCommonDto>>());

            CreateMap<ApplicationUser, UserDto>()
                .IncludeBase<ApplicationUser, UserCommonDto>()
                .ForMember(dest => dest.Roles, o => o.MapFrom<UserRolesResolver>());

            CreateMap<ApplicationUser, ProfileResponseDto>()
                .IncludeBase<ApplicationUser, UserCommonDto>();
                
            CreateMap<ApplicationUser, ProfileUpdateDto>()
                .ForMember(dest => dest.Gender, o => o.MapFrom(src => src.Gender));
            CreateMap<ProfileUpdateDto, ApplicationUser>()
                .ForMember(dest => dest.Gender, o => o.MapFrom(src => src.Gender))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember is not null));

            CreateMap<IdentityRole, RoleDto>();
            CreateMap<RoleToCreateDto, IdentityRole>();
        }
    }
}