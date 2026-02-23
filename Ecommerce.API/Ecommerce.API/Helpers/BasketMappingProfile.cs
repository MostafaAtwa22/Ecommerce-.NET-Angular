
namespace Ecommerce.API.Helpers
{
    public class BasketMappingProfile : Profile
    {
        public BasketMappingProfile()
        {
            CreateMap<CommonItemDto, CommonItem>();

            CreateMap<CustomerBasketDto, CustomerBasket>();
            CreateMap<BasketItemDto, BasketItem>()
                .IncludeBase<CommonItemDto, CommonItem>();

            CreateMap<CustomerWishListDto, CustomerWishList>();
            CreateMap<WishListItemDto, WishListItem>()
                .IncludeBase<CommonItemDto, CommonItem>();
        }
    }
}
