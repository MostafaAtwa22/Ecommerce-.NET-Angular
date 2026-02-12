using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Helpers.Resolver;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.googleDto;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.API.Helpers
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<RegisterDto, ApplicationUser>();

            CreateMap<GoogleUserDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, o => o.MapFrom(src => src.Email))
                .ForMember(dest => dest.NormalizedUserName, o => o.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.NormalizedEmail, o => o.MapFrom(src => src.Email.ToUpper()));

            CreateMap<ApplicationUser, UserCommonDto>()
                .ForMember(dest => dest.Gender, o => o.MapFrom(src => src.Gender.ToString()))
                .ForMember(dest => dest.ProfilePicture,
                    o => o.MapFrom<ImageUrlResolver<ApplicationUser, UserCommonDto>>())
                    .ForMember(dest => dest.Roles, o => o.MapFrom<UserRolesResolver>());

            CreateMap<ApplicationUser, UserDto>()
                .IncludeBase<ApplicationUser, UserCommonDto>();

            CreateMap<ApplicationUser, ProfileResponseDto>()
                .IncludeBase<ApplicationUser, UserCommonDto>()
                .ForMember(dest => dest.IsLocked,
                    opt => opt.MapFrom(src =>
                        src.LockoutEnabled && src.LockoutEnd.HasValue && src.LockoutEnd > DateTimeOffset.UtcNow
                    ));

            CreateMap<ApplicationUser, ProfileUpdateDto>()
                .ForMember(dest => dest.Gender, o => o.MapFrom(src => src.Gender));
            CreateMap<ProfileUpdateDto, ApplicationUser>()
                .ForMember(dest => dest.Gender, o => o.MapFrom(src => src.Gender))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember is not null));

            CreateMap<ApplicationUser, OnlineUserDto>()
                .ForMember(dest => dest.ProfilePictureUrl, o => o.MapFrom<ImageUrlResolver<ApplicationUser, OnlineUserDto>>())
                .ForMember(dest => dest.PhoneNumber, o => o.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Gender, o => o.MapFrom(src => src.Gender.ToString()))
                .ForMember(dest => dest.Roles, o => o.MapFrom<OnlineUserRolesResolver>());
        }
    }
}