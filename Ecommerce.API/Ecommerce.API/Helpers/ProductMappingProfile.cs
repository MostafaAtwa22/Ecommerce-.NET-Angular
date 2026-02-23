using Ecommerce.API.Helpers.Resolver;

namespace Ecommerce.API.Helpers
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.ProductBrandName, o => o.MapFrom(src => src.ProductBrand.Name))
                .ForMember(dest => dest.ProductTypeName, o => o.MapFrom(src => src.ProductType.Name))
                .ForMember(dest => dest.ProductBrandId, o => o.MapFrom(src => src.ProductBrand.Id))
                .ForMember(dest => dest.ProductTypeId, o => o.MapFrom(src => src.ProductType.Id))
                .ForMember(dest => dest.PictureUrl,
                    o => o.MapFrom<ImageUrlResolver<Product, ProductResponseDto>>());

            CreateMap<ProductCreationDto, Product>();
            CreateMap<ProductUpdateDto, Product>();

            CreateMap<ProductBrand, ProductBrandAndTypeResponseDto>();
            CreateMap<ProductType, ProductBrandAndTypeResponseDto>();

            CreateMap<ProductBrandAndTypeCreationDto, ProductBrand>();
            CreateMap<ProductBrandAndTypeCreationDto, ProductType>();

            CreateMap<ProductReview, ProductReviewDto>()
                .ForMember(dest => dest.UserName,
                    o => o.MapFrom(src => src.ApplicationUser != null ? src.ApplicationUser.UserName : null))
                .ForMember(dest => dest.ProfilePictureUrl,
                    o => o.MapFrom<ImageUrlResolver<ProductReview, ProductReviewDto>>())
                .ForMember(dest => dest.FirstName,
                    o => o.MapFrom(src => src.ApplicationUser != null ? src.ApplicationUser.FirstName : null))
                .ForMember(dest => dest.LastName,
                    o => o.MapFrom(src => src.ApplicationUser != null ? src.ApplicationUser.LastName : null));

            CreateMap<ProductReviewFromDto, ProductReview>();
        }
    }
}
