
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
