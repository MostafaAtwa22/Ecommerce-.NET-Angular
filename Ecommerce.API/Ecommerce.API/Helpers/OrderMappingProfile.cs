using Ecommerce.API.Helpers.Resolver;

namespace Ecommerce.API.Helpers
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<OrderItem, OrderItemResponseDto>()
                .ForMember(dest => dest.ProductItemId, o => o.MapFrom(src => src.ProductItemOrdered.ProductItemId))
                .ForMember(dest => dest.ProductName, o => o.MapFrom(src => src.ProductItemOrdered.ProductName))
                .ForMember(dest => dest.PictureUrl,
                    o => o.MapFrom<ImageUrlResolver<OrderItem, OrderItemResponseDto>>());

            CreateMap<Order, OrderResponseDto>()
                .ForMember(dest => dest.DeliveryMethod, o => o.MapFrom(src => src.DeliveryMethod.ShortName))
                .ForMember(dest => dest.ShippingPrice, o => o.MapFrom(src => src.DeliveryMethod.Price))
                .ForMember(dest => dest.Total, o => o.MapFrom(src => src.GetTotal()))
                .ForMember(dest => dest.Status, o => o.MapFrom(src => src.Status.ToString()));

            CreateMap<Order, AllOrdersDto>()
                .ForMember(dest => dest.Total, o => o.MapFrom(src => src.GetTotal()))
                .ForMember(dest => dest.FirstName,
                    o => o.MapFrom(src => src.ApplicationUser.FirstName))
                .ForMember(dest => dest.LastName,
                    o => o.MapFrom(src => src.ApplicationUser.LastName))
                .ForMember(dest => dest.ProfilePictureUrl,
                    o => o.MapFrom<ImageUrlResolver<Order, AllOrdersDto>>());
        }
    }
}
