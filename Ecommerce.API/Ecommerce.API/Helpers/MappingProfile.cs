using AutoMapper;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Helpers.Resolver;
using Ecommerce.Core.Entities;

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
        }
    }
}