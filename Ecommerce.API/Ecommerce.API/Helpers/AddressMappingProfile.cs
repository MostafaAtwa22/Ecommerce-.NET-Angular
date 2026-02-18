
namespace Ecommerce.API.Helpers
{
    public class AddressMappingProfile : Profile
    {
        public AddressMappingProfile()
        {
            CreateMap<Address, AddressDto>().ReverseMap();
            CreateMap<OrderAddressDto, OrderAddress>().ReverseMap();
        }
    }
}
