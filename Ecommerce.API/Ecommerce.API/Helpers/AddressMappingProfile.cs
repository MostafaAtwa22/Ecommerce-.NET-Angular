using AutoMapper;
using Ecommerce.API.Dtos;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Entities.orderAggregate;

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