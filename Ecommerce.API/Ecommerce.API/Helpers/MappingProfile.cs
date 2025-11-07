using AutoMapper;
using Ecommerce.API.Dtos;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Helpers.Resolver;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.API.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.ProductBrandName, o => o.MapFrom(src => src.ProductBrand.Name))
                .ForMember(dest => dest.ProductTypeName, o => o.MapFrom(src => src.ProductType.Name))
                .ForMember(dest => dest.PictureUrl, o => o.MapFrom<ProductUrlResolver>());

            CreateMap<ProductCreationDto, Product>();
            CreateMap<ProductUpdateDto, Product>();

            CreateMap<ProductBrand, ProductBrandAndTypeResponseDto>();
            CreateMap<ProductType, ProductBrandAndTypeResponseDto>();

            CreateMap<ProductBrandAndTypeCreationDto, ProductBrand>();
            CreateMap<ProductBrandAndTypeCreationDto, ProductType>();

            CreateMap<RegisterDto, ApplicationUser>();

            CreateMap<ApplicationUser, UserCommonDto>()
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom<UserUrlResolver>());
            
            CreateMap<ApplicationUser, UserDto>()
                .IncludeBase<ApplicationUser, UserCommonDto>()
                .ForMember(dest => dest.Roles, o => o.MapFrom<UserRolesResolver>());

            CreateMap<ApplicationUser, ProfileResponseDto>()
                .IncludeBase<ApplicationUser, UserCommonDto>()
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()));

            CreateMap<Address, AddressDto>()
                .ReverseMap();

            CreateMap<IdentityRole, RoleDto>();

            CreateMap<RoleToCreateDto, IdentityRole>();

            CreateMap<CustomerBasketDto, CustomerBasket>();
            CreateMap<BasketItemDto, BasketItem>();
        }
    }
}