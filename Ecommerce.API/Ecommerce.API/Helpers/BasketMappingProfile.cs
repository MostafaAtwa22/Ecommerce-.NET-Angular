using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.Core.Entities;

namespace Ecommerce.API.Helpers
{
    public class BasketMappingProfile : Profile
    {
        public BasketMappingProfile()
        {
            CreateMap<CustomerBasketDto, CustomerBasket>();
            CreateMap<BasketItemDto, BasketItem>();
        }
    }
}