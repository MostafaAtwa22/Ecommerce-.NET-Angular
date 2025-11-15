using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.Core.Entities.orderAggregate;

namespace Ecommerce.API.Helpers
{
public class DeliveryMethodMappingProfile : Profile
{
    public DeliveryMethodMappingProfile()
    {
        CreateMap<DeliveryMethod, DeliveryMethodResponseDto>();
        CreateMap<DeliveryMethodDto, DeliveryMethod>()
            .ReverseMap();
    }
}
}