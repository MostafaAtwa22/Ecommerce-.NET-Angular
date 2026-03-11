using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.Core.Entities;

namespace Ecommerce.API.Helpers
{
    public class CouponMappingProfile : Profile
    {
        public CouponMappingProfile()
        {
            CreateMap<CouponCreateDto, Coupon>();
            CreateMap<Coupon, CouponResponseDto>();
        }
    }
}
